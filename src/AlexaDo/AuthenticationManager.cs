﻿//******************************************************************************************************
//  AuthenticationManager.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AlexaDoPlugin;
using GSF;
using GSF.Configuration;
using log4net;

namespace AlexaDo
{
    /// <summary>
    /// Provides Amazon Echo authentication handling.
    /// </summary>
    /// <threading>
    /// This class is dependent on the main form for an active WebBrowser control and feedback
    /// processing, this means that methods will run on the main thread as invoked from primary
    /// message loop; as a result this class is not safe for multi-threaded access.
    /// </threading>
    public class AuthenticationManager : IDisposable
    {
        #region [ Members ]

        // Fields
        private readonly EchoMonitor m_echoMonitor;
        private readonly CategorizedSettingsElementCollection m_userSettings;
        private SHDocVw.WebBrowser m_activeXReference;
        private Dictionary<string, string> m_lastPostData;
        private bool m_navigationComplete;
        private int m_automatedLoginAttempts;
        private bool m_applicationClosing;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AuthenticationManager"/> instance.
        /// </summary>
        /// <param name="echoMonitor">Parent <see cref="EchoMonitor"/> form with a WebBrowser control.</param>
        public AuthenticationManager(EchoMonitor echoMonitor)
        {
            m_echoMonitor = echoMonitor;
            m_lastPostData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Make sure default user settings exist
            m_userSettings = ConfigurationFile.Current.Settings["user"];
            m_userSettings.Add("UserName", "user-setting", "User name to use when authenticating", true, SettingScope.User);
            m_userSettings.Add("Password", "user-setting", "Password to use when authenticating", true, SettingScope.User);

            // Create a new script interface to detect new Echo activities
            ScriptInterface scriptInterface = new ScriptInterface();
            scriptInterface.ReceivedEchoActivity += scriptInterface_ReceivedEchoActivity;
            m_echoMonitor.AuthenticationBrowser.ObjectForScripting = scriptInterface;

            // Attach to needed events
            m_echoMonitor.FormClosing += m_echoMonitor_FormClosing;
            m_echoMonitor.AuthenticationBrowser.DocumentCompleted += BrowserControl_DocumentCompleted;

            try
            {
                // Reference underlying IE based ActiveX control so we can attach to native events
                m_activeXReference = (SHDocVw.WebBrowser)m_echoMonitor.AuthenticationBrowser.ActiveXInstance;

                // BrowserControl_BeforeNavigate2 event provides headers and post data
                m_activeXReference.BeforeNavigate2 += BrowserControl_BeforeNavigate2;
            }
            catch (Exception ex)
            {
                // Any possible failure here would not be fatal to overall operation
                Log.Error($"Failed to register for low level navigation event, login credentials will not be cached: {ex.Message}");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AuthenticationManager"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AuthenticationManager"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // Detach from form events
                        m_echoMonitor.FormClosing -= m_echoMonitor_FormClosing;
                        m_echoMonitor.AuthenticationBrowser.DocumentCompleted -= BrowserControl_DocumentCompleted;

                        // Dereference ActiveX control
                        if ((object)m_activeXReference != null)
                        {
                            m_activeXReference.BeforeNavigate2 -= BrowserControl_BeforeNavigate2;
                            m_activeXReference = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Navigates to a URL and waits until document is loaded.
        /// </summary>
        /// <param name="url">URL to navigate to.</param>
        /// <param name="scrollToBottom">Set to <c>true</c> to scroll to bottom of page when load is complete.</param>
        public void Navigate(string url, bool scrollToBottom = false)
        {
            m_navigationComplete = false;
            m_echoMonitor.AuthenticationBrowser.Navigate(url, null, null, "User-Agent: " + Settings.UserAgent);

            while (!m_navigationComplete && !m_applicationClosing)
                Application.DoEvents();

            if (!scrollToBottom)
                return;

            // Scroll browser window to bring login screen into full view
            HtmlDocument doc = m_echoMonitor.AuthenticationBrowser.Document;

            if ((object)doc != null && (object)doc.Window != null && (object)doc.Body != null)
                doc.Window.ScrollTo(0, doc.Body.ScrollRectangle.Height);
        }

        /// <summary>
        /// Authenticates with Amazon Echo web application.
        /// </summary>
        /// <param name="requestCredentials">Set to <c>true</c> to request new user credentials.</param>
        public void Authenticate(bool requestCredentials = false)
        {
            try
            {
                // Reset global authentication state
                Settings.Authenticated = false;

                m_echoMonitor.UpdateStatus("Attempting Amazon Echo authentication...");

                // Had to resort to using WebBrowser control to automate Amazon authentication, initial
                // attempts to use WebClient failed even after cloning all hidden inputs and adding email,
                // password and create parameters. Speculating that JavaScript is needed in order to
                // dynamically add other hidden inputs, e.g., "metadata1" that do not exist in base form
                // HTML - perhaps someone else will have more time / better luck, but the following seems
                // reliable. Since a web form is being used, decided to take advantage of it and just use
                // the web page to get user to authenticate, then cache the credentials for future loads.
                Navigate(Settings.BaseURL, true);

                HtmlDocument doc = m_echoMonitor.AuthenticationBrowser.Document;

                if ((object)doc == null)
                    return;

                string userName = null, password = null;
                bool manualLogin = false;

                if (requestCredentials)
                {
                    // Logout, if currently logged in
                    Navigate(Settings.BaseURL + "/logout", true);
                }
                else
                {
                    try
                    {
                        userName = m_userSettings["UserName"].Value;
                        password = m_userSettings["Password"].Value;
                    }
                    catch
                    {
                        // We just clear the cache for any failures to get credentials, e.g, failure to decrypt cached credentials
                        ClearCredentials();
                        userName = "";
                        password = "";
                    }
                }

                // Next action will navigate, reset wait flag
                m_navigationComplete = false;

                while (!m_navigationComplete && !m_applicationClosing)
                {
                    // See if cached user credentials exist
                    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                    {
                        // Make sure window is visible, we need user to login
                        m_echoMonitor.ShowWindow();
                        ShowNotification("Enter credentials to login to Amazon Echo...", ToolTipIcon.Info, forceDisplay: true);
                        manualLogin = true;
                    }
                    else
                    {
                        // Have existing credentials, attempt automated login
                        AutomatedLogin(doc, userName, password);
                    }

                    while (!m_navigationComplete && !m_applicationClosing)
                        Application.DoEvents();

                    if (m_applicationClosing)
                        break;

                    // Assuming that arrival at alexa.amazon.com indicates successful authentication
                    // TODO: Check for more reliable way to validate authentication - URL's could change in the future...
                    if ((object)doc.Url != null && doc.Url.Host.Equals("alexa.amazon.com", StringComparison.OrdinalIgnoreCase))
                    {
                        if (manualLogin)
                        {
                            Log.Debug("Manual login succeeded.");

                            try
                            {
                                // Cache credentials for future logins (values will be encrypted and stored in user's appdata folder)
                                if (m_lastPostData.TryGetValue("email", out userName))
                                    m_userSettings["UserName"].Value = userName;

                                if (m_lastPostData.TryGetValue("password", out password))
                                    m_userSettings["Password"].Value = password;
                            }
                            catch (Exception ex)
                            {
                                ShowNotification($"Failed to cache user credentials: {ex.Message}", ToolTipIcon.Error);
                            }
                        }
                        else
                        {
                            Log.Debug("Automated login succeeded.");
                        }

                        Settings.Authenticated = true;
                        m_automatedLoginAttempts = 0;
                        ShowNotification("Successfully authenticated with Amazon Echo, starting activity monitoring cycle...", ToolTipIcon.Info);
#if !DEBUG
                        m_echoMonitor.HideWindow();
#endif
                        // Establish echo activity monitoring
                        EstablishEchoActivityMonitor();
                    }
                    else
                    {
                        // Next action will navigate, reset wait flag
                        m_navigationComplete = false;

                        if (!manualLogin)
                        {
                            // Can't attempt automated login forever, this may lock out user's account
                            if (m_automatedLoginAttempts < 5)
                            {
                                // Retry automated authentication a few times, maybe data connection is not available at the moment
                                ShowNotification("Failed to authenticate, trying again in 5 seconds.", ToolTipIcon.Warning);

                                // Pause 5 seconds between automated authentication attempts
                                DateTime waitTime = DateTime.UtcNow.AddSeconds(5.0);

                                // We are on the UI thread, maintain message loop processing...
                                while (DateTime.UtcNow < waitTime)
                                    Application.DoEvents();
                            }
                            else
                            {
                                // Show window and request user credentials
                                Authenticate(true);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Failure during authentication attempt: {ex.Message}", ToolTipIcon.Error);
            }

            // Update task bar icon tool tip with current authentication state
            m_echoMonitor.UpdateTaskbarTooltip();
        }

        // Establish Echo activity monitoring - try web socket attachment, if this fails, fall back on timer query
        private void EstablishEchoActivityMonitor()
        {
            // Attempt to piggy-back Amazon Websockets for dynamic response - we do this by updating the current page with script to
            // attach to the running Echo communications stack, see: http://www.piettes.com/echo/viewtopic.php?f=3&t=10
            const string activityMonitorScript =
                "    function startMonitor() {" +
                "        $('body').css('display', 'none');\r\n" +
                "        $('html').append('<div style=\"position: fixed; top:2%; left: 3%; transform: translate(-2%, -3%);\"><h2>Monitoring Echo Activity...</h2></div>');\r\n" +
                "        $('html').append('<div id=\"echochamber\" style=\"position: fixed; top:50%; left: 50%; transform: translate(-50%, -50%);\"><h1 class=\"command\">Waiting for you to interact with Alexa...</h1><br /><div class=\"output\" style=\"max-width: 50%; font-size: 10px; font-family: monospace\" /><br /><h3 class=\"displayTime\"></h3></div>');\r\n" +
                "        $('html').append('<div id=\"echochamber_note\" style=\"position: fixed; bottom:50px; left: 50%; transform: translate(-50%, 90%);\"><em>Commands you say that trigger a card to be created will show up here... Try \"tell me a joke\".</em></div>');\r\n" +
                "        $('html').css('background-color', 'white');\r\n" +
                "        return true;\r\n" +
                "    }\r\n\r\n" +
                "    // On card create notification, fetch card details and display\r\n" +
                "    function onNewCardActivity(card) {\r\n" +
                "        // Resets\r\n" +
                "        $('html').css('background-color', 'yellow');\r\n" +
                "        setTimeout(function() { $('html').css('background-color', 'white'); }, 5000);\r\n" +
                "        $('#echochamber h1, #echochamber .output').text(\"\");\r\n\r\n" +
                "        var key = card.key.registeredUserId + \"#\" + card.key.entryId;\r\n" +
                "        var url = '" + Settings.BaseURL + Settings.ActivitiesAPI + "/'+ encodeURIComponent(key);\r\n" +
                "        $.get(url, function(data){\r\n" +
                "            if (data.activity) {\r\n" +
                "                // Trigger .NET call-back\r\n" +
                "                window.external.EchoActivityCallback();\r\n\r\n" +
                "                // Parse JSON response for display\r\n" +
                "                var command;\r\n" +
                "                if (data.activity.description) {\r\n" +
                "                    command = JSON.parse(data.activity.description).summary;\r\n" +
                "                }\r\n" +
                "                // Show output\r\n" +
                "                $('#echochamber h1').text(command);\r\n" +
                "                $('#echochamber .output').text(JSON.stringify(data.activity, undefined, 2));\r\n" +
                "                $('#echochamber h3').text(\"Last command received at \" + Date());\r\n" +
                "            }\r\n" +
                "            $('html').css('background-color', 'green');\r\n" +
                "        });\r\n" +
                "    }\r\n\r\n" +
                "    // Attach to the card creation listener\r\n" +
                "    setTimeout(function() {" +
                "        var cardStream = require(\"collections/cardstream/card-collection\").getInstance();\r\n" +
                "        cardStream.listenTo(cardStream, \"pushMessage\", onNewCardActivity);\r\n" +
                "        $('#echochamber .output').text('Established card creation listener...');\r\n" +
                "    }, 1000);";

            HtmlDocument doc = m_echoMonitor.AuthenticationBrowser.Document;
            object response = null;

            // TODO: What to do if user navigates away from this page? Monitor for page change and switch to poll (maybe with a warning) or lock out browser control?
            // TODO: Maybe want to still use timer to query process activities, although on a longer interval, to make sure user session doesn't expire
            if ((object)doc != null && (object)doc.Body != null)
            {
                HtmlElement script = doc.CreateElement("script");

                if ((object)script != null)
                {
                    script.SetAttribute("text", activityMonitorScript);
                    doc.Body?.AppendChild(script);
                    response = doc.InvokeScript("startMonitor");
                }
            }

            // If dynamic monitoring is not available, start activity query on a timer
            if (response.ToNonNullNorEmptyString("false").ParseBoolean())
            {
                Log.Debug("Successfully established Websocket monitor for Echo activities.");
            }
            else
            {
                Log.Debug("Failed to establish Websocket monitor for Echo activities.");
                m_echoMonitor.QueryTimer.Interval = Settings.QueryInterval;
                m_echoMonitor.QueryTimer.Enabled = true;
                ShowNotification("Using timer based query for activity polling, web socket is unavailable...", ToolTipIcon.Warning);
            }
        }

        private void AutomatedLogin(HtmlDocument doc, string userName, string password)
        {
            m_automatedLoginAttempts++;

            HtmlElementCollection forms = doc.GetElementsByTagName("form");

            foreach (HtmlElement form in forms)
            {
                string action = form.GetAttribute("action");

                if (string.IsNullOrEmpty(action) || !IsHttpUrl(action))
                    continue;

                HtmlElementCollection inputs = form.GetElementsByTagName("input");
                bool emailSet = false, passwordSet = false;

                foreach (HtmlElement input in inputs)
                {
                    string type = input.GetAttribute("type");

                    if (type.Equals("email", StringComparison.OrdinalIgnoreCase))
                    {
                        input.SetAttribute("value", userName);
                        emailSet = true;

                        if (passwordSet)
                            break;
                    }
                    else if (type.Equals("password", StringComparison.OrdinalIgnoreCase))
                    {
                        input.SetAttribute("value", password);
                        passwordSet = true;

                        if (emailSet)
                            break;
                    }
                }

                form.InvokeMember("submit");
                break;
            }
        }

        private static bool IsHttpUrl(string url)
        {
            try
            {
                Uri test = new Uri(url);
                return test.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void ClearCredentials()
        {
            try
            {
                m_userSettings["UserName"].Value = "";
                m_userSettings["Password"].Value = "";
                ConfigurationFile.Current.Save();
            }
            catch
            {
                try
                {
                    // For exceptions, usually related to key changes, reset user settings file
                    ConfigurationFile.Current.RestoreDefaultUserSettings();
                }
                catch (Exception ex)
                {
                    ShowNotification($"Failed to clear cached user credentials: {ex.Message}", ToolTipIcon.Error);
                }
            }
        }

        private void scriptInterface_ReceivedEchoActivity(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state => m_echoMonitor.QueueProcessActivities());
        }

        private void m_echoMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel any pending navigation waiters when application is exiting - note that the UserClosing
            // reason is an exception since this just minimizes application to the task area
            m_applicationClosing = e.CloseReason != CloseReason.UserClosing;
        }

        private void BrowserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            m_navigationComplete = true;
        }

        private void BrowserControl_BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
            try
            {
                // Grab post data so that we can cache credentials between runs...
                byte[] data = PostData as byte[];

                if ((object)data != null && data.Length > 0)
                    m_lastPostData = Encoding.UTF8.GetString(data).ParseKeyValuePairs('&');
            }
            catch (Exception ex)
            {
                // Need post data to cache user credentials between runs, if this fails it's not the end of the world,
                // it just means the user will need to login each time the application runs
                Log.Error($"Failed to get post data, login credentials cannot be cached: {ex.Message}");
                m_lastPostData.Clear();
            }
        }

        private void ShowNotification(string message, ToolTipIcon icon = Settings.DefaultToolTipIcon, int timeout = Settings.DefaultToolTipTimeout, bool forceDisplay = false)
        {
            m_echoMonitor.ShowNotification(message, icon, timeout, forceDisplay, Log);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(AuthenticationManager));

        #endregion
    }
}
