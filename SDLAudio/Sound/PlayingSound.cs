namespace SDLAudio.Sound;

using System;

/// <summary>
/// A helper class representing a sound that is being played.
/// </summary>
public class PlayingSound {
    private readonly AudioDevice device;

    private readonly ISound sound;
    private uint playhead;
    private bool paused;
    private bool canceled;

    public float Volume;
    public bool Done => canceled || sound.IsDone(playhead);

    public ReadOnlySpan<byte> PlaySamples(uint len) {
        ReadOnlySpan<byte> samples = sound.GetSamples(playhead, len);
        playhead += len;
        return samples;
    }

    public void SetVolume(float volume) {
        device.Lock();
        Volume = Math.Clamp(volume, 0f, 1f);
        device.Unlock();
    }

    public float GetVolume() => Volume;

    public void Pause(bool paused) {
        device.Lock();
        this.paused = paused;
        device.Unlock();
    }

    public bool Paused() => paused;

    public void Cancel() {
        canceled = true;
    }

    public PlayingSound(AudioDevice device, ISound sound, float volume, bool paused) {
        this.device = device;
        this.sound = sound;
        this.paused = paused;
        Volume = Math.Clamp(volume, 0f, 1f);
        canceled = false;
        playhead = 0;
    }
}
