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

using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    // Definitions:

    //     Trigger: A set of key phrases and a match style that will trigger a command
    //  Key Phrase: List of words that will trigger a command
    // Match Style: How to match words of a key phrase in order to trigger a command





    /// <summary>
    /// Represents a command to be executed consisting of a trigger, action and response.
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
    }

    /// <summary>
    /// Collection of commands defined in an action file.
    /// </summary>
    [XmlRoot("commands")]
    public class PluginCommands
    {
        /// <summary>
        /// Array of defined commands.
        /// </summary>
        [XmlArray("commands")]
        public Command[] Commands;
    }
}
