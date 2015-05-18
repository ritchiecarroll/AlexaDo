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

using GSF;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents a key phrase, i.e., a list of words, used to trigger a <see cref="Command"/>.
    /// </summary>
    public class KeyPhrase
    {
        /// <summary>
        /// Comma separated list of words that make up key phrase.
        /// </summary>
        public string Value;

        /// <summary>
        /// Get array of words parsed from key phrase <see cref="Value"/>.
        /// </summary>
        public string[] Words
        {
            get
            {
                return Value.ToNonNullNorWhiteSpace().Split(',');
            }
        }
    }
}
