namespace SDLAudio.Sound;

using System;
using System.Runtime.InteropServices;
using Format;

public class Converting<SampleTo, AccuTo, FormatTo>
    : Sound<SampleTo, AccuTo, FormatTo>
    where SampleTo : struct
    where AccuTo : struct
    where FormatTo : IAudioFormat<SampleTo, AccuTo>
{
    private readonly SDLAudio sdlAudio;
    private readonly ISound src;

    public Converting(
        SDLAudio sdlAudio,
        ISound src,
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

        ReadOnlySpan<byte> srcSamples = src.GetSamples(
            correctedPlayhead,
            correctedSamples
        );

        byte[] converted = sdlAudio.Convert(
            srcSamples,
            src.Format,
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
