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

using System.IO;
using System.ServiceModel;
using System.Xml.Serialization;
using GSF;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Collection of commands defined in an action file.
    /// </summary>
    [XmlRoot("commands"), XmlSerializerFormat]
    public class PluginCommands
    {
        /// <summary>
        /// Array of defined commands.
        /// </summary>
        [XmlElement("command")]
        public Command[] Commands;

        /// <summary>
        /// Serializes this <see cref="PluginCommands"/> instance.
        /// </summary>
        /// <param name="fileName">File name to serialize.</param>
        public void Save(string fileName)
        {
            using (FileStream stream = File.OpenWrite(fileName))
                Serialization.Serialize(this, SerializationFormat.Xml, stream);
        }

        /// <summary>
        /// Deserializes a <see cref="PluginCommands"/> instance from the specified <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">File name to deserialize.</param>
        /// <returns>New <see cref="PluginCommands"/> instance deserialized from <paramref name="fileName"/>.</returns>
        public static PluginCommands Parse(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return Serialization.Deserialize<PluginCommands>(stream, SerializationFormat.Xml);
        }
    }
}
