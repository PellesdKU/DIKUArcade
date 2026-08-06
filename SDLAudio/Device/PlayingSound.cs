namespace SDLAudio.Device;

using System;
using Sounds;

public delegate void SoundEventHandler(AudioDevice device);


/// <summary>
/// A stateful helper class representing a sound that is being played, tracking progress and volume.
/// </summary>
public class PlayingSound {
    private readonly AudioDevice device;
    private readonly ISound sound;
    private ulong playhead;
    private bool canceled;

    public event SoundEventHandler? OnDone;

    public bool Paused { get; private set; } = false;
    public float Volume { get; private set; }
    public bool Done(uint sampleRate) => canceled || sound.IsDone(playhead, sampleRate);

    public ReadOnlySpan<float> PlaySamples(uint nSamples, uint sampleRate, byte channel) {
        ReadOnlySpan<float> span = sound.GetSamples(playhead, nSamples, sampleRate, channel);
        return span;
    }

    public void Advance(uint samples) {
        playhead += samples;
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

    public void HandleDone(AudioDevice device) {
        if (OnDone is not null) {
            OnDone(device);
        }
    }

    public PlayingSound(
        AudioDevice device,
        ISound sound,
        float volume
    ) {
        this.device = device;
        this.sound = sound;
        Volume = Math.Clamp(volume, 0f, 1f);
        canceled = false;
        playhead = 0;
    }
}
