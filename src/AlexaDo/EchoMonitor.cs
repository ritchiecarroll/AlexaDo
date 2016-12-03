//******************************************************************************************************
//  EchoMonitor.cs - Gbtc
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
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using AlexaDoPlugin;
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
#if DEBUG
            TestCommandButton.Visible = true;
            TestCommandButton.Enabled = true;
            StatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Right;
#else
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
                return;

            // When user selects to close application, just hide window
            e.Cancel = true;
            HideWindow(true);
        }

        private void BrowserControl_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, WebBrowserNavigatingEventArgs>(BrowserControl_Navigating), sender, e);
                return;    
            }

            URLFeedbackLabel.Text = $"Navigating to {e.Url}...";
        }

        private void BrowserControl_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, WebBrowserProgressChangedEventArgs>(BrowserControl_ProgressChanged), sender, e);
                return;
            }

            if (e.CurrentProgress < 0)
                return;

            URLLoadingProgress.Maximum = (int)e.MaximumProgress;
            URLLoadingProgress.Value = (int)e.CurrentProgress;
        }

        private void BrowserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, WebBrowserDocumentCompletedEventArgs>(BrowserControl_DocumentCompleted), sender, e);
                return;
            }

            URLFeedbackLabel.Text = $"Loaded {e.Url}.";
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
            QueueProcessActivities();
        }

        private void TestCommandButton_ButtonClick(object sender, EventArgs e)
        {
            if ((object)m_activityProcessor == null)
                return;

            using (TestCommand commandForm = new TestCommand())
            {
                if (commandForm.ShowDialog(this) == DialogResult.OK)
                    m_activityProcessor.TestActivity(commandForm.CommandText.Text);
            }
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

        internal void ShowNotification(string message, ToolTipIcon icon = Settings.DefaultToolTipIcon, int timeout = Settings.DefaultToolTipTimeout, bool forceDisplay = false, ILog logger = null)
        {
            NotifyIcon.BalloonTipText = message;
            NotifyIcon.BalloonTipIcon = icon;

            if (!Visible || forceDisplay)
                NotifyIcon.ShowBalloonTip(timeout);

            UpdateStatus($"{(icon == ToolTipIcon.None ? "" : $"{icon}: ")}{message}");

            if ((object)logger == null)
                logger = Log;

            switch (icon)
            {
                case ToolTipIcon.Warning:
                    logger.Warn(message);
                    break;
                case ToolTipIcon.Error:
                    logger.Error(message);
                    break;
                default:
                    logger.Info(message);
                    break;
            }
        }

        internal void UpdateStatus(string message, params object[] args)
        {
            StatusLabel.Text = $"[{DateTime.Now:F}] {string.Format(message, args)}";
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
                ShowNotification($"{NotifyIcon.Tag} still running in task bar...", ToolTipIcon.Info, forceDisplay: true);

            Hide();
        }

        // This function should normally be called from the thread pool..
        internal void QueueProcessActivities()
        {            
            try
            {
                bool pending = m_activityProcessor?.ProcessActivities() ?? false;

                // There may be several activities for a single voice command, e.g., one for Alexa and another for the speech heard, so
                // wait a half-second or so before kicking off activity processing
                if (pending)
                    Thread.Sleep(500);

                BeginInvoke((Action)UpdateTaskbarTooltip);
            }
            catch (Exception ex)
            {
                Log.Error("QueueProcessActivities Exception", ex);
            }
        }

        internal void UpdateTaskbarTooltip()
        {
            if ((object)m_activityProcessor == null)
                return;

            long queries = m_activityProcessor.TotalQueries;

            NotifyIcon.Text = $"{NotifyIcon.Tag} - {(Settings.Authenticated ? $"Authenticated, {queries:N0} {(queries == 1 ? "query" : "queries")}" : "Not Authenticated")}";
        }

        private void Reauthenticate(bool requestCredentials)
        {
            // To allow operation to be run from other threads, queue call for message loop processing if needed
            if (InvokeRequired)
            {
                BeginInvoke((Action<bool>)Reauthenticate, requestCredentials);
                return;
            }

            QueryTimer.Enabled = false;

            m_authenticationManager?.Authenticate(requestCredentials);
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
                    string[] selectedVoices = TTSEngine.VoiceNames.Where(voice => voice.Equals(voiceName, StringComparison.OrdinalIgnoreCase)).ToArray();

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
                ShowNotification($"Failed to select voice: {ex.Message}", ToolTipIcon.Error);
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
