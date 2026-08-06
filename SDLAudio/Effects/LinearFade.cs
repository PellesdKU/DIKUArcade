namespace SDLAudio.Effects;

using System;

/// <summary>
/// An effect which linearly fades audio.
/// </summary>
public class LinearFadeEffect : IAudioEffect {
    private readonly float startVolume, endVolume, deltaVolume;
    private readonly float startTime, deltaTime;

    public void Apply(Span<float> samples, ulong playhead, byte channel, uint sampleRate) {
        for (uint i = 0; i < samples.Length; i++) {
            float time = (float)(playhead + i) / sampleRate;
            float volume = float.Clamp(
                startVolume + deltaVolume * (time - startTime) / deltaTime,
                startVolume,
                endVolume
            );
            Console.WriteLine(volume);
            samples[(int)i] *= volume;
        }
    }

    public LinearFadeEffect(float startVolume, float endVolume, float startTime, float endTime) {
        this.startTime = startTime;
        this.startVolume = startVolume;
        this.endVolume = endVolume;
        deltaTime = endTime - startTime;
        deltaVolume = endVolume - startTime;
    }
}
