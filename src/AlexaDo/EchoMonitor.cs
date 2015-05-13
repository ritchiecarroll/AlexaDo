//******************************************************************************************************
//  EchoMonitor.cs - Gbtc
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
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;
using GSF;
using GSF.Configuration;

namespace AlexaDo
{
    public partial class EchoMonitor : Form
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
        private const string DefaultKeyWord = "Simon Says";
        private const string BaseURL = "https://pitangui.amazon.com";
        private const string ActivitiesAPI = "/api/activities?startTime=&endTime=&size=5&offset=-1";

        private SHDocVw.WebBrowser m_browserReference;
        private Dictionary<string, string> m_lastPostData;
        private readonly CategorizedSettingsElementCollection m_systemSettings;
        private readonly CategorizedSettingsElementCollection m_userSettings;
        private string m_userAgent;
        private double m_timeTolerance;
        private string m_keyWord;
        private bool m_navigationComplete;
        private bool m_ttsFeedbackEnabled;
        private bool m_initialDisplay;
        private Size m_initialSize;
        private Point m_initialLocation;

        public EchoMonitor()
        {
            InitializeComponent();

            m_lastPostData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            m_systemSettings = ConfigurationFile.Current.Settings["system"];
            m_userSettings = ConfigurationFile.Current.Settings["user"];
        }

        private void Browser_Load(object sender, EventArgs e)
        {
            // Load speech voices into user selection drop-down
            VoiceDropDown.DropDownItems.Add("None");

            foreach (string voice in TTSEngine.VoiceNames)
                VoiceDropDown.DropDownItems.Add(voice);

            // Make sure default settings exist
            m_systemSettings.Add("UserAgent", "", "Browser User-Agent to use when authenticating", false, SettingScope.Application);
            m_systemSettings.Add("TTSSpeed", "0", "Speech rate to use for TTS engine (-10 to 10)", false, SettingScope.Application);
            m_systemSettings.Add("QueryInterval", "3", "Echo activity query interval, in seconds (integer)", false, SettingScope.Application);
            m_systemSettings.Add("TimeTolerance", "30.0", "Echo activity time processing tolerance, in seconds (floating-point)", false, SettingScope.Application);
            m_systemSettings.Add("KeyWord", "", "Key word for commands, defaults to Simon Says", false, SettingScope.Application);

            m_userSettings.Add("UserName", "", "User name to use when authenticating", true, SettingScope.User);
            m_userSettings.Add("Password", "", "Password to use when authenticating", true, SettingScope.User);
            m_userSettings.Add("TTSVoice", "", "Selected text-to-speech voice", false, SettingScope.User);

            // Apply current settings
            m_userAgent = m_systemSettings["UserAgent"].Value.ToNonNullNorWhiteSpace(DefaultUserAgent);
            TTSEngine.SetRate(m_systemSettings["TTSSpeed"].ValueAs(0));
            QueryTimer.Interval = m_systemSettings["QueryInterval"].ValueAs(3) * 1000;
            m_timeTolerance = m_systemSettings["TimeTolerance"].ValueAs(30.0);
            m_keyWord = m_systemSettings["KeyWord"].Value.ToNonNullNorWhiteSpace(DefaultKeyWord);

            // Restore last selected voice
            SelectVoice(m_userSettings["TTSVoice"].ValueAs(""));

            // Start hidden
            m_initialDisplay = true;
            m_initialSize = Size;
            m_initialLocation = Location;
            WindowState = FormWindowState.Minimized;
        }

