namespace SDLAudio.Sound;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Format;

/// <summary>
/// A decorator which converts samples from a sound of one format to another
/// </summary>
/// <typeparam name="SampleFrom">Type of sample to convert from</typeparam>
/// <typeparam name="AccuFrom">Accumulator type to convert from</typeparam>
/// <typeparam name="FormatFrom">Format to convert from</typeparam>
/// <typeparam name="SampleTo">Sample type to convert to</typeparam>
/// <typeparam name="AccuTo">Accumulator type to convert to</typeparam>
/// <typeparam name="FormatTo">Format to convert to</typeparam>
internal class Converting<SampleFrom, AccuFrom, FormatFrom, SampleTo, AccuTo, FormatTo>
    : Sound<SampleTo, AccuTo, FormatTo>
    where SampleFrom : struct
    where SampleTo : struct
    where AccuFrom : struct, IAdditionOperators<AccuFrom, AccuFrom, AccuFrom>
    where AccuTo : struct, IAdditionOperators<AccuTo, AccuTo, AccuTo>
    where FormatFrom : IAudioFormat<SampleFrom, AccuFrom>
    where FormatTo : IAudioFormat<SampleTo, AccuTo>
{
    private readonly SDLAudio sdlAudio;
    private readonly Sound<SampleFrom, AccuFrom, FormatFrom> src;

    public Converting(
        SDLAudio sdlAudio,
        Sound<SampleFrom, AccuFrom, FormatFrom> src,
        uint dstRate,
        byte dstChannels
    ) : base(dstRate, dstChannels) {
        this.src = src;
        this.sdlAudio = sdlAudio;
    }

    public override ReadOnlySpan<SampleTo> GetSamples(ulong playhead, uint samples) {
        float channelCorrectionFactor = (float)src.Channels / Channels;
        float freqCorrectionFactor = (float)src.SampleRate / SampleRate;
        float correctionFactor = channelCorrectionFactor*freqCorrectionFactor;

        ulong correctedPlayhead = (ulong)(playhead*correctionFactor);
        uint correctedSamples = (uint)(samples*correctionFactor);

        ReadOnlySpan<SampleFrom> srcSamples = src.GetSamples(
            correctedPlayhead,
            correctedSamples
        );

        byte[] converted = sdlAudio.Convert(
            MemoryMarshal.Cast<SampleFrom, byte>(srcSamples),
            FormatFrom.SDLFormat,
            src.Channels,
            src.SampleRate,
            FormatTo.SDLFormat,
            Channels,
            SampleRate
        ).MapOrElse(
            converted => converted,
            _ => Array.Empty<byte>()
        );

        return MemoryMarshal.Cast<byte, SampleTo>(converted);
    }

    public override bool IsDone(ulong playhead) => src.IsDone(playhead);
}
