//******************************************************************************************************
//  ResponseMessage.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/18/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Media;
using System.Xml.Serialization;
using GSF;
using GSF.IO;
using log4net;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents a response message for a triggered <see cref="Command"/>.
    /// </summary>
    public class ResponseMessage
    {
        /// <summary>
        /// Response type, if any is defined.
        /// </summary>
        [XmlAttribute("type")]
        public ResponseType Type;

        private string m_value;
        private string m_fileName;

        /// <summary>
        /// Response file name, used when response type is for a Wave file.
        /// </summary>
        [XmlAttribute("fileName")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;

                // Reference file from local folder if no path was provided
                if (!string.IsNullOrWhiteSpace(m_fileName))
                    m_fileName = FilePath.GetAbsolutePath(m_fileName);
            }
        }

        /// <summary>
        /// Message value.
        /// </summary>
        [XmlText]
        public string Value
        {
            get
            {
                return m_value ?? "";
            }
            set
            {
                m_value = value.ToNonNullString().Trim().RemoveDuplicateWhiteSpace();
            }
        }

        /// <summary>
        /// Processes response.
        /// </summary>
        /// <param name="failureReason">Any failure reason to process.</param>
        public void ProcessResponse(string failureReason = null)
        {
            string responseValue = Value;

            if (!string.IsNullOrWhiteSpace(failureReason))
                responseValue = responseValue.ReplaceCaseInsensitive("[reason]", failureReason);

            switch (Type)
            {
                case ResponseType.Tts:
                    // Respond using text-to-speech engine
                    if (!string.IsNullOrWhiteSpace(responseValue) && Settings.TTSFeedbackEnabled)
                        TTSEngine.Speak(responseValue);
                    break;
                case ResponseType.Wav:
                    try
                    {
                        // Attempt to play Wave file based response
                        if (!string.IsNullOrWhiteSpace(m_fileName) && File.Exists(m_fileName))
                        {
                            // Wave sound will keep playing in background even after player is disposed
                            using (SoundPlayer player = new SoundPlayer(m_fileName))
                                player.Play();
                        }
                        else
                        {
                            throw new FileNotFoundException();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WarnFormat("Failed to playback Wave response \"{0}\": {1}", m_fileName, ex.Message);

                        // Fall back on TTS response string if Wave file cannot be played
                        if (!string.IsNullOrWhiteSpace(responseValue) && Settings.TTSFeedbackEnabled)
                            TTSEngine.Speak(responseValue);
                    }
                    break;
            }
        }

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResponseMessage));

        #endregion
    }
}
