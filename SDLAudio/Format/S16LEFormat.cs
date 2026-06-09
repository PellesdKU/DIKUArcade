namespace SDLAudio.Format;

using Internal;

public class S16LEFormat : IAudioFormat<short, int> {
    static SDLAudioFormat IAudioFormat<short, int>.SDLFormat => SDLAudioFormat.S16LE;

    public static short Clamp(int b)
        => (short)int.Clamp(b, short.MinValue, short.MaxValue);

    public static int VolumeAdjust(short t, float volume)
        => (int)(t*volume);
}
