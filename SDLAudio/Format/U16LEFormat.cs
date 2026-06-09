namespace SDLAudio.Format;

using Internal;

public class U16LEFormat : IAudioFormat<ushort, uint> {
    static SDLAudioFormat IAudioFormat<ushort, uint>.SDLFormat => SDLAudioFormat.U16LE;

    public static ushort Clamp(uint b)
        => (ushort)uint.Clamp(b, ushort.MinValue, ushort.MaxValue);

    public static uint VolumeAdjust(ushort t, float volume)
        => (uint)(t*volume);
}
