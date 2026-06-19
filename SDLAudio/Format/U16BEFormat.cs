namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Big endian unsigned 16-bit integer audio format
/// </summary>
public struct U16BEFormat : IAudioFormat<ushort, uint> {
    static SDLAudioFormat IAudioFormat<ushort, uint>.SDLFormat => SDLAudioFormat.U16BE;

    public static ushort Clamp(uint b) =>
        (ushort)uint.Clamp(b, ushort.MinValue, ushort.MaxValue);

    public static uint VolumeAdjust(ushort t, float volume) =>
        (uint)(t*volume);
}