        private void Browser_Shown(object sender, EventArgs e)
        {
            try
            {
                m_browserReference = (SHDocVw.WebBrowser)BrowserControl.ActiveXInstance;
                m_browserReference.BeforeNavigate2 += BrowserControl_BeforeNavigate2;
            }
            catch
            {
                // Any possible failure here would not be fatal to operation
            }

            HideWindow();
            Authenticate();
        }

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                m_navigationComplete = true;
                return;
            }

            // When user selects to close application, just hide window
            e.Cancel = true;
            HideWindow(true);
        }

        private void BrowserControl_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            URLFeedbackLabel.Text = string.Format("Navigating to {0}...", e.Url);
        }

        private void BrowserControl_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress < 0)
                return;

            URLLoadingProgress.Maximum = (int)e.MaximumProgress;
            URLLoadingProgress.Value = (int)e.CurrentProgress;
        }

        private void BrowserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            URLFeedbackLabel.Text = string.Format("Loaded {0}.", e.Url);
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

        private void ShowMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void HideMenuItem_Click(object sender, EventArgs e)
        {
            HideWindow();
        }

        private void ReauthenticateMenuItem_Click(object sender, EventArgs e)
        {
            Reauthenticate(true);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            m_navigationComplete = true;
            QueryTimer.Enabled = false;
            ConfigurationFile.Current.Save(ConfigurationSaveMode.Full);
            Application.Exit();
        }

        private void QueryTimer_Tick(object sender, EventArgs e)
        {
            // If did not process activities, retry authentication
            if (!ProcessActivities())
                Reauthenticate(false);

            NotifyIcon.Text = string.Format("{0} - {1}", NotifyIcon.Tag, m_authenticated ?
                string.Format("Authenticated, {0:N0} queries", m_totalQueries) : "Not Authenticated");
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Visible)
                    HideWindow();
                else
                    ShowWindow();
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void VoiceDropDown_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SelectVoice(e.ClickedItem.Text);
        }

        private void Navigate(string url, bool scrollToBottom = false)
        {
            m_navigationComplete = false;
            BrowserControl.Navigate(url, null, null, "User-Agent: " + m_userAgent);

            while (!m_navigationComplete)
                Application.DoEvents();

            if (!scrollToBottom)
                return;

            // Scroll browser window to bring login screen into full view
            HtmlDocument doc = BrowserControl.Document;

            if ((object)doc != null && (object)doc.Window != null && (object)doc.Body != null)
                doc.Window.ScrollTo(0, doc.Body.ScrollRectangle.Height);
        }

        private void Reauthenticate(bool clearCredentials)
        {
            QueryTimer.Enabled = false;
            Authenticate(clearCredentials);
        }

        private void SelectVoice(string voiceName)
        {
            try
            {
                if ((object)voiceName == null)
                    voiceName = "";

                if (voiceName.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    m_ttsFeedbackEnabled = false;
                    VoiceDropDown.Text = "None";
                }
                else
                {
                    IEnumerable<string> selectedVoices = TTSEngine.VoiceNames.Where(voice => voice.Equals(voiceName, StringComparison.OrdinalIgnoreCase));

                    if (selectedVoices.Any())
                    {
                        TTSEngine.SelectedVoice = selectedVoices.First();
                    }
                    else
                    {
                        // Try to pick a female voice similar to Alexa
                        if (!TTSEngine.TrySelectVoice("Zira") && !TTSEngine.TrySelectVoice("Helen"))
                        {
                            InstalledVoice[] femaleVoices = TTSEngine.InstalledVoices.Where(voice => voice.VoiceInfo.Gender == VoiceGender.Female).ToArray();

                            if (femaleVoices.Length > 0)
                                TTSEngine.SelectedVoice = femaleVoices[0].VoiceInfo.Name;
                            else
                                TTSEngine.SelectVoice(0);
                        }
                    }

                    m_ttsFeedbackEnabled = true;
                    VoiceDropDown.Text = TTSEngine.SelectedVoice;
                }
            }
            catch (Exception ex)
            {
                // Fall back on no voice for errors
                m_ttsFeedbackEnabled = false;
                VoiceDropDown.Text = "None";
                ShowNotifcation(string.Format("Failed to select voice: {0}", ex.Message), ToolTipIcon.Error);
            }

            // Save current voice selection
            m_userSettings["TTSVoice"].Value = VoiceDropDown.Text;
        }

        private void ShowNotifcation(string message, ToolTipIcon icon = ToolTipIcon.None, int timeout = 1500, bool forceDisplay = false)
        {
            NotifyIcon.BalloonTipText = message;
            NotifyIcon.BalloonTipIcon = icon;

            if (!Visible || forceDisplay)
                NotifyIcon.ShowBalloonTip(timeout);

            UpdateStatus(string.Format("{0}{1}", icon == ToolTipIcon.None ? "" : string.Format("{0}: ", icon), message));
        }

        private void UpdateStatus(string message, params object[] args)
        {
            StatusLabel.Text = string.Format("[{0:F}] {1}", DateTime.Now, string.Format(message, args));
        }

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;

            if (m_initialDisplay)
            {
                m_initialDisplay = false;
                Size = m_initialSize;
                Location = m_initialLocation;
            }

            Activate();
        }

        private void HideWindow(bool notify = false)
        {
            if (notify)
                ShowNotifcation(string.Format("{0} still running in task bar...", NotifyIcon.Tag), ToolTipIcon.Info, forceDisplay: true);

            Hide();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            bool handled = false;

            switch (m.Msg)
            {
                // Preempt minimize message so we can save window size and position prior to minimization
                case WM_SYSCOMMAND:
                    if ((m.WParam.ToInt32() & 0xfff0) == SC_MINIMIZE)
                    {
                        HideWindow(true);
                        handled = true;
                    }
                    break;
            }

            if (!handled)
                base.WndProc(ref m);
        }
    }
}
