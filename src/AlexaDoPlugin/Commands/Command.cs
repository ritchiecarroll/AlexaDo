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

using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents a command to be executed consisting of a trigger, action and a response.
    /// </summary>
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
    }
}
