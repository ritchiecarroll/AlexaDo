//******************************************************************************************************
//  TTSEngine.cs - Gbtc
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
using System.Linq;
using System.Speech.Synthesis;

namespace AlexaDoPlugin
{
    /// <summary>
    /// Functions for text-to-speech handling
    /// </summary>
    public static class TTSEngine
    {
        // Base voice name for voice names copied from server registry key. See following article for installing other voices in Windows: 
        // https://forums.robertsspaceindustries.com/discussion/147385/voice-attack-getting-free-alternate-tts-voices-working-with-win7-8-64bit
        private const string BaseVoiceName = "Microsoft Server Speech Text to Speech Voice (";

        // Static Fields
        private static readonly SpeechSynthesizer s_synthesizer;
        private static readonly Dictionary<string, InstalledVoice> s_voices;
        private static string m_selectedVoice;

        // Static Constructor
        static TTSEngine()
        {
            s_synthesizer = new SpeechSynthesizer();
            s_voices = new Dictionary<string, InstalledVoice>(StringComparer.OrdinalIgnoreCase);

            foreach (InstalledVoice voice in s_synthesizer.GetInstalledVoices())
                s_voices.Add(GetShortVoiceName(voice.VoiceInfo.Name), voice);
        }

        // Static Properties

        /// <summary>
        /// Gets list of condensed TTS voice names.
        /// </summary>
        public static string[] VoiceNames => s_voices.Keys.ToArray();

        /// <summary>
        /// Gets list of installed voice information.
        /// </summary>
        public static InstalledVoice[] InstalledVoices => s_voices.Values.ToArray();

        /// <summary>
        /// Gets or sets selected TTS voice.
        /// </summary>
        public static string SelectedVoice
        {
            get
            {
                if (string.IsNullOrEmpty(m_selectedVoice))
                    m_selectedVoice = s_voices.First().Key;

                return m_selectedVoice;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = s_voices.First().Key;

                value = GetShortVoiceName(value);

                m_selectedVoice = value;

                s_synthesizer.SelectVoice(s_voices[m_selectedVoice].VoiceInfo.Name);
            }
        }

        // Static Methods

        /// <summary>
        /// Invoke TTS for given <paramref name="text"/> in currently selected voice.
        /// </summary>
        /// <param name="text">Text to speak.</param>
        public static void Speak(string text)
        {
            s_synthesizer.SpeakAsync(text);
        }

        /// <summary>
        /// Selects voice by index.
        /// </summary>
        /// <param name="voice">Voice index.</param>
        public static void SelectVoice(int voice)
        {
            SelectedVoice = InstalledVoices[voice].VoiceInfo.Name;
        }

        /// <summary>
        /// Sets voice speaking rate.
        /// </summary>
        /// <param name="rate">Speech rate, -10 to +10.</param>
        public static void SetRate(int rate)
        {
            if (rate < -10)
                rate = -10;

            if (rate > 10)
                rate = 10;

            s_synthesizer.Rate = rate;
        }

        /// <summary>
        /// Attempts to select voice by closest matching name.
        /// </summary>
        /// <param name="voiceName">Voice name to attempt to select.</param>
        /// <returns><c>true</c> if a matching voice name was selected; otherwise, <c>false</c>.</returns>
        public static bool TrySelectVoice(string voiceName)
        {
            InstalledVoice[] desiredVoices = InstalledVoices.Where(voice => voice.VoiceInfo.Name.IndexOf(voiceName, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            if (desiredVoices.Length == 0)
                return false;

            SelectedVoice = desiredVoices[0].VoiceInfo.Name;
            return true;
        }

        // When installing extra voices from server key, names are excessively long
        private static string GetShortVoiceName(string voiceName)
        {
            if ((object)voiceName == null)
                throw new ArgumentNullException(nameof(voiceName));

            if (voiceName.StartsWith(BaseVoiceName, StringComparison.OrdinalIgnoreCase))
                voiceName = voiceName.Substring(BaseVoiceName.Length, voiceName.Length - BaseVoiceName.Length - 1);

            return voiceName;
        }
    }
}
