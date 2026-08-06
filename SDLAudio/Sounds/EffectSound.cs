namespace SDLAudio.Sounds;

using System.Collections.Generic;
using System;
using Effects;

/// <summary>
/// A decorator which applies effects to a sound.
/// </summary>
public class EffectSound : ISound {
    private readonly List<IAudioEffect> effects = new();

    private readonly ISound inner;

    public EffectSound AddEffect(IAudioEffect effect) {
        effects.Add(effect);
        return this;
    }

    public void AddEffects(IEnumerable<IAudioEffect> effects) {
        this.effects.AddRange(effects);
    }

    public void RemoveEffect(IAudioEffect effect) {
        effects.Remove(effect);
    }

    public void ClearEffects() {
        effects.Clear();
    }

    public ReadOnlySpan<float> GetSamples(ulong playhead, uint samples, uint sampleRate, byte channel) {
        float[] raw_samples = inner.GetSamples(playhead, samples, sampleRate, channel).ToArray();

        Span<float> span = raw_samples;
        foreach (IAudioEffect effect in effects) {
            effect.Apply(span, playhead, channel, sampleRate);
        }

        return span;
    }

    public bool IsDone(ulong playhead, uint sampleRate) =>
        inner.IsDone(playhead, sampleRate);

    public EffectSound(ISound sound) {
        inner = sound;
    }
}
