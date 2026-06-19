namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Little endian signed 16-bit integer audio format
/// </summary>
public struct S16LEFormat : IAudioFormat<short, int> {
    static SDLAudioFormat IAudioFormat<short, int>.SDLFormat => SDLAudioFormat.S16LE;

    public static short Clamp(int b) =>
        (short)int.Clamp(b, short.MinValue, short.MaxValue);

    public static int VolumeAdjust(short t, float volume) =>
        (int)(t*volume);
}
