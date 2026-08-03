namespace SDLAudio.Sounds;

using System;
using Result;

/// <summary>
/// A sound of a specific format.
/// </summary>
public abstract class Sound {
    public uint SampleRate { get; private init; }
    public byte Channels { get; private init; }

    public Sound(uint sampleRate, byte channels) {
        SampleRate = sampleRate;
        Channels = channels;
    }

    public virtual Result<Sound, string> Convert(
        SDLAudio sdlAudio, uint dstRate, byte dstChannels
    ) => new(new Converting(sdlAudio, this, dstRate, dstChannels));

    public abstract bool IsDone(ulong playhead);
    public abstract ReadOnlySpan<float> GetSamples(ulong playhead, uint samples);
}
