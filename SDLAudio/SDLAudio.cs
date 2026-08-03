namespace SDLAudio;

using System;
using System.IO;
using System.Collections.Generic;
using Result;
using Internal;
using Device;
using Sounds;
using System.Runtime.InteropServices;

internal delegate void SDLAudioCallback(IntPtr /* void* */ userdata, IntPtr /* uint8* */ stream, int len);

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// This class acts as a guard around SDL Audio, preventing the audio functions from being called
/// before the audio subsystem is initalized, as well as caching the available devices.
/// </summary>
public class SDLAudio {
    private readonly Dictionary<string, int> playbackDevices;

    private SDLAudio() {
        playbackDevices = new();

        RefreshPlaybackDevices();
    }

    ~SDLAudio() {
        // SDL doesn't actually tear down the audio subsystem before quit has been called
        // as many times as init was. Think of it as a reference counted object.
        SDLInternal.QuitAudio();
    }

    /// <summary>
    /// Refresh the cache of playback devices.
    /// </summary>
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

    /// <summary>
    /// Get the names of known playback devices.
    /// </summary>
    public IEnumerable<string> GetPlaybackDevices() => playbackDevices.Keys;

    /// <summary>
    /// Get the name of the default playback device. Note that this is not necessarily the same
    /// device as will be opened by OpenDefaultPlaybackDevice().
    /// </summary>
    public Result<string, string> GetDefaultDevice() {
        SDLAudioSpec spec = default;
        if(!GetDefaultAudioInfo(out string name, ref spec, DeviceType.PLAYBACK)) {
            return Result<string, string>
                .Err($"Couldn't get default device: {SDLInternal.GetError()}");
        }

        return Result<string, string>.Ok(name);
    }

    /// <summary>
    /// Open a playback device by name.
    /// </summary>
    /// <param name="name">
    /// Name of the playback device to open. One of the names returned by GetPlaybackDevices().
    /// </param>
    /// <returns>An opened audio device, or an error message.</returns>
    public Result<AudioDevice, string> OpenPlaybackDevice(string name) {
        lock (playbackDevices) {
            if(!playbackDevices.TryGetValue(name, out int index)) {
                return new($"Unknown device: {name}");
            }

            if(!GetAudioDeviceSpec(index, DeviceType.PLAYBACK, out SDLAudioSpec spec)) {
                return new($"Couldn't get device specification for {name}: {SDLInternal.GetError()}");
            }

            DeviceCallbackProxy proxy = new();
            spec.Callback = proxy.Listener;
            spec.Format = SDLAudioFormat.F32Native;

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

            return new(obtainedSpec.MakeDevice(this, id, proxy));
        }
    }

    /// <summary>
    /// Open the default playback device. Note that this is not necessarily the same device whose
    /// name is returned by GetDefaultDevice().
    /// </summary>
    /// <returns></returns>
    public Result<AudioDevice, string> OpenDefaultPlaybackDevice() {
        SDLAudioSpec spec = new();
        DeviceCallbackProxy proxy = new();
        spec.Callback = proxy.Listener;
        spec.Format = SDLAudioFormat.F32Native;

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

        return new(obtainedSpec.MakeDevice(this, id, proxy));
    }

    /// <summary>
    /// Load a sound from a stream of a .wav file.
    /// </summary>
    /// <param name="stream">Stream of a .wav file.</param>
    /// <returns>A Result of a Sound or a loading error.</returns>
    public Result<Sound, string> LoadWAV(SDLAudio sdlAudio, Stream stream) {
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
                new ReadOnlySpan<byte>((void*)wavData, (int)len).CopyTo(soundData);

                sdlAudio.FreeWAV(wavData);

                return sdlAudio.Convert(
                    soundData,
                    spec.Format,
                    spec.Channels,
                    (uint)spec.Freq,
                    SDLAudioFormat.F32Native,
                    spec.Channels,
                    (uint)spec.Freq
                ).MapOk(newData => spec.MakeClip(
                    MemoryMarshal.Cast<byte, float>(newData).ToArray()
                ) as Sound);
            }
        }
    }

    /// <summary>
    /// Initialize the SDL Audio subsystem.
    /// </summary>
    /// <returns>An SDLAudio on success, or an error message.</returns>
    public static Result<SDLAudio, string> Create() {
        if (SDLInternal.InitAudio()) {
            return new(new SDLAudio());
        }

        return new($"Couldn't initialize SDL_Audio: {SDLInternal.GetError()}");
    }

    /// <summary>
    /// Get the name of the audio driver that SDL Audio was initialized with.
    /// </summary>
    public string GetCurrentAudioDriver() => SDLInternal.GetCurrentAudioDriver();

    internal bool GetDefaultAudioInfo(
        out string name,
        ref SDLAudioSpec spec,
        DeviceType deviceType
    ) => SDLInternal.GetDefaultAudioInfo(out name, ref spec, deviceType) == 0;

    internal int GetNumAudioDevices(DeviceType deviceType) =>
        SDLInternal.GetNumAudioDevices(deviceType);

    internal string GetAudioDeviceName(int index, DeviceType deviceType) =>
        SDLInternal.GetAudioDeviceName(index, deviceType);

    internal bool GetAudioDeviceSpec(int index, DeviceType deviceType, out SDLAudioSpec spec) =>
        SDLInternal.GetAudioDeviceSpec(index, deviceType, out spec) == 0;

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

    internal void CloseAudioDevice(SDLAudioDeviceID id) =>
        SDLInternal.CloseAudioDevice(id);

    internal void PauseAudioDevice(SDLAudioDeviceID id, bool pause) =>
        SDLInternal.PauseAudioDevice(id, pause);

    internal SDLAudioStatus GetAudioDeviceStatus(SDLAudioDeviceID id) =>
        SDLInternal.GetAudioDeviceStatus(id);

    internal void LockAudioDevice(SDLAudioDeviceID id) =>
        SDLInternal.LockAudioDevice(id);
    internal void UnlockAudioDevice(SDLAudioDeviceID id) =>
        SDLInternal.UnlockAudioDevice(id);

    internal IntPtr LoadWAV_RW(
        IntPtr src,
        bool freesrc,
        out SDLAudioSpec spec,
        out IntPtr audioBuf,
        out uint audioLen
    ) => SDLInternal.LoadWAV_RW(src, freesrc, out spec, out audioBuf, out audioLen);

    internal void FreeWAV(IntPtr audioBuf) =>
        SDLInternal.FreeWAV(audioBuf);

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

    internal int ConvertAudio(ref SDLAudioCVT cvt) =>
        SDLInternal.ConvertAudio(ref cvt);

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
