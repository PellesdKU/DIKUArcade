namespace DIKUArcade.Audio;

using System;
using SDLAudio.Format;
using SDLAudio.Sound;

public delegate float SoundGenerator(float time);

public class ProceduralSound : Sound<float, double, FloatNativeFormat> {
    private readonly SoundGenerator gen;
    private readonly Func<float, bool> isDone;

    public ProceduralSound(
        SoundGenerator gen,
        Func<float, bool> isDone,
        uint sampleRate,
        byte channels
    ) : base(sampleRate, channels) {
        this.gen = gen;
        this.isDone = isDone;
    }

    private float Time(ulong playhead) => (float)playhead/SampleRate;

    public override ReadOnlySpan<float> GetSamples(ulong playhead, uint samples) {
        float[] generated = new float[samples];

        for (uint i = 0; i < generated.Length; i++) {
            float time = Time(playhead);
            generated[i] = gen(time);
            playhead++;
        }

        return generated.AsSpan();
    }

    public override bool IsDone(ulong playhead) => isDone(Time(playhead));
}
