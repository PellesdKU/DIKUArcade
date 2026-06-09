namespace SDLAudio.Format;

using Internal;

public class S32LEFormat : IAudioFormat<int, long> {
    static SDLAudioFormat IAudioFormat<int, long>.SDLFormat => SDLAudioFormat.S32LE;

    public static int Clamp(long b)
        => (int)long.Clamp(b, int.MinValue, int.MaxValue);

    public static long VolumeAdjust(int t, float volume)
        => (long)(t*volume);
}
