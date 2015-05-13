//******************************************************************************************************
//  EchoMonitor.Authenticate.Designer.cs - Gbtc
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
using System.Windows.Forms;
using GSF.Configuration;

namespace AlexaDo
{
    public partial class EchoMonitor
    {
        private bool m_authenticated;

        private void Authenticate(bool clearCredentials = false)
        {
            try
            {
                m_authenticated = false;

                UpdateStatus("Attempting Amazon Echo authentication...");

                // Had to resort to using WebBrowser control to automate Amazon authentication, initial
                // attempts to use WebClient failed even after cloning all hidden inputs and adding email,
                // password and create parameters. Speculating that JavaScript is needed in order to
                // dynamically add other hidden inputs, e.g., "metadata1" that do not exist in base form
                // HTML - perhaps someone else will have more time / better luck, but the following seems
                // reliable. Since a web form is being used, decided to take advantage of it and just use
                // the web page to get user to authenticate, then cache the credentials for future loads.
                Navigate(BaseURL, true);

                HtmlDocument doc = BrowserControl.Document;

                if ((object)doc == null)
                    return;

                string userName = null, password = null;
                bool manualLogin = false;

                if (clearCredentials)
                {
                    ClearCredentials();
                    Navigate(BaseURL + "/logout", true);
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

                // See if cached user credentials exist
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    // Request credentials from user
                    ShowWindow();
                    ShowNotifcation("Enter credentials to login to Amazon Echo...", ToolTipIcon.Info, forceDisplay: true);
                    m_navigationComplete = false;
                    manualLogin = true;
                }
                else
                {
                    // Have existing credentials, attempt automated login
                    AutomatedLogin(doc, userName, password);
                }

                while (!m_navigationComplete || (object)doc.Url == null)
                    Application.DoEvents();

                // Arrival at echo.amazon.com indicates successful authentication
                if (doc.Url.Host.Equals("echo.amazon.com", StringComparison.OrdinalIgnoreCase))
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
                            ShowNotifcation(string.Format("Failed to cache user credentials: {0}", ex.Message), ToolTipIcon.Error);
                        }
                    }

                    m_authenticated = true;
                    HideWindow();
                    ShowNotifcation("Successfully authenticated Amazon Echo, starting activity monitoring cycle...", ToolTipIcon.Info);
                }
                else
                {
                    if (!manualLogin)
                        ShowNotifcation(string.Format("Failed to authenticate, trying again in {0:N0} seconds.", QueryTimer.Interval / 1000), ToolTipIcon.Warning);
                }

                // Start timer to begin activity query - or - retry automated authentication, maybe data connection is not available
                QueryTimer.Enabled = m_authenticated || !manualLogin;
            }
            catch (Exception ex)
            {
                ShowNotifcation(string.Format("Failure during authentication attempt: {0}", ex.Message), ToolTipIcon.Error);
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

                m_navigationComplete = false;
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
                ShowNotifcation(string.Format("Failed to clear cached user credentials: {0}", ex.Message), ToolTipIcon.Error);
            }
        }
    }
}
