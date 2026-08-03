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

    private readonly Stack<PlayingSound> playingSounds;

    public bool Stopped => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.STOPPED;
    public bool Paused => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.PAUSED;

    private void Callback(Span<float> stream) {
        mixer.ClearSamples((uint)stream.Length);

        Stack<PlayingSound> continueSounds = new();

        PlayingSound? sound;
        while (true) {
            lock(playingSounds) {
                if (!playingSounds.TryPop(out sound)) { break; }
            }

            if (!sound.Paused) {
                ReadOnlySpan<float> span = sound.PlaySamples((uint)stream.Length);

                if (span.Length == 0) { continue; }

                mixer.AddTrack(span, sound.Volume*masterVolume);
            }

            if (!sound.Done) {
                continueSounds.Push(sound);
            }
        }

        mixer.Mix(stream);

        lock(playingSounds) {
            foreach (PlayingSound continueSound in continueSounds) {
                playingSounds.Push(continueSound);
            }
        }
    }

    private Result<PlayingSound, string> PlaySoundNative(
        Sound sound,
        float volume = 1f
    ) {
        if (Stopped) { return new("Device stopped."); }

        PlayingSound playingSound = new(this, sound, volume, false);

        lock(playingSounds) {
            playingSounds.Push(playingSound);
        }

        return new(playingSound);
    }

    public Result<PlayingSound, string> PlaySound(Sound sound, float volume = 1f) =>
        Convert(sound)
        .AndThen(converted => PlaySoundNative(converted, volume));

    public Result<Sound, string> Looping(Sound src) =>
        Convert(src)
        .MapOk(converted => new Looping(converted, SampleRate, Channels) as Sound);

    public Result<Sound, string> Convert(Sound src) =>
        src.Convert(sdlAudio, SampleRate, Channels);

    internal AudioDevice(
        SDLAudio sdlAudio,
        SDLAudioDeviceID deviceId,
        DeviceCallbackProxy proxy,
        uint sampleRate,
        byte channels
    ) {
        this.sdlAudio = sdlAudio;
        SampleRate = sampleRate;
        Channels = channels;
        id = deviceId;
        playingSounds = new();
        mixer = new();
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
