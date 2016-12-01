//******************************************************************************************************
//  ResponseType.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/15/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace AlexaDoPlugin.Commands
{
    /// <summary>
    /// Enumeration of possible response types in a <see cref="ResponseMessage"/>.
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// Text-to-speech response.
        /// </summary>
        Tts,
        TTS = Tts,
        tts = Tts,
        /// <summary>
        /// Wave file response.
        /// </summary>
        Wav,
        WAV = Wav,
        wav = Wav
    }
}
