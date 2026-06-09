namespace SDLAudio;

using System;
using System.Collections.Generic;
using Device;
using Sound.Clip;
using Internal;
using Result;
using System.IO;

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
        SDLAudioSpec spec = default;
        if(!GetDefaultAudioInfo(out string name, ref spec, DeviceType.PLAYBACK)) {
            return Result<string, string>
                .Err($"Couldn't get default device: {SDLInternal.GetError()}");
        }

        return Result<string, string>.Ok(name);
    }

    public Result<IAudioDevice, string> OpenPlaybackDevice(string name) {
        lock (playbackDevices) {
            if(!playbackDevices.TryGetValue(name, out int index)) {
                return new($"Unknown device: {name}");
            }

            if(!GetAudioDeviceSpec(index, DeviceType.PLAYBACK, out SDLAudioSpec spec)) {
                return new($"Couldn't get device specification for {name}: {SDLInternal.GetError()}");
            }

            DeviceCallbackProxy proxy = new();
            spec.Callback = proxy.Listener;

            SDLAudioDeviceID id = OpenAudioDevice(
                device:         name,
                deviceType:     DeviceType.PLAYBACK,
                desiredSpec:    ref spec,
                obtainedSpec:   out SDLAudioSpec obtainedSpec,
                allowedChanges: SDLAllowChange.ANY
            );

            if (id.IsInvalid) {
                return new($"Couldn't open audio device: {SDLInternal.GetError()}");
            }

            return obtainedSpec.MakeDevice(this, id, proxy);
        }
    }

    public Result<IAudioDevice, string> OpenDefaultPlaybackDevice() {
        SDLAudioSpec spec = new();
        DeviceCallbackProxy proxy = new();
        spec.Callback = proxy.Listener;

        SDLAudioDeviceID id = OpenAudioDevice(
            device:         null,
            deviceType:     DeviceType.PLAYBACK,
            desiredSpec:    ref spec,
            obtainedSpec:   out SDLAudioSpec obtainedSpec,
            allowedChanges: SDLAllowChange.ANY
        );

        if (id.IsInvalid) {
            return new($"Couldn't open audio device: {SDLInternal.GetError()}");
        }

        return obtainedSpec.MakeDevice(this, id, proxy);
    }

    /// <summary>
    /// Load a sound from a stream of a .wav file.
    /// </summary>
    /// <param name="stream">Stream of a .wav file.</param>
    /// <returns>A Result of a Sound or a loading error.</returns>
    public Result<IClip, string> LoadWAV(SDLAudio sdlAudio, Stream stream) {
        // I can't be bothered to implement an SDL_RWops marshaller, so we just load the whole
        // file into memory and create an RWops* with SDL_RWFromMem().
        byte[] fileData = new byte[stream.Length];
        stream.Read(fileData.AsSpan());

        unsafe {
            fixed (void* ptr = fileData) {
                IntPtr memoryRWops = SDLInternal.RWFromMem((IntPtr)ptr, fileData.Length);

                if(sdlAudio.LoadWAV_RW(
                    memoryRWops, true,
                    out SDLAudioSpec spec,
                    out IntPtr wavData,
                    out uint len) == 0) {
                    return new(SDLInternal.GetError());
                }

                byte[] soundData = new byte[len];
                unsafe {
                    new ReadOnlySpan<byte>((void*)wavData, (int)len).CopyTo(soundData);
                }

                sdlAudio.FreeWAV(wavData);

                return spec.MakeClip(this, soundData);
            }
        }
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
        ref SDLAudioSpec spec,
        DeviceType deviceType
    )  => SDLInternal.GetDefaultAudioInfo(out name, ref spec, deviceType) == 0;

    internal int GetNumAudioDevices(DeviceType deviceType)
        => SDLInternal.GetNumAudioDevices(deviceType);

    internal string GetAudioDeviceName(int index, DeviceType deviceType)
        => SDLInternal.GetAudioDeviceName(index, deviceType);

    internal bool GetAudioDeviceSpec(int index, DeviceType deviceType, out SDLAudioSpec spec)
        => SDLInternal.GetAudioDeviceSpec(index, deviceType, out spec) == 0;

    internal SDLAudioDeviceID OpenAudioDevice(
        string? device,
        DeviceType deviceType,
        ref SDLAudioSpec desiredSpec,
        out SDLAudioSpec obtainedSpec,
        SDLAllowChange allowedChanges
    ) => SDLInternal.OpenAudioDevice(
        device:         device,
        deviceType:     deviceType,
        desired:        ref desiredSpec,
        obtained:       out obtainedSpec,
        allowedChanges: allowedChanges
    );

    internal void CloseAudioDevice(SDLAudioDeviceID id)
        => SDLInternal.CloseAudioDevice(id);

    internal void PauseAudioDevice(SDLAudioDeviceID id, bool pause)
        => SDLInternal.PauseAudioDevice(id, pause);

    internal SDLAudioStatus GetAudioDeviceStatus(SDLAudioDeviceID id)
        => SDLInternal.GetAudioDeviceStatus(id);

    internal void LockAudioDevice(SDLAudioDeviceID id) => SDLInternal.LockAudioDevice(id);
    internal void UnlockAudioDevice(SDLAudioDeviceID id) => SDLInternal.UnlockAudioDevice(id);

    internal IntPtr LoadWAV_RW(
        IntPtr src,
        bool freesrc,
        out SDLAudioSpec spec,
        out IntPtr audioBuf,
        out uint audioLen
    ) => SDLInternal.LoadWAV_RW(src, freesrc, out spec, out audioBuf, out audioLen);

    internal void FreeWAV(IntPtr audioBuf)
        => SDLInternal.FreeWAV(audioBuf);

    internal int BuildAudioCVT(
        out SDLAudioCVT cvt,
        SDLAudioFormat srcFormat,
        byte srcChannels,
        int srcRate,
        SDLAudioFormat dstFormat,
        byte dstChannels,
        int dstRate
    ) => SDLInternal.BuildAudioCVT(
        out cvt,
        srcFormat,
        srcChannels,
        srcRate,
        dstFormat,
        dstChannels,
        dstRate
    );

    internal int ConvertAudio(ref SDLAudioCVT cvt)
        => SDLInternal.ConvertAudio(ref cvt);

    internal Result<byte[], string> Convert(
        ReadOnlySpan<byte> src,
        SDLAudioFormat srcFormat,
        byte srcChannels,
        uint srcRate,
        SDLAudioFormat dstFormat,
        byte dstChannels,
        uint dstRate
    ) {
        int res = BuildAudioCVT(
            out SDLAudioCVT converter,
            srcFormat,
            srcChannels,
            (int)srcRate,
            dstFormat,
            dstChannels,
            (int)dstRate
        );

        if (res == 0) {
            return new(src.ToArray());
        } else if (res < 0) {
            return new(SDLInternal.GetError());
        }

        converter.Len = src.Length;

        int cvtBufLen = converter.Len*converter.Len_mult;
        int finalLen = (int)(converter.Len*converter.Len_ratio);
        byte[] buf = new byte[cvtBufLen];
        byte[] finalBuf = new byte[finalLen];

        unsafe {
            fixed (byte* bufPtr = &buf[0]) {
                converter.Buf = (IntPtr)bufPtr;

                var conversionBuffer = new Span<byte>((void*)converter.Buf, converter.Len);
                src.CopyTo(conversionBuffer);

                if (ConvertAudio(ref converter) != 0) {
                    return new(SDLInternal.GetError());
                }

                var convertedBuffer = new Span<byte>((void*)converter.Buf, finalLen);
                var newBuffer = finalBuf.AsSpan();
                convertedBuffer.CopyTo(newBuffer);
            }
        }

        return new(finalBuf);
    }

    internal void MixAudioFormat(
        IntPtr dst,
        IntPtr src,
        SDLAudioFormat format,
        uint len,
        float volume
    ) => SDLInternal.MixAudioFormat(dst, src, format, len, (int)(volume*128));
}
#pragma warning restore CA1822 // Mark members as static
