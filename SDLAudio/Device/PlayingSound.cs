namespace SDLAudio.Device;

using System;
using Format;
using Sound;

/// <summary>
/// A helper class representing a sound that is being played.
/// </summary>
internal class PlayingSound<Sample, Accu, Format> : IPlayingSound
    where Sample : struct
    where Accu : struct
    where Format : IAudioFormat<Sample, Accu>
{
    private readonly IAudioDevice device;
    private readonly Sound<Sample, Accu, Format> sound;
    private ulong playhead;
    private bool canceled;

    public bool Paused { get; private set; }
    public float Volume { get; private set; }
    public bool Done => canceled || sound.IsDone(playhead);

    public ReadOnlySpan<Sample> PlaySamples(uint samples) {
        ReadOnlySpan<Sample> span = sound.GetSamples(playhead, samples);
        playhead += samples;
        return span;
    }

    public void SetVolume(float volume) {
        device.Lock();
        Volume = Math.Clamp(volume, 0f, 1f);
        device.Unlock();
    }

    public void Pause(bool paused) {
        device.Lock();
        Paused = paused;
        device.Unlock();
    }

    public void Cancel() {
        canceled = true;
    }

    public PlayingSound(
        IAudioDevice device,
        Sound<Sample, Accu, Format> sound,
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
