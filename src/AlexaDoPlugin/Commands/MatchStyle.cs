//******************************************************************************************************
//  MatchStyle.cs - Gbtc
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

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Enumeration of possible match styles when testing for key phrases in a command <see cref="Trigger"/>.
    /// </summary>
    public enum MatchStyle
    {
        /// <summary>
        /// Command starts with key phrase, words matched in order of definition.
        /// </summary>
        StartsWith,
        /// <summary>
        /// Command ends with key phrase, words matched in order of definition.
        /// </summary>
        EndsWith,
        /// <summary>
        /// Command contains key phrase, words matched in any order.
        /// </summary>
        AnyOrder
    }
}