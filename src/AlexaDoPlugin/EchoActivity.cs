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
using System.Collections.Generic;
using GSF;

namespace AlexaDoPlugin
{
    /// <summary>
    /// Defines a data structure for relevant Echo activity data.
    /// </summary>
    [Serializable]
    public struct EchoActivity : IEquatable<EchoActivity>, IComparable<EchoActivity>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Activity status.
        /// </summary>
        public readonly string Status;

        /// <summary>
        /// Activity time.
        /// </summary>
        public readonly DateTime Time;

        /// <summary>
        /// Activity ID.
        /// </summary>
        public readonly string ID;

        private string m_command;

        [NonSerialized]
        private HashSet<string> m_commandWords;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="EchoActivity"/>
        /// </summary>
        /// <param name="status"></param>
        /// <param name="time"></param>
        /// <param name="id"></param>
        /// <param name="command"></param>
        public EchoActivity(string status, DateTime time, string id, string command)
        {
            Status = status;
            Time = time;
            ID = id;
            m_command = CleanUpCommand(command);
            m_commandWords = null;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets activity command, i.e., heard speech.
        /// </summary>
        public string Command
        {
            get
            {
                return m_command ?? "";
            }
            set
            {
                m_command = CleanUpCommand(value);
            }
        }

        /// <summary>
        /// Get set of words parsed from <see cref="Command"/>.
        /// </summary>
        public HashSet<string> CommandWords
        {
            get
            {
                if ((object)m_commandWords == null)
                    m_commandWords = new HashSet<string>(Command.Split(' '), StringComparer.OrdinalIgnoreCase);

                return m_commandWords;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(ID))
                return 0;

            return ID.GetHashCode();
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// </returns>
        public int CompareTo(EchoActivity other)
        {
            return string.CompareOrdinal(ID, other.ID);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="obj">Another object to compare to.</param>
        public override bool Equals(object obj)
        {
            if (obj is EchoActivity)
                return Equals((EchoActivity)obj);

            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(EchoActivity other)
        {
            return string.CompareOrdinal(ID, other.ID) == 0;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Clean up activity command string (makes match processing easier)
        private static string CleanUpCommand(string command)
        {
            return command.ToNonNullString().Trim().RemoveDuplicateWhiteSpace();
        }

        #endregion
    }
}
