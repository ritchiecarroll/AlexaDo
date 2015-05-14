//******************************************************************************************************
//  ScriptInterface.cs - Gbtc
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
using System.Runtime.InteropServices;
using log4net;

namespace AlexaDo
{
    /// <summary>
    /// Defines a simple COM based interface to accept call backs from JavaScript on WebBrowser
    /// </summary>
    [ComVisible(true)]
    public class ScriptInterface
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event raised when new Echo Activity has been received.
        /// </summary>
        public event EventHandler ReceivedEchoActivity;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// JavaScript call back function for received Echo activities.
        /// </summary>
        public void EchoActivityCallback()
        {
            if ((object)ReceivedEchoActivity != null)
                ReceivedEchoActivity(this, EventArgs.Empty);

            Log.DebugFormat("Activity call back received at {0:yyyy-MM-dd HH:mm:ss.000}", DateTime.UtcNow);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptInterface));

        #endregion
    }
}
