namespace SDLAudio.Device;

using System;
using Sounds;

/// <summary>
/// A helper class representing a sound that is being played.
/// </summary>
public class PlayingSound {
    private readonly AudioDevice device;
    private readonly Sound sound;
    private ulong playhead;
    private bool canceled;

    public bool Paused { get; private set; }
    public float Volume { get; private set; }
    public bool Done => canceled || sound.IsDone(playhead);

    public ReadOnlySpan<float> PlaySamples(uint samples) {
        ReadOnlySpan<float> span = sound.GetSamples(playhead, samples);
        playhead += samples;
        return span;
    }

    public void SetVolume(float volume) {
        device.Lock();
        Volume = Math.Clamp(volume, 0f, 1f);
        device.Unlock();
    }

    public void Pause(bool paused = true) {
        device.Lock();
        Paused = paused;
        device.Unlock();
    }

    public void Cancel() {
        canceled = true;
    }

    public PlayingSound(
        AudioDevice device,
        Sound sound,
        float volume,
        bool paused
    ) {
        this.device = device;
        this.sound = sound;
        Paused = paused;
        Volume = Math.Clamp(volume, 0f, 1f);
        canceled = false;
        playhead = 0;
    }
}
