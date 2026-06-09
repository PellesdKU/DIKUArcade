namespace SDLAudio.Sound;

using System;
using System.Runtime.InteropServices;
using Format;

internal class Converting<SampleFrom, AccuFrom, FormatFrom, SampleTo, AccuTo, FormatTo>
    : Sound<SampleTo, AccuTo, FormatTo>
    where SampleFrom : struct
    where SampleTo : struct
    where AccuFrom : struct
    where AccuTo : struct
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
