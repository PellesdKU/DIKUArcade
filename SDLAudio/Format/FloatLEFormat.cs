namespace SDLAudio.Format;

using Internal;

public class FloatLEFormat : IAudioFormat<float, double> {
    static SDLAudioFormat IAudioFormat<float, double>.SDLFormat => SDLAudioFormat.F32LE;

    public static float Clamp(double b)
        => (float)double.Clamp(b, float.MinValue, float.MaxValue);

    public static double VolumeAdjust(float t, float volume)
        => t*(double)volume;
}
