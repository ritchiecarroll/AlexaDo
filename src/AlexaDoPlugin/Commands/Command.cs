//******************************************************************************************************
//  Command.cs - Gbtc
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

using System;
using System.Linq;
using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents a command to be executed consisting of a trigger, action and a response.
    /// </summary>
    [XmlType("command")]
    public class Command
    {
        /// <summary>
        /// Command description.
        /// </summary>
        [XmlAttribute("description")]
        public string Description;

        /// <summary>
        /// Command usage.
        /// </summary>
        [XmlAttribute("usage")]
        public string Usage;

        /// <summary>
        /// Command enabled flag.
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled;

        /// <summary>
        /// Command trigger.
        /// </summary>
        [XmlElement("trigger")]
        public Trigger Trigger;

        /// <summary>
        /// Command action.
        /// </summary>
        [XmlElement("action")]
        public Action Action;

        /// <summary>
        /// Command response.
        /// </summary>
        [XmlElement("response")]
        public Response Response;

        /// <summary>
        /// Any associated notes.
        /// </summary>
        [XmlElement("notes", IsNullable = true)]
        public Notes Notes;

        // TODO: add out query string parameter (for [query] replacement) that will remove key phrase upon start/end match

        /// <summary>
        /// Determines if Echo activity is a match for this <see cref="Command"/>.
        /// </summary>
        /// <param name="activity">Echo activity to test for match.</param>
        /// <returns><c>true</c> if <paramref name="activity"/> is a match for this command; otherwise, <c>false</c>.</returns>
        public bool IsMatch(EchoActivity activity)
        {
            // Validate inputs and outputs needed for test
            if (string.IsNullOrWhiteSpace(activity.Command) || (object)Trigger == null || (object)Trigger.KeyPhrases == null || Trigger.KeyPhrases.Length == 0)
                return false;

            bool isMatch = false;

            // Test for key phrase match
            foreach (KeyPhrase phrase in Trigger.KeyPhrases)
            {
                switch (Trigger.MatchStyle)
                {
                    case MatchStyle.StartsWith:
                        isMatch = activity.Command.StartsWith(phrase.CleanValue, StringComparison.OrdinalIgnoreCase);
                        break;
                    case MatchStyle.EndsWith:
                        isMatch = activity.Command.EndsWith(phrase.CleanValue, StringComparison.OrdinalIgnoreCase);
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
