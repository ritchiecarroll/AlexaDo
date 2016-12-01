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

using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents parameters, e.g., key value pairs, for an <see cref="Action"/>.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Parameter encrypted flag.
        /// </summary>
        [XmlAttribute(AttributeName = "encrypted")]
        public bool Encrypted;

        /// <summary>
        /// Parameter value.
        /// </summary>
        [XmlText]
        public string Value;
    }
}
