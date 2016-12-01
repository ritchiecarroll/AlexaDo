//******************************************************************************************************
//  DisarmSystem.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using AlexaDoPlugin;

namespace FrontPointSecurity
{
    public class DisarmSystem : AlexaDoPluginBase
    {
        // What we really need here is an API from Alarm.com, in-lieu of that, here you go...

        /// <summary>
        /// Plug-in method to process Echo activity.
        /// </summary>
        /// <param name="activity">Echo activity to process.</param>
        /// <param name="query">Remaining command query, if any, when key phrase matched at start or end is removed.</param>
        /// <remarks>
        /// Implementor should throw an exception on failure - exception message text will be reported
        /// through [reason] text as specified in any command/response/failure definition text.
        /// </remarks>
        public override void ProcessActivity(EchoActivity activity, string query)
        {
            throw new System.NotImplementedException();
        }
    }
}
