//******************************************************************************************************
//  Notes.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/18/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Xml.Linq;
using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents any possible notes associated with a <see cref="Command"/>.
    /// </summary>
    public class Notes
    {
        private string m_value;

        /// <summary>
        /// Notes value, typically a CDATA value.
        /// </summary>
        [XmlText]
        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = XElement.Parse(value).Value;
            }
        }
    }
}
