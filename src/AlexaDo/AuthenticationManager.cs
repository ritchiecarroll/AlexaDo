//******************************************************************************************************
//  AuthenticationManager.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
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
using System.Windows.Forms;
using GSF;
using GSF.Configuration;

namespace AlexaDo
{
    /// <summary>
    /// Provides Amazon Echo authentication handling.
    /// </summary>
    /// <threading>
    /// This class is dependent on the main form for an active WebBrowser control and feedback
    /// processing, as such methods will run on the main thread as invoked from primary message
    /// loop. As a result this class is not safe for multi-threaded access.
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
            m_userSettings.Add("UserName", "", "User name to use when authenticating", true, SettingScope.User);
            m_userSettings.Add("Password", "", "Password to use when authenticating", true, SettingScope.User);

            // Attach to needed events
            m_echoMonitor.FormClosing += m_echoMonitor_FormClosing;
            m_echoMonitor.BrowserControl.DocumentCompleted += BrowserControl_DocumentCompleted;

            try
            {
                // Reference underlying IE based ActiveX control so we can attach to native events
                m_activeXReference = (SHDocVw.WebBrowser)m_echoMonitor.BrowserControl.ActiveXInstance;

                // BrowserControl_BeforeNavigate2 event provides headers and post data
                m_activeXReference.BeforeNavigate2 += BrowserControl_BeforeNavigate2;
            }
            catch
            {
                // Any possible failure here would not be fatal to overall operation
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
                        m_echoMonitor.BrowserControl.DocumentCompleted -= BrowserControl_DocumentCompleted;

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
            m_echoMonitor.BrowserControl.Navigate(url, null, null, "User-Agent: " + Settings.UserAgent);

            while (!m_navigationComplete)
                Application.DoEvents();

            if (!scrollToBottom)
                return;

            // Scroll browser window to bring login screen into full view
            HtmlDocument doc = m_echoMonitor.BrowserControl.Document;

            if ((object)doc != null && (object)doc.Window != null && (object)doc.Body != null)
                doc.Window.ScrollTo(0, doc.Body.ScrollRectangle.Height);
        }

        /// <summary>
        /// Authenticates with Amazon Echo web application.
        /// </summary>
        /// <param name="clearCredentials">Set to <c>true</c> to clear any existing cached credentials.</param>
        public void Authenticate(bool clearCredentials = false)
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

                HtmlDocument doc = m_echoMonitor.BrowserControl.Document;

                if ((object)doc == null)
                    return;

                string userName = null, password = null;
                bool manualLogin = false;

                if (clearCredentials)
                {
                    ClearCredentials();
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
                        ClearCredentials();
                        userName = "";
                        password = "";
                    }
                }

                // Next action will navigate, reset wait flag
                m_navigationComplete = false;

                // See if cached user credentials exist
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    // Make sure window is visible, we need user to login
                    m_echoMonitor.ShowWindow();
                    m_echoMonitor.ShowNotification("Enter credentials to login to Amazon Echo...", ToolTipIcon.Info, forceDisplay: true);
                    m_navigationComplete = false;
                    manualLogin = true;
                }
                else
                {
                    // Have existing credentials, attempt automated login
                    AutomatedLogin(doc, userName, password);
                }

                while (!m_navigationComplete)
                    Application.DoEvents();

                // Arrival at echo.amazon.com indicates successful authentication
                if ((object)doc.Url != null && doc.Url.Host.Equals("echo.amazon.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (manualLogin)
                    {
                        try
                        {
                            // Cache credentials for future logins (will be encrypted)
                            if (m_lastPostData.TryGetValue("email", out userName))
                                m_userSettings["UserName"].Value = userName;

                            if (m_lastPostData.TryGetValue("password", out password))
                                m_userSettings["Password"].Value = password;
                        }
                        catch (Exception ex)
                        {
                            m_echoMonitor.ShowNotification(string.Format("Failed to cache user credentials: {0}", ex.Message), ToolTipIcon.Error);
                        }
                    }

                    Settings.Authenticated = true;
                    m_echoMonitor.HideWindow();
                    m_echoMonitor.ShowNotification("Successfully authenticated with Amazon Echo, starting activity monitoring cycle...", ToolTipIcon.Info);
                }
                else
                {
                    if (!manualLogin)
                        m_echoMonitor.ShowNotification(string.Format("Failed to authenticate, trying again in {0:N0} seconds.", m_echoMonitor.QueryTimer.Interval / 1000), ToolTipIcon.Warning);
                }

                // Start timer to begin activity query - or - retry automated authentication, maybe data connection is not available
                m_echoMonitor.QueryTimer.Enabled = Settings.Authenticated || !manualLogin;
            }
            catch (Exception ex)
            {
                m_echoMonitor.ShowNotification(string.Format("Failure during authentication attempt: {0}", ex.Message), ToolTipIcon.Error);
            }
        }

        private void AutomatedLogin(HtmlDocument doc, string userName, string password)
        {
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
                // Space assignment is important to properly clear credentials
                // during encrypted serialization to user configuration...
                m_userSettings["UserName"].Update(" ");
                m_userSettings["Password"].Update(" ");
                ConfigurationFile.Current.Save();
            }
            catch (Exception ex)
            {
                m_echoMonitor.ShowNotification(string.Format("Failed to clear cached user credentials: {0}", ex.Message), ToolTipIcon.Error);
            }
        }

        private void m_echoMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel any pending navigation waiters when application is exiting - note that the UserClosing
            // reason is an exception since this just minimizes application to the task area
            m_navigationComplete = e.CloseReason != CloseReason.UserClosing;
        }

        private void BrowserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            m_navigationComplete = true;
        }

        private void BrowserControl_BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
            try
            {
                byte[] data = PostData as byte[];

                if ((object)data != null && data.Length > 0)
                    m_lastPostData = Encoding.UTF8.GetString(data).ParseKeyValuePairs('&');
            }
            catch
            {
                // Need post data to cache user credentials between runs, if this fails it's not the end of the world,
                // it just means the user will need to login each time the application runs
                m_lastPostData.Clear();
            }
        }

        #endregion
    }
}
