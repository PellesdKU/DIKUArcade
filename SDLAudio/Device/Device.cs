namespace SDLAudio.Device;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal;
using Sound;
using Result;
using Mixer;
using Format;
using System.Numerics;

internal class Device<Sample, Accu, Format> : IAudioDevice
    where Sample : struct
    where Accu : struct, IAdditionOperators<Accu, Accu, Accu>
    where Format : IAudioFormat<Sample, Accu>
{
    private static readonly uint monoSampleSize = (uint)Unsafe.SizeOf<Sample>();
    private readonly SDLAudio sdlAudio;
    private readonly SDLAudioDeviceID id;

    // save the proxy so it doesn't get garbage collected
#pragma warning disable IDE0052
    private readonly DeviceCallbackProxy proxy;
#pragma warning restore IDE0052

    private Mixer<Sample, Accu, Format> mixer;
    private float masterVolume = 1;

    public uint SampleRate { get; private init; }
    public byte Channels { get; private init; }

    private readonly Stack<PlayingSound<Sample, Accu, Format>> playingSounds;

    public bool Stopped => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.STOPPED;
    public bool Paused => sdlAudio.GetAudioDeviceStatus(id) == SDLAudioStatus.PAUSED;

    private void Callback(Span<byte> stream) {
        if (stream.Length % monoSampleSize != 0) {
            throw new ArgumentException(
                $"Stream length must be a multiple of {monoSampleSize}",
                nameof(stream)
            );
        }

        Span<Sample> samples = MemoryMarshal.Cast<byte, Sample>(stream);
        mixer.ClearSamples((uint)samples.Length);

        Stack<PlayingSound<Sample, Accu, Format>> continueSounds = new();

        PlayingSound<Sample, Accu, Format>? sound;
        while (true) {
            lock(playingSounds) {
                if (!playingSounds.TryPop(out sound)) { break; }
            }

            ReadOnlySpan<Sample> span = sound.PlaySamples((uint)samples.Length);

            if (span.Length == 0) { continue; }

            mixer.AddChannel(span, sound.Volume*masterVolume);

            if (!sound.Done) {
                continueSounds.Push(sound);
            }
        }

        mixer.Mix(samples);

        lock(playingSounds) {
            foreach (PlayingSound<Sample, Accu, Format> continueSound in continueSounds) {
                playingSounds.Push(continueSound);
            }
        }
    }

    public Result<PlayingSound<Sample, Accu, Format>, string> PlaySound(
        Sound<Sample, Accu, Format> sound,
        float volume = 1f
    ) {
        if (Stopped) { return new("Device stopped."); }

        PlayingSound<Sample, Accu, Format> playingSound = new(this, sound, volume, false);

        lock(playingSounds) {
            playingSounds.Push(playingSound);
        }

        return new(playingSound);
    }

    public Result<IPlayingSound, string> PlaySound(ISound sound, float volume = 1f) =>
        Convert(sound)
        .AndThen(converted => PlaySound(converted, volume))
        .MapOk(p => p as IPlayingSound);

    public Result<ISound, string> Looping(ISound src) =>
        Convert(src)
        .MapOk(converted => new Looping<Sample, Accu, Format>(converted, SampleRate, Channels) as ISound);

    private Result<Sound<Sample, Accu, Format>, string> Convert(ISound src) =>
        src switch {
            Sound<Sample, Accu, Format> native => new(native),
            _ => src.Convert<Sample, Accu, Format>(sdlAudio, SampleRate, Channels)
        };

    Result<ISound, string> IAudioDevice.Convert(ISound sound) =>
        Convert(sound)
        .MapOk(sound => sound as ISound);

    public Device(
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
        GC.SuppressFinalize(this);
    }
}
