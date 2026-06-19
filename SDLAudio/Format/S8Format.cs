namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Signed 8-bit integer audio format
/// </summary>
public struct S8Format : IAudioFormat<sbyte, short> {
    static SDLAudioFormat IAudioFormat<sbyte, short>.SDLFormat => SDLAudioFormat.S8;

    public static sbyte Clamp(short b) =>
        (sbyte)short.Clamp(b, sbyte.MinValue, sbyte.MaxValue);

    public static short VolumeAdjust(sbyte t, float volume) =>
        (short)(t*volume);
}
