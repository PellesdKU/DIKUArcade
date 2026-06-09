namespace SDLAudio.Format;

using System;
using Internal;

public class S16NativeFormat : IAudioFormat<short, int> {
    static SDLAudioFormat IAudioFormat<short, int>.SDLFormat => BitConverter.IsLittleEndian
        ? SDLAudioFormat.S16LE
        : SDLAudioFormat.S16BE;

    public static short Clamp(int b)
        => (short)int.Clamp(b, short.MinValue, short.MaxValue);

    public static int VolumeAdjust(short t, float volume)
        => (int)(t*volume);
}
