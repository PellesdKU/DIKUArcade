namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Big endian signed 16-bit audio format
/// </summary>
public struct S16BEFormat : IAudioFormat<short, int> {
    static SDLAudioFormat IAudioFormat<short, int>.SDLFormat => SDLAudioFormat.S16BE;

    public static short Clamp(int b) =>
        (short)int.Clamp(b, short.MinValue, short.MaxValue);

    public static int VolumeAdjust(short t, float volume) =>
        (int)(t*volume);
}
