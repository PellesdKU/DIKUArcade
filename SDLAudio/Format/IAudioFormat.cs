namespace SDLAudio.Format;

using Internal;

public interface IAudioFormat<Sample, Accu> // if only C# had associated types 😔
    where Sample : struct
{
    static abstract internal SDLAudioFormat SDLFormat { get; }

    static abstract Accu VolumeAdjust(Sample t, float volume);

    static abstract Sample Clamp(Accu b);
}
