﻿//******************************************************************************************************
//  EchoMonitor.ProcessActivities.Designer.cs - Gbtc
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
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using AlexaDoPlugin;
using GSF;
using GSF.IO;
using GSF.Units;
using Json;

namespace AlexaDo
{
    public partial class EchoMonitor
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetGetCookie(string lpszUrl, string lpszCookieName, StringBuilder lpszCookieData, ref uint lpdwSize);

        private const string ProcessedActivitiesCacheFileName = "ProcessedActivities.cache";

        private HashSet<EchoActivity> m_processedActivities;
        private long m_totalQueries;
        private bool m_processing;

        private bool ProcessActivities()
        {
            // Make sure to only process one activity query at a time, query timer interval set too small?
            if (m_processing)
                return false;

            try
            {
                m_processing = true;

                if (m_authenticated)
                {
                    Ticks startTime = DateTime.UtcNow;
                    UpdateStatus("Downloading Echo activity data...");

                    string processedCacheFileName = FilePath.GetAbsolutePath(ProcessedActivitiesCacheFileName);
                    bool processCacheUpdated = false;

                    // Deserialize processed activity cache from last run, if any
                    if ((object)m_processedActivities == null)
                    {
                        m_processedActivities = DeserializeProcessedActivitiesCache(processedCacheFileName);
                        processCacheUpdated = true;
                    }

                    string activities;

                    // Have to use WebClient to get JSON data, WebBrowser control tries to download it to a file
                    // prompting UI for a file name - may be able to intercept this using ActiveX API, but the
                    // following seems to work OK:
                    using (WebClient client = new WebClient())
                    {
                        const string url = BaseURL + ActivitiesAPI;
                        uint datasize = 32768;

                        StringBuilder cookieData = new StringBuilder((int)datasize);

                        // Pass authentication data in WebBrowser cookies along to WebClient
                        if (InternetGetCookie(url, null, cookieData, ref datasize) && cookieData.Length > 0)
                            client.Headers.Add(HttpRequestHeader.Cookie, cookieData.ToString());

                        // Make sure we look like the same browser that started the session
                        client.Headers[HttpRequestHeader.UserAgent] = m_userAgent;

                        // Download the JSON Echo Activities data
                        activities = Encoding.UTF8.GetString(client.DownloadData(url));
                    }

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
                            time = (new UnixTimeTag(double.Parse(activityElements["creationTimestamp"].ToString()) * SI.Milli)).ToDateTime();
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
                                processCacheUpdated = true;

                                // Only process successful activities that have occurred recently - note that
                                // if local clock is way off, things may never get processed
                                if (activity.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) &&
                                    Math.Abs((DateTime.UtcNow - activity.Time).TotalSeconds) <= m_timeTolerance)
                                {
                                    UpdateStatus("Processing Echo activity \"[{0}]: {1}\"", activity.Status, activity.Command);

                                    if (activity.Command.StartsWith(m_keyWord, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Remove key word from command
                                        activity.Command = activity.Command.Substring(m_keyWord.Length);

                                        if (m_ttsFeedbackEnabled)
                                            TTSEngine.Speak("Processing command: " + activity.Command);

                                        // TODO: Process plug-ins
                                    }
                                }
                            }

                            // Possible optimization: if activities are always time-sorted, you can break out of loop early...
                        }

                        // Maintain processed Echo activity dictionary size
                        HashSet<EchoActivity> expiredActivities = new HashSet<EchoActivity>();

                        foreach (EchoActivity activity in m_processedActivities)
                        {
                            // If activity no longer appears in JSON list or is beyond time tolerance, mark activity for removal
                            if (!encounteredActivities.Contains(activity))
                                expiredActivities.Add(activity);
                            else if (Math.Abs((DateTime.UtcNow - activity.Time).TotalSeconds) > m_timeTolerance * 2)
                                expiredActivities.Add(activity);
                        }

                        // Remove expired activities
                        if (expiredActivities.Count > 0)
                        {
                            processCacheUpdated = true;

                            foreach (EchoActivity activity in expiredActivities)
                                m_processedActivities.Remove(activity);
                        }

                        // Serialize processed activities cache for future runs
                        if (processCacheUpdated)
                            SerializeProcessedActivitiesCache(processedCacheFileName);

                        UpdateStatus("Query {0:N0} processed {1:N0} Echo activities in {2}", ++m_totalQueries, encounteredActivities.Count, (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2));

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotifcation(string.Format("Failure while processing activities: {0}", ex.Message), ToolTipIcon.Error);
            }
            finally
            {
                m_processing = false;
            }

            return false;
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
                ShowNotifcation(string.Format("Failed to restore processed activities cache: {0}", ex.Message), ToolTipIcon.Error);
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
                ShowNotifcation(string.Format("Failed to persist processed activities cache: {0}", ex.Message), ToolTipIcon.Error);
            }
        }
    }
}
