//******************************************************************************************************
//  PluginManager.cs - Gbtc
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlexaDoPlugin.Commands;
using GSF;
using GSF.IO;
using GSF.Security.Cryptography;
using log4net;

namespace AlexaDoPlugin
{
    public class PluginManager : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Raised when an <see cref="EchoActivity"/> is successfully processed by a plug-in.
        /// </summary>
        public event EventHandler<EventArgs<EchoActivity>> ProcessedActivity;

        // Fields
        private readonly Action<string, ToolTipIcon, int, bool, ILog> m_showNotificationFunction;
        private readonly AlexaDoPluginBase[] m_plugins;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PluginManager"/> instance.
        /// </summary>
        /// <param name="showNotificationFunction">Show notification function.</param>
        /// <param name="beginInvokeFunction">Main thread invocation function.</param>
        public PluginManager(Action<string, ToolTipIcon, int, bool, ILog> showNotificationFunction, Func<Delegate, object[], IAsyncResult> beginInvokeFunction)
        {
            const CipherStrength CryptoStrength = CipherStrength.Aes256;                // Desired cipher strength for encrypted parameters
            const string DefaultCryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";     // Dictionary key used to deserialize needed Key/IV for decryption

            try
            {
                m_showNotificationFunction = showNotificationFunction;

                List<AlexaDoPluginBase> plugins = new List<AlexaDoPluginBase>();

                // Find all command definition files
                foreach (string commandsDefintionFile in FilePath.GetFileList(FilePath.GetAbsolutePath("*.commands")))
                {
                    try
                    {
                        // Parse command definition file and attempt to load each command action
                        foreach (Command command in PluginCommands.Parse(commandsDefintionFile).Commands)
                        {
                            // Skip loading plug-in if it is marked as not enabled in the definition file
                            if (!command.Enabled)
                                continue;

                            try
                            {
                                // Load assembly and plug-in type
                                Assembly assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(command.Action.AssemblyName));
                                Type type = assembly.GetType(command.Action.TypeName);

                                // Make sure specified type derives from AlexaDoPluginBase
                                if (type.IsSubclassOf(typeof(AlexaDoPluginBase)))
                                {
                                    // Create a new instance of plug-in
                                    AlexaDoPluginBase plugin = (AlexaDoPluginBase)Activator.CreateInstance(type);

                                    // Assign associated command and delegate handlers
                                    plugin.Command = command;
                                    plugin.ShowNotificationFunction = showNotificationFunction;
                                    plugin.BeginInvokeFunction = beginInvokeFunction;

                                    // Decrypt parameters, if encrypted
                                    if (command.Action.Parameters.Encrypted && !string.IsNullOrWhiteSpace(command.Action.Parameters.Value))
                                        plugin.Parameters = command.Action.Parameters.Value.Decrypt(DefaultCryptoKey, CryptoStrength);
                                    else
                                        plugin.Parameters = command.Action.Parameters.Value;

                                    // Add loaded plug-in to list of plug-ins
                                    plugins.Add(plugin);
                                    Log.InfoFormat("Loaded AlexaDo plug-in command \"{0}\" [{1}] from \"{2}\"", command.Description, plugin.GetType().FullName, commandsDefintionFile);
                                }
                                else
                                {
                                    ShowNotification($"Failed to load AlexaDo plug-in command \"{command.Description}\" from \"{commandsDefintionFile}\": {command.Action.TypeName} is not derived from AlexaDoPluginBase", ToolTipIcon.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Catch failure to load command assembly or create plug-in instance
                                ShowNotification($"Failed to load AlexaDo plug-in command \"{command.Description}\" from \"{commandsDefintionFile}\": {ex.Message}", ToolTipIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Catch failure to parse commands definition XML
                        ShowNotification($"Failed to load AlexaDo plug-in commands from \"{commandsDefintionFile}\": {ex.Message}", ToolTipIcon.Error);
                    }
                }

                m_plugins = plugins.ToArray();

                if (m_plugins.Length > 0)
                {
                    // Start plug-in initialization on a background thread
                    Thread initializePlugins = new Thread(state =>
                    {
                        Ticks overAllStartTime = DateTime.UtcNow.Ticks;

                        foreach (AlexaDoPluginBase plugin in m_plugins)
                        {
                            try
                            {
                                Ticks startTime = DateTime.UtcNow.Ticks;
                                plugin.Initialize();
                                Log.InfoFormat("Initialized AlexaDo plug-in command \"{0}\" [{1}] - total time: {2}", plugin.Command.Description, plugin.GetType().FullName, (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2));

                            }
                            catch (Exception ex)
                            {
                                ShowNotification($"Failed to initialize AlexaDo plug-in command \"{plugin.Command.Description}\" [{plugin.GetType().FullName}]: {ex.Message}", ToolTipIcon.Error);
                            }
                        }

                        Log.InfoFormat("Initialization completed for all AlexaDo plug-ins - total time: {0}", (DateTime.UtcNow.Ticks - overAllStartTime).ToElapsedTimeString(2));
                    });

                    initializePlugins.IsBackground = true;
                    initializePlugins.Start();
                }
                else
                {
                    ShowNotification("No AlexaDo plug-ins loaded - no command actions will be handled.", ToolTipIcon.Warning, forceDisplay: true);
                }
            }
            catch (Exception ex)
            {
                // Catch any other exceptions
                m_plugins = new AlexaDoPluginBase[0];
                ShowNotification($"Failed to load AlexaDo plug-ins: {ex.Message}", ToolTipIcon.Error, forceDisplay: true);
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to collection of loaded AlexaDo plug-ins.
        /// </summary>
        public ReadOnlyCollection<AlexaDoPluginBase> Plugins => new ReadOnlyCollection<AlexaDoPluginBase>(m_plugins);

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
        /// <returns>Count of plug-ins that processed activity.</returns>
        public int ProcessActivity(EchoActivity activity)
        {
            // If there is no activity command, nothing can be processed
            if (string.IsNullOrWhiteSpace(activity.Command))
                return 0;

            string query;
            int processed = 0;

            foreach (AlexaDoPluginBase plugin in m_plugins)
            {
                // If plug-in is disabled or there is no associated command/trigger for the plug-in, nothing can be processed
                if (!plugin.Enabled || (object)plugin.Command == null || (object)plugin.Command.Trigger == null)
                    continue;

                // See if this plug-in command trigger matches what was heard in the activity
                if (plugin.Command.Trigger.IsMatch(activity, out query))
                {
                    try
                    {
                        // Attempt to process activity with any remaining query text
                        plugin.ProcessActivity(activity, query);
                        processed++;

                        // Handle plug-in success response
                        if ((object)plugin.Command.Response != null && (object)plugin.Command.Response.Succeeded != null)
                            plugin.Command.Response.Succeeded.ProcessResponse();

                        Log.InfoFormat("Successfully processed Echo activity [{0}]: {1} \"{2}\"", activity.Status, activity.ID, activity.Command);
                        OnProcessedActivity(plugin, activity);
                    }
                    catch (Exception ex)
                    {
                        // Handle plug-in failure response
                        if ((object)plugin.Command.Response != null && (object)plugin.Command.Response.Failed != null)
                            plugin.Command.Response.Failed.ProcessResponse(ex.Message);

                        Log.WarnFormat("Failed to processed Echo activity [{0}]: {1} \"{2}\" - {3}", activity.Status, activity.ID, activity.Command, ex.Message.ToNonNullString());
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

        private void ShowNotification(string message, ToolTipIcon icon = Settings.DefaultToolTipIcon, int timeout = Settings.DefaultToolTipTimeout, bool forceDisplay = false)
        {
            if ((object)m_showNotificationFunction != null)
            {
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

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginManager));

        #endregion
    }
}
