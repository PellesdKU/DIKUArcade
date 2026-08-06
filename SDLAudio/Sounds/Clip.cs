namespace SDLAudio.Sounds;

using System;

/// <summary>
/// A clip of audio.
/// </summary>
public class Clip : ISound {
    private readonly float[][] data;

    public byte Channels { get; private init; }
    public uint SampleRate { get; private init; }
    public uint Samples { get; private init; }

    /// <summary>
    /// Length of the clip in samples. This is across all channels, so a stereo clip will have
    /// twice the Length of a mono clip of the same duration.
    /// </summary>
    public uint Length => (uint)data.Length;

    public bool IsDone(ulong playhead, uint _) => playhead > Samples;

    /// <summary>
    /// Get a number of samples after some index (playhead). May return fewer samples than requested
    /// if, for example, the end of the clip has been reached.
    /// </summary>
    /// <param name="playhead">The index from which to return samples.</param>
    /// <param name="samples">The maximum number of samples to return.</param>
    public ReadOnlySpan<float> GetSamples(
        ulong playhead,
        uint samples,
        uint sampleRate,
        byte channel
    ) {
        if ((int)playhead > Samples) {
            return new();
        }

        int index = int.Min((int)playhead, (int)Samples);
        int remaining = (int)Samples - index;
        int len = int.Min(remaining, (int)samples);

        return data[channel].AsSpan().Slice(index, len);
    }

    public Clip(
        float[] audioData,
        uint sampleRate,
        byte channels
    ) {
        SampleRate = sampleRate;
        Channels = channels;
        Samples = (uint)audioData.Length/channels;

        data = new float[Channels][];

        for (byte c = 0; c < Channels; c++) {
            data[c] = new float[Samples];
            for (int s = 0; s < Samples; s++) {
                data[c][s] = audioData[c+s*Channels];
            }
        }
    }
}
