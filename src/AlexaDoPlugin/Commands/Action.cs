//******************************************************************************************************
//  Action.cs - Gbtc
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
    /// Represents the action to perform for a <see cref="Command"/>.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Assembly name that contains the action to perform.
        /// </summary>
        [XmlElement("assemblyName")]
        public string AssemblyName;

        /// <summary>
        /// Type name of the action to perform.
        /// </summary>
        [XmlElement("typeName")]
        public string TypeName;

        /// <summary>
        /// Parameters to use for the action to perform.
        /// </summary>
        [XmlElement("parameters")]
        public Parameters Parameters;
    }
}
