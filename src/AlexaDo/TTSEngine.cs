//******************************************************************************************************
//  TTSEngine.cs - Gbtc
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
using System.Linq;
using System.Speech.Synthesis;

namespace AlexaDo
{
    public static class TTSEngine
    {
        private const string BaseVoiceName = "Microsoft Server Speech Text to Speech Voice (";

        private static readonly SpeechSynthesizer s_synthesizer;
        private static readonly Dictionary<string, InstalledVoice> s_voices;
        private static string m_selectedVoice;

        static TTSEngine()
        {
            s_synthesizer = new SpeechSynthesizer();
            s_voices = new Dictionary<string, InstalledVoice>(StringComparer.OrdinalIgnoreCase);

            foreach (InstalledVoice voice in s_synthesizer.GetInstalledVoices())
                s_voices.Add(GetShortVoiceName(voice.VoiceInfo.Name), voice);
        }

        public static string[] VoiceNames
        {
            get
            {
                return s_voices.Keys.ToArray();
            }
        }

        public static InstalledVoice[] InstalledVoices
        {
            get
            {
                return s_voices.Values.ToArray();
            }
        }

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

        public static void Speak(string text)
        {
            s_synthesizer.Speak(text);
        }

        public static void SelectVoice(int voice)
        {
            SelectedVoice = InstalledVoices[voice].VoiceInfo.Name;
        }

        public static void SetRate(int rate)
        {
            if (rate < -10)
                rate = -10;

            if (rate > 10)
                rate = 10;

            s_synthesizer.Rate = rate;
        }

        public static bool TrySelectVoice(string voiceName)
        {
            InstalledVoice[] desiredVoices = InstalledVoices.Where(voice => voice.VoiceInfo.Name.IndexOf(voiceName, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            if (desiredVoices.Length == 0)
                return false;

            SelectedVoice = desiredVoices[0].VoiceInfo.Name;
            return true;
        }

        private static string GetShortVoiceName(string voiceName)
        {
            if ((object)voiceName == null)
                throw new ArgumentNullException("voiceName");

            if (voiceName.StartsWith(BaseVoiceName, StringComparison.OrdinalIgnoreCase))
                voiceName = voiceName.Substring(BaseVoiceName.Length, voiceName.Length - BaseVoiceName.Length - 1);

            return voiceName;
        }
    }
}
