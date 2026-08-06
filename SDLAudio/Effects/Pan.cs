namespace SDLAudio.Effects;

using System;

/// <summary>
/// An effect which adjusts the volume of a single channel, which can be used for panning.
/// </summary>
public class PanEffect : IAudioEffect {
    private readonly byte channel;
    public float Pan;

    public void Apply(Span<float> samples, ulong _, byte channel, uint sampleRate) {
        if (channel != this.channel) { return; }

        for (int i = 0; i < samples.Length; i++) {
            samples[i] *= Pan;
        }
    }

    public PanEffect(byte channel, float pan) {
        this.channel = channel;
        Pan = float.Clamp(pan, 0f, 1f);
    }
}
