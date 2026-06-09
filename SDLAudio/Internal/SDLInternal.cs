namespace SDLAudio.Internal;

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using Device;

/// <summary>
/// Wraps the raw SDL functions. Should not be used directly.
/// Legacy functions are not included.
/// </summary>
internal static partial class SDLInternal {
    internal static bool Initialized { get; private set; } = false;

    private const string LIB_NAME = "SDL2";
    private const uint INIT_AUDIO = 0x00000010;

    internal static bool InitAudio() => Initialized = InitSubSystem(INIT_AUDIO) == 0;
    internal static void QuitAudio() => QuitSubSystem(INIT_AUDIO);

    // # ----------------- #
    // # General functions #
    // # ----------------- #

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_InitSubSystem")]
    internal static partial int InitSubSystem(uint flags);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_QuitSubSystem")]
    internal static partial void QuitSubSystem(uint flags);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetError", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NoFreeUTF8Marshaller))]
    internal static partial string GetError();

    // # --------------- #
    // # Audio functions #
    // # --------------- #

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetNumAudioDrivers")]
    internal static partial int GetNumAudioDrivers();

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetAudioDriver", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NoFreeUTF8Marshaller))]
    internal static partial string GetAudioDriver(int index);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetCurrentAudioDriver", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NoFreeUTF8Marshaller))]
    internal static partial string GetCurrentAudioDriver();

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetNumAudioDevices")]
    internal static partial int GetNumAudioDevices(DeviceType deviceType);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetAudioDeviceName", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NoFreeUTF8Marshaller))]
    internal static partial string GetAudioDeviceName(int index, DeviceType deviceType);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetAudioDeviceSpec")]
    internal static partial int GetAudioDeviceSpec(
        int index,
        DeviceType deviceType,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] out SDLAudioSpec spec
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetDefaultAudioInfo", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int GetDefaultAudioInfo(
        out string name,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDLAudioSpec spec,
        DeviceType deviceType
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_OpenAudioDevice")]
    internal static partial SDLAudioDeviceID OpenAudioDevice(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? device,
        DeviceType deviceType,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDLAudioSpec desired,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] out SDLAudioSpec obtained,
        SDLAllowChange allowedChanges
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetAudioDeviceStatus")]
    internal static partial SDLAudioStatus GetAudioDeviceStatus(
        SDLAudioDeviceID dev
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_PauseAudioDevice")]
    internal static partial void PauseAudioDevice(
        SDLAudioDeviceID dev,
        [MarshalUsing(typeof(BoolToIntMarshaller))] bool pauseOn
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_RWFromFile", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr RWFromFile(string file, string mode);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_RWFromMem")]
    internal static partial IntPtr RWFromMem(IntPtr mem, int size);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_RWclose")]
    internal static partial int RWclose(IntPtr context);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_LoadWAV_RW")]
    internal static partial IntPtr LoadWAV_RW(
        IntPtr /* SDL_RWops* */ src,
        [MarshalUsing(typeof(BoolToIntMarshaller))] bool freesrc,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] out SDLAudioSpec spec,
        out IntPtr /* Uint8 ** */ audioBuf,
        out uint audioLen
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_FreeWAV")]
    internal static partial void FreeWAV(IntPtr /* Uint8* */ audioBuf);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_BuildAudioCVT")]
    internal static partial int BuildAudioCVT(
        [MarshalUsing(typeof(SDLAudioCVTMarshaller))] out SDLAudioCVT cvt,
        SDLAudioFormat srcFormat,
        byte srcChannels,
        int srcRate,
        SDLAudioFormat dstFormat,
        byte dstChannels,
        int dstRate
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_ConvertAudio")]
    internal static partial int ConvertAudio(
        [MarshalUsing(typeof(SDLAudioCVTMarshaller))] ref SDLAudioCVT cvt
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_NewAudioStream")]
    internal static partial IntPtr /* SDL_AudioStream* */ NewAudioStream(
        SDLAudioFormat src_format,
        byte src_channels,
        int src_rate,
        SDLAudioFormat dst_format,
        byte dst_channels,
        int dst_rate
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_AudioStreamPut")]
    internal static partial int AudioStreamPut(IntPtr /* SDL_AudioStream* */ stream, IntPtr /* void* */ buf, int len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_AudioStreamGet")]
    internal static partial int AudioStreamGet(IntPtr /* SDL_AudioStream* */ stream, IntPtr /* void* */ buf, int len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_AudioStreamAvailable")]
    internal static partial int AudioStreamAvailable(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_AudioStreamFlush")]
    internal static partial int AudioStreamFlush(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_AudioStreamClear")]
    internal static partial void AudioStreamClear(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_FreeAudioStream")]
    internal static partial void FreeAudioStream(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_MixAudioFormat")]
    internal static partial void MixAudioFormat(
        IntPtr /* Uint8* */ dst,
        IntPtr /* Uint8* */ src,
        SDLAudioFormat format,
        uint len,
        int volume
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_QueueAudio")]
    internal static partial int QueueAudio(
        SDLAudioDeviceID dev,
        IntPtr /* void* */ data,
        uint len
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_DequeueAudio")]
    internal static partial uint DequeueAudio(
        SDLAudioDeviceID dev,
        IntPtr /* void* */ data,
        uint len
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_GetQueuedAudioSize")]
    internal static partial uint GetQueuedAudioSize(
        SDLAudioDeviceID dev
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_ClearQueuedAudio")]
    internal static partial void ClearQueuedAudio(
        SDLAudioDeviceID dev
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_LockAudioDevice")]
    internal static partial void LockAudioDevice(
        SDLAudioDeviceID dev
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_UnlockAudioDevice")]
    internal static partial void UnlockAudioDevice(
        SDLAudioDeviceID dev
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, EntryPoint = "SDL_CloseAudioDevice")]
    internal static partial void CloseAudioDevice(
        SDLAudioDeviceID dev
    );
}
