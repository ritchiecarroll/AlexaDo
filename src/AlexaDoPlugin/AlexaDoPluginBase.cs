//******************************************************************************************************
//  AlexaDoPlugin.cs - Gbtc
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
using AlexaDoPlugin.Commands;
using log4net;

namespace AlexaDoPlugin
{
    /// <summary>
    /// Defines base class for AlexaDo plug-ins.
    /// </summary>
    public abstract class AlexaDoPluginBase : IDisposable
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields

        /// <summary>
        /// Log writer.
        /// </summary>
        protected readonly ILog Log;

        private Command m_command;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AlexaDoPluginBase"/> class instance.
        /// </summary>
        protected AlexaDoPluginBase()
        {
            // Get a logger specific to derived plug-in
            Log = LogManager.GetLogger(GetType());
            Log.InfoFormat("Created a new {0} plug-in instance", GetType().FullName);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets associated <see cref="AlexaDoPlugin.Commands.Command"/> definition.
        /// </summary>
        public Command Command
        {
            get
            {
                return m_command;
            }
            internal set
            {
                m_command = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AlexaDoPluginBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AlexaDoPluginBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        Log.DebugFormat("{0} plug-instance is disposing", GetType().FullName);
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Plug-in method to process Echo activity.
        /// </summary>
        /// <param name="activity">Echo activity to process.</param>
        /// <param name="failureReason">Reason for any failure.</param>
        /// <returns><c>true</c> if activity was successfully processed; otherwise, <c>false</c>.</returns>
        public abstract bool ProcessActivity(EchoActivity activity, out string failureReason);

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
    }
}
