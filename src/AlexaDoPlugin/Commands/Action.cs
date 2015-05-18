//******************************************************************************************************
//  Action.cs - Gbtc
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
    /// Represents the action to perform for a <see cref="Command"/>.
    /// </summary>
    public class Action
    {
        [XmlElement("assemblyName")]
        public string AssemblyName;

        [XmlElement("typeName")]
        public string TypeName;

        [XmlElement("parameters")]
        public Parameters Parameters;
    }
}
