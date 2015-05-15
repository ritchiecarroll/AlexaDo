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

namespace AlexaDoPlugin
{
    /// <summary>
    /// Enumeration of possible match styles when testing for key phrases in a command trigger.
    /// </summary>
    public enum MatchStyle
    {
        /// <summary>
        /// Command starts with key phrase, in order of definition.
        /// </summary>
        StartsWith,
        /// <summary>
        /// Command ends with key phrase, in order of definition.
        /// </summary>
        EndsWith,
        /// <summary>
        /// Command contains key phrase, in any order.
        /// </summary>
        AnyOrder
    }

    /// <summary>
    /// Represents a key phrase used to trigger a command.
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

    /// <summary>
    /// Represents the possible key phrases and match style that will trigger a command.
    /// </summary>
    [XmlType("trigger")]
    public class Trigger
    {
        /// <summary>
        /// Array of key phrases that will trigger a command.
        /// </summary>
        [XmlIgnore]
        public KeyPhrase[] KeyPhrases;

        /// <summary>
        /// Pipe separated list of possible key phrase values.
        /// </summary>
        [XmlElement(ElementName = "keyPhrase")]
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
                }).ToArray();
            }
        }

        /// <summary>
        /// Match style to use when evaluating key phrases.
        /// </summary>
        [XmlElement(ElementName = "matchStyle")]
        public MatchStyle MatchStyle;
    }

    [XmlType("command")]
    public class Command
    {
        [XmlAttribute(AttributeName = "description")]
        public string Description;

        [XmlAttribute(AttributeName = "usage")]
        public string Usage;

        [XmlAttribute(AttributeName = "enabled")]
        public bool Enabled;

        [XmlElement(ElementName = "trigger")]
        public Trigger Trigger;
    }

    [XmlRoot("commands")]
    public class PluginCommands
    {
        [XmlArray(ElementName = "commands")]
        public Command[] Commands;
    }
}
