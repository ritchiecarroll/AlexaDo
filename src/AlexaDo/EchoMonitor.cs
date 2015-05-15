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
using System.Windows.Forms;
using GSF.Configuration;
using log4net;

namespace AlexaDo
{
    /// <summary>
    /// EchoMonitor provides a base WebBrowser control and manages any TTS voice selections.
    /// </summary>
    public partial class EchoMonitor : Form
    {
        #region [ Members ]

        // Fields
        private AuthenticationManager m_authenticationManager;
        private ActivityProcessor m_activityProcessor;
        private CategorizedSettingsElementCollection m_userSettings;
        private bool m_initialDisplay;
        private Size m_initialSize;
        private Point m_initialLocation;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="EchoMonitor"/> form instance.
        /// </summary>
        public EchoMonitor()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Event Handlers ]

        private void Browser_Load(object sender, EventArgs e)
        {
            // Load speech voices into user selection drop-down
            VoiceDropDown.DropDownItems.Add("None");

            foreach (string voice in TTSEngine.VoiceNames)
                VoiceDropDown.DropDownItems.Add(voice);

            // Make sure default user settings exist
            m_userSettings = ConfigurationFile.Current.Settings["user"];
            m_userSettings.Add("TTSVoice", "", "Selected text-to-speech voice", false, SettingScope.User);

            // Restore last selected voice
            SelectVoice(m_userSettings["TTSVoice"].ValueAs(""));

            m_initialDisplay = true;
            m_initialSize = Size;
            m_initialLocation = Location;
#if !DEBUG
            WindowState = FormWindowState.Minimized;
#endif
        }

        private void Browser_Shown(object sender, EventArgs e)
        {
#if !DEBUG
            // Start hidden
            HideWindow();
#endif
            if ((object)m_activityProcessor == null)
                m_activityProcessor = new ActivityProcessor(this);

            if ((object)m_authenticationManager == null)
                m_authenticationManager = new AuthenticationManager(this);

            m_authenticationManager.Authenticate();
        }

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                if ((object)m_authenticationManager != null)
                    m_authenticationManager.Dispose();

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
            QueryTimer.Enabled = false;
            ConfigurationFile.Current.Save(ConfigurationSaveMode.Full);
            Application.Exit();
        }

        private void QueryTimer_Tick(object sender, EventArgs e)
        {
            TryProcessActivities();
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Windows Form base class message processing function.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            bool handled = false;

            switch (m.Msg)
            {
                // Preempt minimize message so we hide window
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

        internal void ShowNotification(string message, ToolTipIcon icon = ToolTipIcon.None, int timeout = 1500, bool forceDisplay = false)
        {
            NotifyIcon.BalloonTipText = message;
            NotifyIcon.BalloonTipIcon = icon;

            if (!Visible || forceDisplay)
                NotifyIcon.ShowBalloonTip(timeout);

            UpdateStatus(string.Format("{0}{1}", icon == ToolTipIcon.None ? "" : string.Format("{0}: ", icon), message));

            switch (icon)
            {
                case ToolTipIcon.Warning:
                    Log.Warn(message);
                    break;
                case ToolTipIcon.Error:
                    Log.Error(message);
                    break;
                default:
                    Log.Info(message);
                    break;
            }
        }

        internal void UpdateStatus(string message, params object[] args)
        {
            StatusLabel.Text = string.Format("[{0:F}] {1}", DateTime.Now, string.Format(message, args));
        }

        internal void ShowWindow()
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

        internal void HideWindow(bool notify = false)
        {
            if (notify)
                ShowNotification(string.Format("{0} still running in task bar...", NotifyIcon.Tag), ToolTipIcon.Info, forceDisplay: true);

            Hide();
        }

        internal void TryProcessActivities()
        {
            if ((object)m_activityProcessor == null)
                return;

            // If did not process activities, retry authentication
            if (!m_activityProcessor.ProcessActivities())
                Reauthenticate(false);

            // To allow update operation to be run from other threads, queue call for message loop processing if needed
            if (InvokeRequired)
                BeginInvoke((Action)UpdateTaskbarTooltip);
            else
                UpdateTaskbarTooltip();
        }

        private void UpdateTaskbarTooltip()
        {
            if ((object)m_activityProcessor == null)
                return;

            NotifyIcon.Text =
                string.Format("{0} - {1}", NotifyIcon.Tag, Settings.Authenticated ?
                string.Format("Authenticated, {0:N0} queries", m_activityProcessor.TotalQueries) : "Not Authenticated");
        }

        private void Reauthenticate(bool requestCredentials)
        {
            QueryTimer.Enabled = false;

            if ((object)m_authenticationManager != null)
                m_authenticationManager.Authenticate(requestCredentials);
        }

        private void SelectVoice(string voiceName)
        {
            try
            {
                if ((object)voiceName == null)
                    voiceName = "";

                if (voiceName.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    Settings.TTSFeedbackEnabled = false;
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

                    Settings.TTSFeedbackEnabled = true;
                    VoiceDropDown.Text = TTSEngine.SelectedVoice;
                }
            }
            catch (Exception ex)
            {
                // Fall back on no voice for errors
                Settings.TTSFeedbackEnabled = false;
                VoiceDropDown.Text = "None";
                ShowNotification(string.Format("Failed to select voice: {0}", ex.Message), ToolTipIcon.Error);
            }

            // Save current voice selection
            m_userSettings["TTSVoice"].Value = VoiceDropDown.Text;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(EchoMonitor));

        #endregion
    }
}
