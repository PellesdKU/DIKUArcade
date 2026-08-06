namespace SDLAudio.Effects;

using System;

/// <summary>
/// An effect which can be applied to a span of samples.
/// </summary>
public interface IAudioEffect {
    /// <summary>
    /// Apply effect to a span of samples. Channels will be laid out according to SDL:
    /// https://wiki.libsdl.org/SDL2/SDL_AudioSpec
    /// </summary>
    /// <param name="samples">Sample to apply effect to</param>
    /// <param name="playhead">Number of the first sample in the span</param>
    /// <param name="channel">Number of audio channels in the span</param>
    /// <param name="sampleRate">Sample rate of span</param>
    void Apply(Span<float> samples, ulong playhead, byte channel, uint sampleRate);
}
