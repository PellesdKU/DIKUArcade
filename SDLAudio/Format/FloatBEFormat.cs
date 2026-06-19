namespace SDLAudio.Format;

using Internal;

/// <summary>
/// Big endian float audio format
/// </summary>
public struct FloatBEFormat : IAudioFormat<float, double> {
    static SDLAudioFormat IAudioFormat<float, double>.SDLFormat => SDLAudioFormat.F32BE;

    public static float Clamp(double b) =>
        (float)double.Clamp(b, float.MinValue, float.MaxValue);

    public static double VolumeAdjust(float t, float volume) =>
        t*(double)volume;
}
