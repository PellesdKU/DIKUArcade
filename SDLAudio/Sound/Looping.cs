namespace SDLAudio.Sound;

using System;
using Format;

internal class Looping<Sample, Accu, Format>
    : Sound<Sample, Accu, Format>
    where Sample : struct
    where Accu : struct
    where Format : IAudioFormat<Sample, Accu>
{
    private readonly Sound<Sample, Accu, Format> inner;
    private ulong offset = 0;

    internal Looping(
        Sound<Sample, Accu, Format> inner,
        uint dstRate,
        byte dstChannels
    ) : base(dstRate, dstChannels) {
        this.inner = inner;
    }

    private ReadOnlySpan<Sample> GetInnerSamples(ulong playhead, uint samples)
        => inner.GetSamples(playhead - offset, samples);

    public override ReadOnlySpan<Sample> GetSamples(ulong playhead, uint samples) {
        uint patternHead = 0;
        Sample[] patternBuf = new Sample[samples];

        while (patternHead < samples) {
            uint remainingSamples = (uint)patternBuf.Length - patternHead;
            uint samplesToGet = uint.Min(remainingSamples, samples);
            ReadOnlySpan<Sample> pattern = GetInnerSamples(playhead + patternHead, samplesToGet);

            if (pattern.Length < samplesToGet) {
                offset = playhead + (ulong)pattern.Length;
            }

            pattern.CopyTo(patternBuf.AsSpan((int)patternHead, pattern.Length));
            patternHead += (uint)pattern.Length;
        }

        return patternBuf.AsSpan();
    }

    public override bool IsDone(ulong _) => false;
}
