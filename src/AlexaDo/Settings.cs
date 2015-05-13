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

using GSF;
using GSF.Configuration;

namespace AlexaDo
{
    /// <summary>
    /// Key word styles.
    /// </summary>
    public enum KeyWordStyle
    {
        /// <summary>
        /// Recognize key word at beginning of command, e.g., Simon Says.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Recognize key word at end of command, e.g., Stop.
        /// </summary>
        EndsWith
    }

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
        /// <remarks>
        /// Just grabbing the last five activities for now.
        /// </remarks>
        public const string ActivitiesAPI = "/api/activities?startTime=&endTime=&size=5&offset=-1";

        // Default browser user-agent
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
        private const string DefaultKeyWord = "Simon Says";
        private const KeyWordStyle DefaultKeyWordStyle = KeyWordStyle.StartsWith;

        // Static Fields
        private readonly static string s_userAgent;
        private readonly static int s_queryInterval;
        private readonly static double s_timeTolerance;
        private readonly static string s_keyWord;
        private readonly static KeyWordStyle s_keyWordStyle;
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
            systemSettings.Add("QueryInterval", "3", "Echo activity query interval, in seconds (integer)", false, SettingScope.Application);
            systemSettings.Add("TimeTolerance", "30.0", "Echo activity time processing tolerance, in seconds (floating-point)", false, SettingScope.Application);
            systemSettings.Add("KeyWord", "", "Key word for commands, defaults to Simon Says", false, SettingScope.Application);
            systemSettings.Add("KeyWordStyle", "StartsWith", "Key word style for commands, either StartsWith or EndsWith", false, SettingScope.Application);
            systemSettings.Add("TTSSpeed", "0", "Speech rate to use for TTS engine (-10 to 10)", false, SettingScope.Application);

            // Get current settings
            s_userAgent = systemSettings["UserAgent"].Value.ToNonNullNorWhiteSpace(DefaultUserAgent);
            s_queryInterval = systemSettings["QueryInterval"].ValueAs(3) * 1000;
            s_timeTolerance = systemSettings["TimeTolerance"].ValueAs(30.0);
            s_keyWord = systemSettings["KeyWord"].Value.ToNonNullNorWhiteSpace(DefaultKeyWord);
            s_keyWordStyle = systemSettings["KeyWordStyle"].ValueAs(DefaultKeyWordStyle);

            // Assign configured speech rate to text-to-speech engine
            TTSEngine.SetRate(systemSettings["TTSSpeed"].ValueAs(0));
        }

        // Static Properties

        /// <summary>
        /// Gets the configured user-agent string to use when making web requests.
        /// </summary>
        public static string UserAgent
        {
            get
            {
                return s_userAgent;
            }
        }

        /// <summary>
        /// Gets the configured query interval to use when making activity queries.
        /// </summary>
        public static int QueryInterval
        {
            get
            {
                return s_queryInterval;
            }
        }

        /// <summary>
        /// Gets the configured time tolerance, compared to local clock, for processing recent Echo activities.
        /// </summary>
        public static double TimeTolerance
        {
            get
            {
                return s_timeTolerance;
            }
        }

        /// <summary>
        /// Gets the configured Echo key word for marking commands, e.g., Simon Says or Stop.
        /// </summary>
        public static string KeyWord
        {
            get
            {
                return s_keyWord;
            }
        }

        /// <summary>
        /// Gets the configured Echo key word style, i.e., key word at beginning or end of command.
        /// </summary>
        public static KeyWordStyle KeyWordStyle
        {
            get
            {
                return s_keyWordStyle;
            }
        }

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
