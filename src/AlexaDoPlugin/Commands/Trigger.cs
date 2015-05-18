//******************************************************************************************************
//  PluginCommands.cs - Gbtc
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
using System.Xml.Serialization;
using GSF;

namespace AlexaDoPlugin.Commands
{
    // Helpful definitions:

    //     Trigger: A set of key phrases and a match style that will trigger a command
    //  Key Phrase: List of words that will trigger a command
    // Match Style: How to match words of a key phrase in order to trigger a command

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
                KeyPhrases = value.ToNonNullString().Split('|').Select(keyPhrase =>
                    new KeyPhrase
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
    }
}
