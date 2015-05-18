//******************************************************************************************************
//  Response.cs - Gbtc
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

using System.Xml.Serialization;

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Represents the response to return, if any, when a <see cref="Command"/> succeeds or fails.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Success response, if any.
        /// </summary>
        [XmlElement("succeeded", IsNullable = true)]
        public ResponseMessage Succeeded;

        /// <summary>
        /// Failure response, if any.
        /// </summary>
        [XmlElement("failed", IsNullable = true)]
        public ResponseMessage Failed;
    }
}
