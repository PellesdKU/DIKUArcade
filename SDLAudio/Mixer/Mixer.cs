namespace SDLAudio.Mixer;

using System;

/// <summary>
/// A stateful sound mixer for native float format.
/// </summary>
internal readonly struct Mixer {
    private readonly double[,] buf;

    public Mixer(byte channels, ushort samples) {
        buf = new double[channels,samples];
    }

    /// <summary>
    /// Add a new track to the mix. Note that ClearSamples() must have been called with a length
    /// greater than or equal to the Length of <paramref name="audio" />.
    /// </summary>
    /// <param name="audio">Audio data to mix in.</param>
    /// <param name="volume">Volume to adjust audio with.</param>
    public readonly void AddTrack(ReadOnlySpan<float> audio, byte channel, float volume) {
        if (channel > buf.GetLength(0)) { return; }

        uint i = 0;
        foreach (float sample in audio) {
            buf[channel, i] += sample * volume;
            i++;
        }
    }

    /// <summary>
    /// Write the mixed audio to a span, clamping the mixed audio to the float range.
    /// Note that ClearSamples() must have been called with a length greater than or equal to
    /// the Length of <paramref name="dst" />.
    /// </summary>
    /// <param name="dst">Destination span to write to.</param>
    public readonly void Mix(Span<float> dst) {
        if (dst.Length < buf.Length) { return; }

        byte channels = (byte)buf.GetLength(0);
        int samples   = buf.GetLength(1);

        for (byte c = 0; c < channels; c++) {
            for (int i = 0; i < samples; i++) {
                dst[c+i*channels] = (float)double.Clamp(buf[c,i], float.MinValue, float.MaxValue);
            }
        }
    }

    public readonly void Clear() {
        Array.Clear(buf);
    }
}
