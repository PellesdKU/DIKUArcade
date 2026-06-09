namespace SDLAudio.Format;

using System;
using Internal;

public class S32NativeFormat : IAudioFormat<int, long> {
    static SDLAudioFormat IAudioFormat<int, long>.SDLFormat => BitConverter.IsLittleEndian
        ? SDLAudioFormat.S32LE
        : SDLAudioFormat.S32BE;

    public static int Clamp(long b)
        => (int)long.Clamp(b, int.MinValue, int.MaxValue);

    public static long VolumeAdjust(int t, float volume)
        => (long)(t*volume);
}
