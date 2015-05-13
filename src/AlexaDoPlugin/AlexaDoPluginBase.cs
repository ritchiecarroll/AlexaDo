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

using log4net;

namespace AlexaDoPlugin
{
    public class AlexaDoPluginBase
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields

        /// <summary>
        /// log4net log writer.
        /// </summary>
        protected readonly ILog Log;

        #endregion

        #region [ Constructors ]

        public AlexaDoPluginBase()
        {
            // Get a logger specific to derived plug-in
            Log = LogManager.GetLogger(GetType());
            Log.InfoFormat("Created a new {0} plug-in instance", GetType().FullName);
        }

        #endregion

        #region [ Properties ]

        #endregion

        #region [ Methods ]

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
    }
}
