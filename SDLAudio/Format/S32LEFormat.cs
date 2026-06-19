namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Little endian signed 32-bit integer audio format
/// </summary>
public struct S32LEFormat : IAudioFormat<int, long> {
    static SDLAudioFormat IAudioFormat<int, long>.SDLFormat => SDLAudioFormat.S32LE;

    public static int Clamp(long b) =>
        (int)long.Clamp(b, int.MinValue, int.MaxValue);

    public static long VolumeAdjust(int t, float volume) =>
        (long)(t*volume);
}
