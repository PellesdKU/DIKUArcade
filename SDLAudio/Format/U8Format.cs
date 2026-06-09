namespace SDLAudio.Format;

using Internal;

public struct U8Format : IAudioFormat<byte, ushort> {
    static SDLAudioFormat IAudioFormat<byte, ushort>.SDLFormat => SDLAudioFormat.U8;

    public static byte Clamp(ushort b) =>
        (byte)ushort.Clamp(b, byte.MinValue, byte.MaxValue);

    public static ushort VolumeAdjust(byte t, float volume) =>
        (ushort)(t*volume);
}
