﻿//******************************************************************************************************
//  ActivityProcessor.cs - Gbtc
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AlexaDoPlugin;
using GSF;
using GSF.IO;
using GSF.Threading;
using GSF.Units;
using Json;
using log4net;

namespace AlexaDo
{
    /// <summary>
    /// Processes Amazon Echo activities as dispatches commands to plug-ins.
    /// </summary>
    public class ActivityProcessor : IDisposable
    {
        #region [ Members ]

        // Constants
        private const string ProcessedActivitiesCacheFileName = "ProcessedActivities.cache";

        // Fields
        private readonly EchoMonitor m_echoMonitor;
        private readonly PluginManager m_pluginManager;
        private readonly ShortSynchronizedOperation m_processActivites;
        private HashSet<EchoActivity> m_processedActivities;
        private bool m_navigationComplete;
        private bool m_applicationClosing;
        private long m_totalQueries;
        private int m_processing;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ActivityProcessor"/> instance.
        /// </summary>
        /// <param name="echoMonitor">Parent <see cref="EchoMonitor"/> form used for user feedback processing.</param>
        public ActivityProcessor(EchoMonitor echoMonitor)
        {
            m_echoMonitor = echoMonitor;
            m_pluginManager = new PluginManager(ShowNotification, echoMonitor.BeginInvoke);
            m_processActivites = new ShortSynchronizedOperation(ProcessNewActivities);

            // Attach to needed events
            m_echoMonitor.FormClosing += m_echoMonitor_FormClosing;
            m_echoMonitor.ActivityBrowser.DocumentCompleted += BrowserControl_DocumentCompleted;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Total Echo activity queries.
        /// </summary>
        public long TotalQueries => m_totalQueries;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ActivityProcessor"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ActivityProcessor"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_pluginManager?.Dispose();
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Processes current activities.
        /// </summary>
        public bool ProcessActivities()
        {
            m_processActivites.RunOnce();
            return m_processActivites.IsPending;
        }

        private void ProcessNewActivities()
        {
            if (!Settings.Authenticated)
                return;

            // Make sure to only process one activity query at a time - even if testing
            if (Thread.VolatileRead(ref m_processing) != 0)
                return;

            try
            {
                Interlocked.Exchange(ref m_processing, 1);

                Ticks startTime = DateTime.UtcNow;
                UpdateStatus("Downloading Echo activity data...");

                string processedCacheFileName = Path.Combine(FilePath.GetApplicationDataFolder(), ProcessedActivitiesCacheFileName);
                bool processedCacheUpdated = false;

                // Deserialize processed activity cache from last run, if any
                if ((object)m_processedActivities == null)
                {
                    m_processedActivities = DeserializeProcessedActivitiesCache(processedCacheFileName);
                    processedCacheUpdated = true;
                }

                // Download the JSON Echo Activities data
                string activities = Navigate(Settings.BaseURL + Settings.ActivitiesAPI + Settings.QueryTopFiveActivities);

                if (!string.IsNullOrEmpty(activities))
                {
                    UpdateStatus("Parsing Echo activity data...");

                    HashSet<EchoActivity> encounteredActivities = new HashSet<EchoActivity>();
                    string status, id, command;
                    DateTime time;

                    dynamic json = JsonParser.Deserialize(activities);

                    for (int i = 0; i < json.activities.Count; i++)
                    {
                        // Parse key activity elements
                        Dictionary<string, object> activityElements = json.activities[i];
                        status = activityElements["activityStatus"].ToString();
                        time = new UnixTimeTag((decimal)(double.Parse(activityElements["creationTimestamp"].ToString()) * SI.Milli)).ToDateTime();
                        id = activityElements["id"].ToString();
                        command = ParseEchoSpeechSummary(activityElements["description"].ToString());

                        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(status))
                            continue;

                        EchoActivity activity = new EchoActivity(status, time, id, command);

                        encounteredActivities.Add(activity);

                        if (!m_processedActivities.Contains(activity))
                        {
                            // Mark activity as processed
                            m_processedActivities.Add(activity);
                            processedCacheUpdated = true;

                            // Only process activities that have occurred recently - note that
                            // if local clock is way off, things may never get processed
                            if (Math.Abs((DateTime.UtcNow - activity.Time).TotalSeconds) <= Settings.TimeTolerance)
                                ProcessActivity(activity);
                        }

                        // Possible optimization: if activities are always time-sorted, you can break out of loop early...
                    }

                    // Maintain processed Echo activity cache size
                    HashSet<EchoActivity> expiredActivities = new HashSet<EchoActivity>();

                    foreach (EchoActivity activity in m_processedActivities)
                    {
                        // If activity no longer appears in JSON list or is beyond time tolerance, mark activity for removal
                        if (!encounteredActivities.Contains(activity))
                            expiredActivities.Add(activity);
                        else if (Math.Abs((DateTime.UtcNow - activity.Time).TotalSeconds) > Settings.TimeTolerance * 2)
                            expiredActivities.Add(activity);
                    }

                    // Remove expired activities
                    if (expiredActivities.Count > 0)
                    {
                        processedCacheUpdated = true;

                        foreach (EchoActivity activity in expiredActivities)
                            m_processedActivities.Remove(activity);
                    }

                    // Serialize processed activities cache for future runs
                    if (processedCacheUpdated)
                        SerializeProcessedActivitiesCache(processedCacheFileName);

                    UpdateStatus("Query {0:N0} processed {1:N0} Echo activities in {2}", ++m_totalQueries, encounteredActivities.Count, (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2));
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Failure while processing activities: {ex.Message}", ToolTipIcon.Error);
            }
            finally
            {
                Interlocked.Exchange(ref m_processing, 0);
            }
        }

        /// <summary>
        /// Tests an activity based on specified command.
        /// </summary>
        /// <param name="command">Command to trigger.</param>
        public void TestActivity(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            // Make sure to only process one activity query at a time - even if testing
            if (Thread.VolatileRead(ref m_processing) != 0)
                return;

            try
            {
                Interlocked.Exchange(ref m_processing, 1);

                if (command.StartsWith(Settings.StartKeyWord, StringComparison.OrdinalIgnoreCase))
                    ProcessActivity(new EchoActivity("SUCCESS", DateTime.UtcNow, "TestActivity", command));
                else
                    ProcessActivity(new EchoActivity("SYSTEM_ABANDONED", DateTime.UtcNow, "TestActivity", command));
            }
            catch (Exception ex)
            {
                ShowNotification($"Failure while processing test activity: {ex.Message}", ToolTipIcon.Error);
            }
            finally
            {
                Interlocked.Exchange(ref m_processing, 0);
            }
        }

        // Process an activity
        private void ProcessActivity(EchoActivity activity)
        {
            bool processCommand = false;

            // Check for commands ending with end key word, e.g, "Stop"
            bool endsWithKeyWord = activity.Command.EndsWith(Settings.EndKeyWord, StringComparison.OrdinalIgnoreCase);

            if (endsWithKeyWord || activity.Status.Equals("SYSTEM_ABANDONED", StringComparison.OrdinalIgnoreCase))
            {
                processCommand = true;

                // Remove end key word from command, if it exists
                if (endsWithKeyWord)
                    activity.Command = activity.Command.Substring(0, activity.Command.Length - Settings.EndKeyWord.Length - 1);
            }

            // Also check for commands beginning with key word, e.g., "Simon Says"
            if (!processCommand &&
                activity.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) &&
                activity.Command.StartsWith(Settings.StartKeyWord, StringComparison.OrdinalIgnoreCase))
            {
                processCommand = true;

                // Remove key word from command
                activity.Command = activity.Command.Substring(Settings.StartKeyWord.Length + 1);
            }

            if (processCommand)
            {
                UpdateStatus("Processing Echo activity [{0}]: {1} \"{2}\"", activity.Status, activity.ID, activity.Command);
                int processed = m_pluginManager.ProcessActivity(activity);
                Log.Info($"{processed:N0} plug-in{(processed == 1 ? "" : "s")} processed Echo activity [{activity.Status}]: {activity.ID} \"{activity.Command}\"");
            }
        }

