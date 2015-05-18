//******************************************************************************************************
//  PluginManager.cs - Gbtc
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

using System;
using System.Collections.ObjectModel;
using GSF;
using log4net;

namespace AlexaDoPlugin
{
    public class PluginManager : IDisposable
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        /// <summary>
        /// Raised when an <see cref="EchoActivity"/> is successfully processed by a plug-in.
        /// </summary>
        public event EventHandler<EventArgs<EchoActivity>> ProcessedActivity;

        // Fields
        private AlexaDoPluginBase[] m_plugins;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PluginManager"/> instance.
        /// </summary>
        public PluginManager()
        {
            m_plugins = new AlexaDoPluginBase[0];
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to collection of loaded AlexaDo plug-ins.
        /// </summary>
        public ReadOnlyCollection<AlexaDoPluginBase> Plugins
        {
            get
            {
                return new ReadOnlyCollection<AlexaDoPluginBase>(m_plugins);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PluginManager"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PluginManager"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        foreach (AlexaDoPluginBase plugin in m_plugins)
                            plugin.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Pass Echo activity to plug-ins for processing. 
        /// </summary>
        /// <param name="activity">Echo activity to process.</param>
        /// <returns>Count ot plug-ins that processed activity.</returns>
        public int ProcessActivity(EchoActivity activity)
        {
            // If there is no activity command, nothing can be processed
            if (string.IsNullOrWhiteSpace(activity.Command))
                return 0;

            string failureReason;
            int processed = 0;

            foreach (AlexaDoPluginBase plugin in m_plugins)
            {
                // If there is no associated command for the plug-in, nothing can be processed
                if ((object)plugin.Command == null)
                    continue;

                // See if this plug-in command matches what was heard in the activity
                if (plugin.Command.IsMatch(activity))
                {
                    if (plugin.ProcessActivity(activity, out failureReason))
                    {
                        processed++;

                        if ((object)plugin.Command.Response != null && (object)plugin.Command.Response.Succeeded != null)
                            plugin.Command.Response.Succeeded.ProcessResponse();

                        Log.InfoFormat("Successfully processed Echo activity [{0}]: {1} \"{2}\"", activity.Status, activity.ID, activity.Command);
                        OnProcessedActivity(plugin, activity);
                    }
                    else
                    {
                        if ((object)plugin.Command.Response != null && (object)plugin.Command.Response.Failed != null)
                            plugin.Command.Response.Failed.ProcessResponse(failureReason);

                        Log.InfoFormat("Failed to processed Echo activity [{0}]: {1} \"{2}\" - {3}", activity.Status, activity.ID, activity.Command, failureReason.ToNonNullString());
                    }
                }
            }

            return processed;
        }

        /// <summary>
        /// Raises the <see cref="ProcessedActivity"/> event for this <see cref="AlexaDoPluginBase"/> instance.
        /// </summary>
        /// <param name="source">Plug-in that processed the Echo activity.</param>
        /// <param name="activity">The Echo activity that was successfully processed.</param>
        protected virtual void OnProcessedActivity(AlexaDoPluginBase source, EchoActivity activity)
        {
            if ((object)ProcessedActivity != null)
                ProcessedActivity(source, new EventArgs<EchoActivity>(activity));
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginManager));

        #endregion
    }
}
