//******************************************************************************************************
//  KeyPhrase.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/15/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Linq;
using GSF;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents a key phrase, i.e., a list of words, used to trigger a <see cref="Command"/>.
    /// </summary>
    public class KeyPhrase
    {
        private string m_value;
        private string m_cleanValue;
        private string[] m_words;

        /// <summary>
        /// Get array of words parsed from key phrase <see cref="Value"/>.
        /// </summary>
        public string[] Words
        {
            get
            {
                return m_words;
            }
        }

        /// <summary>
        /// Comma or space separated list of words that make up key phrase.
        /// </summary>
        public string Value
        {
            get
            {
                return m_value ?? "";
            }
            set
            {
                m_value = value.ToNonNullString();
                m_words = m_value.Split(',', ' ').Select(word => word.Trim().RemoveDuplicateWhiteSpace()).ToArray();
                m_cleanValue = string.Join(" ", m_words);
            }
        }

        /// <summary>
        /// Gets a clean phrase value, i.e., all words joined together with spaces.
        /// </summary>
        public string CleanValue
        {
            get
            {
                return m_cleanValue;
            }
        }
    }
}