        private static string ParseEchoSpeechSummary(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "";

            Dictionary<string, string> values = description.Substring(1, description.Length - 2).ParseKeyValuePairs(',', ':');
            string summary;

            if (values.TryGetValue("summary", out summary))
                return summary;

            return string.Empty;
        }

        private HashSet<EchoActivity> DeserializeProcessedActivitiesCache(string processedCacheFileName)
        {
            try
            {
                if (File.Exists(processedCacheFileName))
                    using (FileStream stream = File.OpenRead(processedCacheFileName))
                        return Serialization.Deserialize<HashSet<EchoActivity>>(stream, SerializationFormat.Binary);
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to restore processed activities cache: {ex.Message}", ToolTipIcon.Error);
            }

            return new HashSet<EchoActivity>();
        }

        private void SerializeProcessedActivitiesCache(string processedCacheFileName)
        {
            try
            {
                using (FileStream stream = File.OpenWrite(processedCacheFileName))
                    Serialization.Serialize(m_processedActivities, SerializationFormat.Binary, stream);
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to persist processed activities cache: {ex.Message}", ToolTipIcon.Error);
            }
        }

        // Proxy notifications to UI message loop
        private void ShowNotification(string message, ToolTipIcon icon = Settings.DefaultToolTipIcon, int timeout = Settings.DefaultToolTipTimeout, bool forceDisplay = false, ILog logger = null)
        {
            if ((object)logger == null)
                logger = Log;

            m_echoMonitor.BeginInvoke((Action<string, ToolTipIcon, int, bool, ILog>)m_echoMonitor.ShowNotification, message, icon, timeout, forceDisplay, logger);
        }

