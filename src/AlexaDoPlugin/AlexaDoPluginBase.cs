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
using System.Windows.Forms;
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

        // Fields

        /// <summary>
        /// Log writer.
        /// </summary>
        protected readonly ILog Log;

        private Action<string, ToolTipIcon, int, bool, ILog> m_showNotificationFunction;
        private Func<Delegate, object[], IAsyncResult> m_beginInvokeFunction;
        private Command m_command;
        private string m_parameters;
        private bool m_enabled;
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

        /// <summary>
        /// Gets or sets parameters for this <see cref="AlexaDoPluginBase"/>.
        /// </summary>
        public virtual string Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = value;
            }
        }

        /// <summary>
        /// Gets or sets enabled state of this <see cref="AlexaDoPluginBase"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes can set enabled to state to <c>false</c> at any time to prevent plug-in from being invoked.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        // Assigns UI notification delegate
        internal Action<string, ToolTipIcon, int, bool, ILog> ShowNotificationFunction
        {
            set
            {
                m_showNotificationFunction = value;
            }
        }

        // Assigns message loop invoke delegate
        internal Func<Delegate, object[], IAsyncResult> BeginInvokeFunction
        {
            set
            {
                m_beginInvokeFunction = value;
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
        /// Initialize plug-in adapter.
        /// </summary>
        /// <remarks>
        /// Derived classes should set <see cref="Enabled"/> state to <c>true</c> upon successful initialization.
        /// </remarks>
        public virtual void Initialize()
        {
            Enabled = true;
        }

        /// <summary>
        /// Plug-in method to process Echo activity.
        /// </summary>
        /// <param name="activity">Echo activity to process.</param>
        /// <param name="query">Remaining command query, if any, when key phrase matched at start or end is removed.</param>
        /// <returns><c>true</c> if activity was successfully processed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Implementor should throw an exception on failure - exception message text will be reported
        /// through [reason] text as specified in any command/response/failure definition text.
        /// </remarks>
        public abstract void ProcessActivity(EchoActivity activity, string query);

        /// <summary>
        /// Displays a UI notification from the tool-bar.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="icon">Icon to use for the message.</param>
        /// <param name="timeout">Message timeout.</param>
        /// <param name="forceDisplay">Flag to force display.</param>
        protected void ShowNotification(string message, ToolTipIcon icon = Settings.DefaultToolTipIcon, int timeout = Settings.DefaultToolTipTimeout, bool forceDisplay = false)
        {
            if ((object)m_showNotificationFunction != null)
            {
                // Notifications are always logged
                m_showNotificationFunction(message, icon, timeout, forceDisplay, Log);
            }
            else
            {
                // Fall back on logging only if notification function is unavailable
                switch (icon)
                {
                    case ToolTipIcon.Warning:
                        Log.Warn(message);
                        break;
                    case ToolTipIcon.Error:
                        Log.Error(message);
                        break;
                    default:
                        Log.Info(message);
                        break;
                }
            }
        }

        /// <summary>
        /// Executes the specified delegate asynchronously with the specified arguments on the main window
        /// control thread for message loop processing.
        /// </summary>
        /// <param name="method">Delegate function to queue for execution on main thread.</param>
        /// <param name="args">Any argument for delegate function.</param>
        /// <returns><see cref="IAsyncResult"/> that represents the result of the BeginInvoke operation.</returns>
        protected IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            if ((object)m_beginInvokeFunction != null)
                return m_beginInvokeFunction(method, args);

            return null;
        }

        #endregion
    }
}
