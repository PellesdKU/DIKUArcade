namespace SDLAudio.Format;

using System;
using Internal;

public class FloatNativeFormat : IAudioFormat<float, double> {
    static SDLAudioFormat IAudioFormat<float, double>.SDLFormat => BitConverter.IsLittleEndian
        ? SDLAudioFormat.F32LE
        : SDLAudioFormat.F32BE;

    public static float Clamp(double b)
        => (float)double.Clamp(b, float.MinValue, float.MaxValue);

    public static double VolumeAdjust(float t, float volume)
        => t*(double)volume;
}
