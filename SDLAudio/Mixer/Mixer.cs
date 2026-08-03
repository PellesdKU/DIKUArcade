namespace SDLAudio.Mixer;

using System;

/// <summary>
/// A stateful sound mixer.
/// </summary>
internal struct Mixer {
    private double[] buf;

    public Mixer() {
        buf = Array.Empty<double>();
    }

    /// <summary>
    /// Add a new channel to the mix. Note that ClearSamples() must have been called with a length
    /// greater than or equal to the Length of <paramref name="audio" />.
    /// </summary>
    /// <param name="audio">Audio data to mix in.</param>
    /// <param name="volume">Volume to adjust audio with.</param>
    public readonly void AddChannel(ReadOnlySpan<float> audio, float volume) {
        uint i = 0;
        foreach (float sample in audio) {
            buf[i] += sample * volume;
            i++;
        }
    }

    /// <summary>
    /// Write the mixed audio to a span, clamping the mixed audio to the range of the format.
    /// Note that ClearSamples() must have been called with a length greater than or equal to
    /// the Length of <paramref name="dst" />.
    /// </summary>
    /// <param name="dst">Destination span to write to.</param>
    public readonly void Mix(Span<float> dst) {
        float[] mix = new float[buf.Length];

        for (int i = 0; i < mix.Length; i++) {
            mix[i] = (float)double.Clamp(buf[i], float.MinValue, float.MaxValue);
        }

        mix.AsSpan().CopyTo(dst);
    }

    /// <summary>
    /// Clear the mix buffer, and expand it to <paramref name="length" />
    /// </summary>
    /// <param name="length">Length to expand the buffer to.</param>
    public void ClearSamples(uint length) {
        if (buf.Length < length) {
            buf = new double[length];
        }

        buf.AsSpan().Clear();
    }
}
