namespace SDLAudio.Sounds;

using System;

/// <summary>
/// A sound that is played repeatedly.
/// </summary>
internal class Looping : Sound {
    private readonly Sound inner;
    private ulong offset = 0;

    internal Looping(
        Sound inner,
        uint dstRate,
        byte dstChannels
    ) : base(dstRate, dstChannels) {
        this.inner = inner;
    }

    private ReadOnlySpan<float> GetInnerSamples(ulong playhead, uint samples)
        => inner.GetSamples(playhead - offset, samples);

    public override ReadOnlySpan<float> GetSamples(ulong playhead, uint samples) {
        uint patternHead = 0;
        float[] patternBuf = new float[samples];

        while (patternHead < samples) {
            uint remainingSamples = (uint)patternBuf.Length - patternHead;
            uint samplesToGet = uint.Min(remainingSamples, samples);
            ReadOnlySpan<float> pattern = GetInnerSamples(playhead + patternHead, samplesToGet);

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
