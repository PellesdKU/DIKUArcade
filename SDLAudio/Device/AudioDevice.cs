namespace SDLAudio.Device;

using System;
using System.Collections.Generic;
using Internal;
using Sounds;
using Result;
using Mixer;

public class AudioDevice {
    private readonly SDLAudio sdlAudio;
    private readonly SDLAudioDeviceID id;

    // save the proxy so it doesn't get garbage collected
#pragma warning disable IDE0052
    private readonly DeviceCallbackProxy proxy;
#pragma warning restore IDE0052

    private Mixer mixer;
    private float masterVolume = 1;

    public uint SampleRate { get; private init; }
    public byte Channels { get; private init; }
    public uint Samples { get; private init; }

    private readonly Stack<PlayingSound> playingSounds;

    public bool Stopped => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.STOPPED;
    public bool Paused => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.PAUSED;

    private void Callback(Span<float> stream) {
        Stack<PlayingSound> continueSounds = new();
        Stack<PlayingSound> doneSounds = new();

        PlayingSound? sound;
        while (true) {
            lock(playingSounds) {
                if (!playingSounds.TryPop(out sound)) { break; }
            }

            if (!sound.Paused) {
                for (byte c = 0; c < Channels; c++) {
                    ReadOnlySpan<float> span = sound.PlaySamples(Samples, SampleRate, c);
                    mixer.AddTrack(span, c, sound.Volume*masterVolume);
                }

                sound.Advance(Samples);
            }

            if (sound.Done(SampleRate)) {
                doneSounds.Push(sound);
            } else {
                continueSounds.Push(sound);
            }
        }

        mixer.Mix(stream);
        mixer.Clear();

        lock(playingSounds) {
            foreach (PlayingSound continueSound in continueSounds) {
                playingSounds.Push(continueSound);
            }
        }

        foreach (PlayingSound doneSound in doneSounds) {
            doneSound.HandleDone(this);
        }
    }

    public Result<PlayingSound, string> PlaySound(
        ISound sound,
        SoundEventHandler onDone,
        float volume = 1f
    ) {
        if (Stopped) { return new("Device stopped."); }

        PlayingSound playingSound = new(this, sound, volume);
        playingSound.OnDone += onDone;

        lock(playingSounds) {
            playingSounds.Push(playingSound);
        }

        return new(playingSound);
    }

    internal AudioDevice(
        SDLAudio sdlAudio,
        SDLAudioDeviceID deviceId,
        DeviceCallbackProxy proxy,
        uint sampleRate,
        byte channels,
        ushort samples
    ) {
        this.sdlAudio = sdlAudio;
        SampleRate = sampleRate;
        Channels = channels;
        Samples = samples;
        id = deviceId;
        playingSounds = new();
        mixer = new(Channels, samples);
        proxy.Callback = Callback;
        this.proxy = proxy;

        Pause(false);
    }

    public void Lock() => sdlAudio.LockAudioDevice(id);
    public void Unlock() => sdlAudio.UnlockAudioDevice(id);

    public void Pause(bool pause = true) => sdlAudio.PauseAudioDevice(id, pause);

    public void SetVolume(float vol) {
        Lock();
        masterVolume = Math.Clamp(vol, 0f, 1f);
        Unlock();
    }

    public void Dispose() {
        sdlAudio.CloseAudioDevice(id);
    }
}
