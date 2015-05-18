//******************************************************************************************************
//  EchoActivity.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace AlexaDoPlugin
{
    /// <summary>
    /// Defines a data structure for relevant Echo activity data.
    /// </summary>
    [Serializable]
    public struct EchoActivity : IEquatable<EchoActivity>, IComparable<EchoActivity>
    {
        public readonly string Status;
        public readonly DateTime Time;
        public readonly string ID;
        public string Command;

        public EchoActivity(string status, DateTime time, string id, string command)
        {
            Status = status;
            Time = time;
            ID = id;
            Command = command;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(ID))
                return 0;

            return ID.GetHashCode();
        }

        public int CompareTo(EchoActivity other)
        {
            return string.CompareOrdinal(ID, other.ID);
        }

        public override bool Equals(object obj)
        {
            if (obj is EchoActivity)
                return Equals((EchoActivity)obj);

            return false;
        }

        public bool Equals(EchoActivity other)
        {
            return string.CompareOrdinal(ID, other.ID) == 0;
        }
    }
}
