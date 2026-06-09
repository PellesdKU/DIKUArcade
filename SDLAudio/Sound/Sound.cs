namespace SDLAudio.Sound;

using System;
using Format;
using Result;

public abstract class Sound<Sample, Accu, Format> : ISound
    where Sample : struct
    where Accu : struct
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
        where AccuTo : struct
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
