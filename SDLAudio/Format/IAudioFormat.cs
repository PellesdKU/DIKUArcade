namespace SDLAudio.Format;

using System.Numerics;
using Internal;

/// <summary>
/// An audio format. Used as a generic type constraint to provide functions used for mixing.
/// </summary>
/// <typeparam name="Sample">Type of the audio samples</typeparam>
/// <typeparam name="Accu">Accumulator type for mixing</typeparam>
public interface IAudioFormat<Sample, Accu> // if only C# had associated types 😔
    where Sample : struct
    where Accu : struct, IAdditionOperators<Accu, Accu, Accu>
{
    /// <summary>
    /// The internal SDL format. Used for conversion.
    /// </summary>
    static abstract internal SDLAudioFormat SDLFormat { get; }

    /// <summary>
    /// Convert a sample to the accumulator type and adjust its volume.
    /// </summary>
    /// <param name="sample">Sample to convert and adjust.</param>
    /// <param name="volume">Volume to adjust the sample to.</param>
    /// <returns>Converted and volume adjusted sample.</returns>
    static abstract Accu VolumeAdjust(Sample sample, float volume);

    /// <summary>
    /// Clamp an accumulator sample to the range of the sample type and convert it back.
    /// </summary>
    /// <param name="accSample">Accumulator sample.</param>
    /// <returns>Clamped and re-converted sample.</returns>
    static abstract Sample Clamp(Accu accSample);
}
