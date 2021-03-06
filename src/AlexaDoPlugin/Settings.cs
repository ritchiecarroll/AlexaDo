﻿//******************************************************************************************************
//  Settings.cs - Gbtc
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

using System.Windows.Forms;
using GSF;
using GSF.Configuration;

namespace AlexaDoPlugin
{
    /// <summary>
    /// Defines configured application settings.
    /// </summary>
    public static class Settings
    {
        // Constants

        /// <summary>
        /// Base URL for Amazon Echo services.
        /// </summary>
        public const string BaseURL = "https://pitangui.amazon.com";

        /// <summary>
        /// URL path for the Amazon Echo activities history.
        /// </summary>
        public const string ActivitiesAPI = "/api/activities";

        /// <summary>
        /// API parameters to query five Amazon Echo activities from history.
        /// </summary>
        public const string QueryTopFiveActivities = "?startTime=&size=5&offset=-1";

        /// <summary>
        /// Default tool-tip icon for notifications.
        /// </summary>
        public const ToolTipIcon DefaultToolTipIcon = ToolTipIcon.None;

        /// <summary>
        /// Default tool-top timeout for notifications.
        /// </summary>
        public const int DefaultToolTipTimeout = 1500;

        // Default browser user-agent
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";

        // Default key words to detect
        private const string DefaultStartKeyWord = "Simon Says";
        private const string DefaultEndKeyWord = "Stop";

        // Static Fields
        private static readonly string s_userAgent;
        private static readonly int s_queryInterval;
        private static readonly double s_timeTolerance;
        private static readonly string s_startKeyWord;
        private static readonly string s_endKeyWord;
        private static bool s_authenticated;
        private static bool s_ttsFeedbackEnabled;

        // Static Constructor
        static Settings()
        {
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["system"];

            // Make sure default settings exist - note that these settings will only be recreated when missing
            // when application has appropriate rights to update to primary configuration file. For example,
            // if application is installed into Program Files folder, default location, the AlexaDo.exe.config
            // file can only be updated when application is run with elevated privileges.
            systemSettings.Add("UserAgent", "", "Browser User-Agent to use when authenticating", false, SettingScope.Application);
            systemSettings.Add("QueryInterval", "0.5", "Echo activity query interval, in seconds (floating-point)", false, SettingScope.Application);
            systemSettings.Add("TimeTolerance", "45.0", "Echo activity time processing tolerance, in seconds (floating-point)", false, SettingScope.Application);
            systemSettings.Add("StartKeyWord", "", "Key word to recognize at start of command, defaults to Simon Says", false, SettingScope.Application);
            systemSettings.Add("EndKeyWord", "", "Key word to recognize at end of command, defaults to Stop", false, SettingScope.Application);
            systemSettings.Add("TTSSpeed", "0", "Speech rate to use for TTS engine (-10 to 10)", false, SettingScope.Application);

            // Get current settings
            s_userAgent = systemSettings["UserAgent"].Value.ToNonNullNorWhiteSpace(DefaultUserAgent);
            s_queryInterval = (int)(systemSettings["QueryInterval"].ValueAs(0.5D) * 1000.0D);
            s_timeTolerance = systemSettings["TimeTolerance"].ValueAs(45.0D);
            s_startKeyWord = systemSettings["StartKeyWord"].Value.ToNonNullNorWhiteSpace(DefaultStartKeyWord);
            s_endKeyWord = systemSettings["EndKeyWord"].Value.ToNonNullNorWhiteSpace(DefaultEndKeyWord);

            // Assign configured speech rate to text-to-speech engine
            TTSEngine.SetRate(systemSettings["TTSSpeed"].ValueAs(0));
        }

        // Static Properties

        /// <summary>
        /// Gets the configured user-agent string to use when making web requests.
        /// </summary>
        public static string UserAgent => s_userAgent;

        /// <summary>
        /// Gets the configured query interval to use when making activity queries.
        /// </summary>
        public static int QueryInterval => s_queryInterval;

        /// <summary>
        /// Gets the configured time tolerance, compared to local clock, for processing recent Echo activities.
        /// </summary>
        public static double TimeTolerance => s_timeTolerance;

        /// <summary>
        /// Gets the configured Echo key word for marking the start of commands, e.g., Simon Says.
        /// </summary>
        public static string StartKeyWord => s_startKeyWord;

        /// <summary>
        /// Gets the configured Echo key word for marking the end of commands, e.g., Stop.
        /// </summary>
        public static string EndKeyWord => s_endKeyWord;

        /// <summary>
        /// Gets or sets current authentication state.
        /// </summary>
        public static bool Authenticated
        {
            get
            {
                return s_authenticated;
            }
            set
            {
                s_authenticated = value;
            }
        }

        /// <summary>
        /// Gets or sets current TTS feedback enabled state.
        /// </summary>
        /// <remarks>
        /// This is set to <c>true</c> when "None" is selected as desired TTS voice.
        /// </remarks>
        public static bool TTSFeedbackEnabled
        {
            get
            {
                return s_ttsFeedbackEnabled;
            }
            set
            {
                s_ttsFeedbackEnabled = value;
            }
        }
    }
}
