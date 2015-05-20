//******************************************************************************************************
//  Execute.cs - Gbtc
//
//  Copyright © 2015, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/19/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AlexaDoPlugin;
using GSF;
using GSF.Units;

namespace AppLauncher
{
    /// <summary>
    /// Enumeration of defined encodings for query parameters.
    /// </summary>
    public enum QueryEncoding
    {
        UTF8,
        URL
    }

    /// <summary>
    /// Executes an application in response to a command heard by Echo.
    /// </summary>
    public class Execute : AlexaDoPluginBase
    {
        #region [ Members ]

        // Fields
        private string m_program;
        private string m_arguments;
        private QueryEncoding m_queryEncoding;
        private ProcessWindowStyle m_windowStyle;
        private int m_timeout;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initialize plug-in adapter.
        /// </summary>
        /// <remarks>
        /// Derived classes should set <see cref="AlexaDoPluginBase.Enabled"/> state to <c>true</c> upon successful initialization.
        /// </remarks>
        public override void Initialize()
        {
            // Parse expected parameters
            Dictionary<string, string> parameters = Parameters.ParseKeyValuePairs();
            string value;

            if (parameters.TryGetValue("program", out value) && !string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim();

                int quotesIndex = value.IndexOf('"');
                int argsIndex = value.IndexOf(' ');

                if (quotesIndex < argsIndex)
                {
                    quotesIndex = value.IndexOf('"', quotesIndex + 1);

                    if (quotesIndex > 0)
                        argsIndex = value.IndexOf(' ', quotesIndex + 1);
                }

                if (argsIndex > 0)
                {
                    m_program = value.Substring(0, argsIndex);
                    m_arguments = value.Substring(argsIndex + 1);
                }
                else
                {
                    m_program = value;
                    m_arguments = null;
                }
            }
            else
            {
                throw new InvalidOperationException("Key \"program\" not found in action parameters.");
            }

            if (!parameters.TryGetValue("queryEncoding", out value) || !Enum.TryParse(value, true, out m_queryEncoding))
                m_queryEncoding = QueryEncoding.UTF8;

            if (!parameters.TryGetValue("windowStyle", out value) || !Enum.TryParse(value, true, out m_windowStyle))
                m_windowStyle = ProcessWindowStyle.Hidden;

            if (!parameters.TryGetValue("timeout", out value) || !int.TryParse(value, out m_timeout))
                m_timeout = 30;

            // Convert timeout from seconds to milliseconds
            if (m_timeout > 0)
                m_timeout *= (int)SI.Kilo;

            // Call base initialization method to ensure plug-in Enabled state is set to true
            base.Initialize();
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
        public override void ProcessActivity(EchoActivity activity, string query)
        {
            string arguments = null;

            if (!string.IsNullOrWhiteSpace(m_arguments))
                arguments = m_arguments.ReplaceCaseInsensitive("[query]", EncodeQuery(query));

            ProcessStartInfo info = new ProcessStartInfo(m_program, arguments)
            {
                WindowStyle = m_windowStyle,
                ErrorDialog = false,
                UseShellExecute = false
            };

            // Attempt to start process
            Process process = Process.Start(info);

            // Wait for process completion on another thread - don't want to clog up activity processing
            ThreadPool.QueueUserWorkItem(WaitForProcessCompletion, new Tuple<Process, string>(process, arguments));
        }

        private void WaitForProcessCompletion(object state)
        {
            try
            {
                Tuple<Process, string> parameters = state as Tuple<Process, string>;

                if ((object)parameters != null)
                {
                    Process process = parameters.Item1;
                    string arguments = parameters.Item2;
                    bool timedOut;

                    try
                    {
                        // Wait for process to exit, or terminate after specified timeout
                        timedOut = process.WaitForExit(m_timeout);

                        // Close main window (if launched application has one)
                        if (!process.HasExited)
                            process.CloseMainWindow();

                        // Don't leave processes hanging around
                        if (!process.HasExited)
                            process.Kill();
                    }
                    finally
                    {
                        process.Close();
                    }

                    // Log execution state
                    Log.InfoFormat("Executed \"{0}\" with parameters \"{1}\" - {2}, exit code: {3}",
                        m_program,
                        arguments.ToNonNullString(),
                        timedOut ?
                            string.Format("application terminated after {0} second timeout", m_timeout * SI.Milli) :
                            "application self-terminated",
                        process.ExitCode);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error while waiting for process completion: {0}", ex.Message);
            }
        }

        private string EncodeQuery(string query)
        {
            // For URL encoding, just need to format spaces as valid URL characters
            if (m_queryEncoding == QueryEncoding.URL)
                return Uri.EscapeUriString(query);

            // Coming in from XML, query string will already be encoded as UTF8
            return query;
        }

        #endregion
    }
}
