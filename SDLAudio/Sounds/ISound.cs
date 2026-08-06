namespace SDLAudio.Sounds;

using System;

/// <summary>
/// A sound of a specific format.
/// </summary>
public interface ISound {
    bool IsDone(ulong playhead, uint sampleRate);
    ReadOnlySpan<float> GetSamples(ulong playhead, uint samples, uint sampleRate, byte channel);
}