        // Proxy status updates to UI message loop
        private void UpdateStatus(string message, params object[] args)
        {
            m_echoMonitor.BeginInvoke((Action<string, object[]>)m_echoMonitor.UpdateStatus, message, args);
        }

        // Navigates to a URL and waits until document is loaded.
        private string Navigate(string url)
        {
            m_navigationComplete = false;
            m_echoMonitor.ActivityBrowser.Navigate(url, null, null, "User-Agent: " + Settings.UserAgent);

            while (!m_navigationComplete && !m_applicationClosing)
                Application.DoEvents();

            return m_echoMonitor.Invoke(new Func<string>(() => m_echoMonitor.ActivityBrowser.DocumentText)) as string;
        }

        private void m_echoMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel any pending navigation waiters when application is exiting - note that the UserClosing
            // reason is an exception since this just minimizes application to the task area
            m_applicationClosing = e.CloseReason != CloseReason.UserClosing;
        }

        private void BrowserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            m_navigationComplete = true;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(ActivityProcessor));

        #endregion
    }
}

#region [ Old Code ]

// Passing existing cookies to WebClient no longer maintains authentication

//using (WebClient client = new WebClient())
//{
//    const string url = Settings.BaseURL + Settings.ActivitiesAPI + Settings.QueryTopFiveActivities;
//    uint datasize = 32768;

//    StringBuilder cookieData = new StringBuilder((int)datasize);

//    // Pass authentication data in WebBrowser cookies along to WebClient
//    if (InternetGetCookie(url, null, cookieData, ref datasize) && cookieData.Length > 0)
//        client.Headers.Add(HttpRequestHeader.Cookie, cookieData.ToString());

//    // Make sure we look like the same browser that started the session
//    client.Headers[HttpRequestHeader.UserAgent] = Settings.UserAgent;

//    // Download the JSON Echo Activities data
//    activities = Encoding.UTF8.GetString(client.DownloadData(url));
//}

// Static Methods
//[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
//private static extern bool InternetGetCookie(string lpszUrl, string lpszCookieName, StringBuilder lpszCookieData, ref uint lpdwSize);

#endregion