namespace SDLAudio.Format;

using Internal;

public struct S32BEFormat : IAudioFormat<int, long> {
    static SDLAudioFormat IAudioFormat<int, long>.SDLFormat => SDLAudioFormat.S32BE;

    public static int Clamp(long b) =>
        (int)long.Clamp(b, int.MinValue, int.MaxValue);

    public static long VolumeAdjust(int t, float volume) =>
        (long)(t*volume);
}
