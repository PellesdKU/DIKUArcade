namespace SDLAudio.Sound;

using System;
using System.Numerics;
using Format;
using Result;

/// <summary>
/// A sound of a specific format.
/// </summary>
/// <typeparam name="Sample">The sample type of the sound.</typeparam>
/// <typeparam name="Accu">The accumulator type of the sound for mixing.</typeparam>
/// <typeparam name="Format"></typeparam>
public abstract class Sound<Sample, Accu, Format> : ISound
    where Sample : struct
    where Accu : struct, IAdditionOperators<Accu, Accu, Accu>
    where Format : IAudioFormat<Sample, Accu>
{
    public uint SampleRate { get; private init; }
    public byte Channels { get; private init; }

    public Sound(uint sampleRate, byte channels) {
        SampleRate = sampleRate;
        Channels = channels;
    }

    protected virtual Result<Sound<SampleTo, AccuTo, FormatTo>, string> Convert<SampleTo, AccuTo, FormatTo>(
        SDLAudio sdlAudio, uint dstRate, byte dstChannels
    )
        where SampleTo : struct
        where AccuTo : struct, IAdditionOperators<AccuTo, AccuTo, AccuTo>
        where FormatTo : IAudioFormat<SampleTo, AccuTo>
    => new(new Converting<Sample, Accu, Format, SampleTo, AccuTo, FormatTo>(
        sdlAudio, this, dstRate, dstChannels
    ));

    Result<Sound<SampleTo, AccuTo, FormatTo>, string> ISound.Convert<SampleTo, AccuTo, FormatTo>(
        SDLAudio sdlAudio, uint dstRate, byte dstChannels
    ) => Convert<SampleTo, AccuTo, FormatTo>(sdlAudio, dstRate, dstChannels);

    public abstract bool IsDone(ulong playhead);
    public abstract ReadOnlySpan<Sample> GetSamples(ulong playhead, uint samples);
}
