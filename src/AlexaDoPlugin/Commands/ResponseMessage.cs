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
        public ResponseType? Type;

        private string m_value;
        private string m_fileName;
        private SoundPlayer m_player;

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
            // Only process a response if one is defined
            if ((object)Type == null)
                return;

            string responseValue = Value;

            if (!string.IsNullOrWhiteSpace(failureReason))
                responseValue = responseValue.ReplaceCaseInsensitive("[reason]", failureReason);

            switch (Type.GetValueOrDefault(ResponseType.Tts))
            {
                case ResponseType.Tts:
                    // Respond using text-to-speech engine
                    if (!string.IsNullOrWhiteSpace(responseValue))
                        TTSEngine.Speak(responseValue);
                    break;
                case ResponseType.Wav:
                    try
                    {
                        // Attempt to play Wave file based response
                        if (!string.IsNullOrWhiteSpace(m_fileName) && File.Exists(m_fileName))
                        {
                            if ((object)m_player == null)
                                m_player = new SoundPlayer(m_fileName);

                            m_player.Play();
                        }
                        else
                        {
                            throw new FileNotFoundException();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fall back on TTS response string if Wave file cannot be played
                        if (!string.IsNullOrWhiteSpace(responseValue))
                            TTSEngine.Speak(responseValue);

                        Log.WarnFormat("Failed to playback Wave response \"{0}\", fell back on TTS: {1}", m_fileName, ex.Message);
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
