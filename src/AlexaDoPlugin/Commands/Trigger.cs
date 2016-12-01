//******************************************************************************************************
//  PluginCommands.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/15/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Xml.Serialization;
using GSF;

namespace AlexaDoPlugin.Commands
{
    // Helpful definitions:

    //     Trigger: A set of key phrases and a match style that will trigger a command
    //  Key Phrase: List of words that will trigger a command
    // Match Style: How to match words of a key phrase in-order to trigger a command

    /// <summary>
    /// Represents the possible key phrases and match style that will trigger a <see cref="Command"/>.
    /// </summary>
    [XmlType("trigger")]
    public class Trigger
    {
        /// <summary>
        /// Array of possible key phrases that will trigger a command.
        /// </summary>
        [XmlIgnore]
        public KeyPhrase[] KeyPhrases;

        /// <summary>
        /// Pipe separated list of possible key phrase values.
        /// </summary>
        [XmlElement("keyPhrase")]
        public string KeyPhraseValues
        {
            get
            {
                return string.Join("|", KeyPhrases.Select(keyPhrase => keyPhrase.Value));
            }
            set
            {
                KeyPhrases = value.ToNonNullString().Split('|').Select(keyPhrase => new KeyPhrase
                {
                    Value = keyPhrase
                })
                .ToArray();
            }
        }

        /// <summary>
        /// Match style to use when evaluating key phrases.
        /// </summary>
        [XmlElement(ElementName = "matchStyle")]
        public MatchStyle MatchStyle;

        /// <summary>
        /// Determines if Echo activity is a match for this <see cref="Command"/>.
        /// </summary>
        /// <param name="activity">Echo activity to test for match.</param>
        /// <param name="query">Remaining command query, if any, with key phrase removed.</param>
        /// <returns><c>true</c> if <paramref name="activity"/> is a match for this command; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// A <paramref name="query"/> value will only be returned for <see cref="Commands.MatchStyle.StartsWith"/> or
        /// <see cref="Commands.MatchStyle.EndsWith"/> and will be the remainder of the <see cref="EchoActivity.Command"/>
        /// minus the matched <see cref="KeyPhrase"/>.
        /// </remarks>
        public bool IsMatch(EchoActivity activity, out string query)
        {
            query = null;

            // Validate inputs and outputs needed for test
            if (string.IsNullOrWhiteSpace(activity.Command) || (object)KeyPhrases == null || KeyPhrases.Length == 0)
                return false;

            bool isMatch = false;

            // Test for key phrase match
            foreach (KeyPhrase phrase in KeyPhrases)
            {
                switch (MatchStyle)
                {
                    case MatchStyle.StartsWith:
                        if (activity.Command.StartsWith(phrase.CleanValue, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;

                            if (activity.Command.Length > phrase.CleanValue.Length)
                                query = activity.Command.Substring(phrase.CleanValue.Length).Trim();
                        }
                        break;
                    case MatchStyle.EndsWith:
                        if (activity.Command.EndsWith(phrase.CleanValue, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;

                            if (activity.Command.Length > phrase.CleanValue.Length)
                                query = activity.Command.Substring(0, activity.Command.Length - phrase.CleanValue.Length).Trim();
                        }
                        break;
                    case MatchStyle.AnyOrder:
                        isMatch = phrase.Words.All(word => activity.CommandWords.Contains(word));
                        break;
                }

                // Stop testing if a match is found
                if (isMatch)
                    break;
            }

            return isMatch;
        }
    }
}
