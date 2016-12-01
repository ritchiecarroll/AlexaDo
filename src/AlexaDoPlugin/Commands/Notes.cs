//******************************************************************************************************
//  Notes.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/18/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents any possible notes associated with a <see cref="Command"/>.
    /// </summary>
    public class Notes : IXmlSerializable
    {
        private string m_value;

        /// <summary>
        /// Notes value, typically a CDATA value.
        /// </summary>
        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                // Read section string or CDATA
                XmlDocument document = new XmlDocument();
                document.Load(reader);

                XPathNavigator navigator = document.CreateNavigator();
                navigator.MoveToChild(XPathNodeType.Element);

                Value = navigator.InnerXml.Trim().StartsWith("&lt;") ? navigator.Value : navigator.InnerXml;
            }
            catch (Exception ex)
            {
                // Not a major catastrophe - notes are not used programatically
                Value = "Failed to load notes: " + ex.Message;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteCData(Value);
        }
    }
}
