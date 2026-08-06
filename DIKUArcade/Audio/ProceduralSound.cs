namespace DIKUArcade.Audio;

using System;
using SDLAudio.Sounds;

public delegate float SoundGenerator(float time, uint channel);

public class ProceduralSound : ISound {
    private readonly SoundGenerator gen;
    private readonly Func<float, bool> isDone;

    public ProceduralSound(
        SoundGenerator gen,
        Func<float, bool> isDone
    ) {
        this.gen = gen;
        this.isDone = isDone;
    }

    public ReadOnlySpan<float> GetSamples(
        ulong playhead,
        uint samples,
        uint sampleRate,
        byte channel
    ) {
        float[] generated = new float[samples];

        for (uint s = 0; s < samples; s++) {
            float time = (float)playhead/sampleRate;
            generated[s] = gen(time, channel);
            playhead++;
        }

        return generated.AsSpan();
    }

    public bool IsDone(ulong playhead, uint sampleRate) => isDone((float)playhead/sampleRate);
}
