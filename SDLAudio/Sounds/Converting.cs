namespace SDLAudio.Sounds;

using System;
using System.Runtime.InteropServices;
using Internal;

/// <summary>
/// A decorator which converts samples from a sound of one format to another
/// </summary>
internal class Converting : Sound {
    private readonly SDLAudio sdlAudio;
    private readonly Sound src;

    public Converting(
        SDLAudio sdlAudio,
        Sound src,
        uint dstRate,
        byte dstChannels
    ) : base(dstRate, dstChannels) {
        this.src = src;
        this.sdlAudio = sdlAudio;
    }

    public override ReadOnlySpan<float> GetSamples(ulong playhead, uint samples) {
        float channelCorrectionFactor = (float)src.Channels / Channels;
        float freqCorrectionFactor = (float)src.SampleRate / SampleRate;
        float correctionFactor = channelCorrectionFactor*freqCorrectionFactor;

        ulong correctedPlayhead = (ulong)(playhead*correctionFactor);
        uint correctedSamples = (uint)(samples*correctionFactor);

        ReadOnlySpan<float> srcSamples = src.GetSamples(
            correctedPlayhead,
            correctedSamples
        );

        byte[] converted = sdlAudio.Convert(
            MemoryMarshal.Cast<float, byte>(srcSamples),
            SDLAudioFormat.F32Native,
            src.Channels,
            src.SampleRate,
            SDLAudioFormat.F32Native,
            Channels,
            SampleRate
        ).MapOrElse(
            converted => converted,
            _ => Array.Empty<byte>()
        );

        return MemoryMarshal.Cast<byte, float>(converted);
    }

    public override bool IsDone(ulong playhead) => src.IsDone(playhead);
}
