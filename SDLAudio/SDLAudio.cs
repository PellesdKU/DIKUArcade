namespace SDLAudio;

using System;
using System.Collections.Generic;
using Internal;
using Result;

internal delegate void SDLAudioCallback(IntPtr /* void* */ userdata, IntPtr /* uint8* */ stream, int len);

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// This class acts as a guard around SDL Audio, preventing the audio functions from being called
/// before the audio subsystem is initalized.
/// </summary>
public class SDLAudio {
    private readonly Dictionary<string, int> playbackDevices;

    private SDLAudio() {
        playbackDevices = new();

        RefreshPlaybackDevices();
    }

    ~SDLAudio() {
        SDLInternal.QuitAudio();
    }

    public void RefreshPlaybackDevices() {
        lock (playbackDevices) {
            playbackDevices.Clear();

            int nDevices = GetNumAudioDevices(DeviceType.PLAYBACK);
            for (int i = 0; i < nDevices; i++) {
                string name = GetAudioDeviceName(i, DeviceType.PLAYBACK);
                playbackDevices[name] = i;
            }
        }
    }

    public IEnumerable<string> GetPlaybackDevices() => playbackDevices.Keys;

    public Result<string, string> GetDefaultDevice() {
        AudioSpec spec = default;
        if(!GetDefaultAudioInfo(out string name, ref spec, DeviceType.PLAYBACK)) {
            return Result<string, string>
                .Err($"Couldn't get default device: {SDLInternal.GetError()}");
        }

        return Result<string, string>.Ok(name);
    }

    public Result<AudioDevice, string> OpenPlaybackDevice(string name) {
        lock (playbackDevices) {
            if(!playbackDevices.TryGetValue(name, out int index)) {
                return new($"Unknown device: {name}");
            }

            if(!GetAudioDeviceSpec(index, DeviceType.PLAYBACK, out AudioSpec spec)) {
                return new($"Couldn't get device specification for {name}: {SDLInternal.GetError()}");
            }

            AudioCallbackProxy proxy = new();
            spec.Callback = proxy.Listener;

            SDLAudioDeviceID id = OpenAudioDevice(
                device:         name,
                deviceType:     DeviceType.PLAYBACK,
                desiredSpec:    ref spec,
                obtainedSpec:   out AudioSpec obtainedSpec,
                allowedChanges: SDLAllowChange.ANY
            );

            if (id.IsInvalid) {
                return new($"Couldn't open audio device: {SDLInternal.GetError()}");
            }

            return new(new AudioDevice(this, id, proxy, obtainedSpec));
        }
    }

    public Result<AudioDevice, string> OpenDefaultPlaybackDevice() {
        AudioSpec spec = new();
        AudioCallbackProxy proxy = new();
        spec.Callback = proxy.Listener;

        SDLAudioDeviceID id = OpenAudioDevice(
            device:         null,
            deviceType:     DeviceType.PLAYBACK,
            desiredSpec:    ref spec,
            obtainedSpec:   out AudioSpec obtainedSpec,
            allowedChanges: SDLAllowChange.ANY
        );

        if (id.IsInvalid) {
            return new($"Couldn't open audio device: {SDLInternal.GetError()}");
        }

        return new(new AudioDevice(this, id, proxy, obtainedSpec));
    }

    public static Result<SDLAudio, string> Create() {
        if (SDLInternal.InitAudio()) {
            return new(new SDLAudio());
        }

        return new($"Couldn't initialize SDL_Audio: {SDLInternal.GetError()}");
    }

    public string GetCurrentAudioDriver() => SDLInternal.GetCurrentAudioDriver();

    internal bool GetDefaultAudioInfo(
        out string name,
        ref AudioSpec spec,
        DeviceType deviceType
    )  => SDLInternal.GetDefaultAudioInfo(out name, ref spec, deviceType) == 0;

    internal int GetNumAudioDevices(DeviceType deviceType)
        => SDLInternal.GetNumAudioDevices(deviceType);

    internal string GetAudioDeviceName(int index, DeviceType deviceType)
        => SDLInternal.GetAudioDeviceName(index, deviceType);

    internal bool GetAudioDeviceSpec(int index, DeviceType deviceType, out AudioSpec spec)
        => SDLInternal.GetAudioDeviceSpec(index, deviceType, out spec) == 0;

    internal SDLAudioDeviceID OpenAudioDevice(
        string? device,
        DeviceType deviceType,
        ref AudioSpec desiredSpec,
        out AudioSpec obtainedSpec,
        SDLAllowChange allowedChanges
    ) => SDLInternal.OpenAudioDevice(
        device:         device,
        deviceType:     deviceType,
        desired:        ref desiredSpec,
        obtained:       out obtainedSpec,
        allowedChanges: allowedChanges
    );

    internal void CloseAudioDevice(AudioDevice device)
        => SDLInternal.CloseAudioDevice(device.id);

    internal void PauseAudioDevice(AudioDevice device, bool pause)
        => SDLInternal.PauseAudioDevice(device.id, pause);

    internal SDLAudioStatus GetAudioDeviceStatus(AudioDevice device)
        => SDLInternal.GetAudioDeviceStatus(device.id);

    internal void LockAudioDevice(AudioDevice dev) => SDLInternal.LockAudioDevice(dev.id);
    internal void UnlockAudioDevice(AudioDevice dev) => SDLInternal.UnlockAudioDevice(dev.id);

    internal IntPtr LoadWAV_RW(
        IntPtr src,
        bool freesrc,
        out AudioSpec spec,
        out IntPtr audioBuf,
        out uint audioLen
    ) => SDLInternal.LoadWAV_RW(src, freesrc, out spec, out audioBuf, out audioLen);

    internal void FreeWAV(IntPtr audioBuf)
        => SDLInternal.FreeWAV(audioBuf);

    internal int ConvertAudio(ref SDLAudioCVT cvt)
        => SDLInternal.ConvertAudio(ref cvt);

    internal void MixAudioFormat(
        IntPtr dst,
        IntPtr src,
        AudioFormat format,
        uint len,
        float volume
    ) => SDLInternal.MixAudioFormat(dst, src, format, len, (int)(volume*128));
}
#pragma warning restore CA1822 // Mark members as static
