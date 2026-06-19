namespace SDLAudio.Format;

using System;
using Internal;

/// <summary>
/// Native endianness unsigned 16-bit integer audio format
/// </summary>
public struct U16NativeFormat : IAudioFormat<ushort, uint> {
    static SDLAudioFormat IAudioFormat<ushort, uint>.SDLFormat => BitConverter.IsLittleEndian
        ? SDLAudioFormat.U16LE
        : SDLAudioFormat.U16BE;

    public static ushort Clamp(uint b) =>
        (ushort)uint.Clamp(b, ushort.MinValue, ushort.MaxValue);

    public static uint VolumeAdjust(ushort t, float volume) =>
        (uint)(t*volume);
}
