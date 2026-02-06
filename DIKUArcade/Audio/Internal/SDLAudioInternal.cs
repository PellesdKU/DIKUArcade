namespace DIKUArcade.Audio.Internal;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices.Marshalling;

public delegate void SDL_AudioCallback(IntPtr /* void* */ userdata, IntPtr /* uint8* */ stream, int len);

public struct SDL_AudioSpec {
    public int Freq;
    public ushort /* SDL_AudioFormat */ Format;
    public byte Channels;
    public byte Silence;
    public byte Samples;
    public byte Padding;
    public SDL_AudioCallback Callback;
    public IntPtr Userdata;
}

[StructLayout(LayoutKind.Explicit)]
public struct SDL_AudioCVT {
    [FieldOffset(0)] public int Needed;
    [FieldOffset(4)] public ushort /* SDL_AudioFormat */ Src_format;
    [FieldOffset(6)] public ushort /* SDL_AudioFormat */ Dst_format;
    [FieldOffset(8)] public double Rate_incr;
    [FieldOffset(16)] public IntPtr /* Uint8* */ Buf;
    [FieldOffset(24)] public int Len;
    [FieldOffset(28)] public int Len_cvt;
    [FieldOffset(32)] public int Len_mult;
    [FieldOffset(36)] public double Len_ratio;
    // following fields are for SDL internal use
    [FieldOffset(44)] public IntPtr /* SDL_AudioFilter */ Filters;
    [FieldOffset(44 + 10*8)] public int Filter_index;
}

enum SDL_AudioStatus {
    SDL_AUDIO_STOPPED = 0,
    SDL_AUDIO_PLAYING = 1,
    SDL_AUDIO_PAUSED = 2,
}

/// <summary>
/// Wraps the raw SDL audio functions. Should not be used directly.
/// Legacy functions have not been implemented.
/// </summary>
internal static partial class SDLAudioInternal {
    private const string LIB_NAME = "SDL2";
    internal const uint SDL_INIT_AUDIO = 0x00000010;

    internal const int SDL_AUDIO_ALLOW_FREQUENCY_CHANGE = 0x00000001;
    internal const int SDL_AUDIO_ALLOW_FORMAT_CHANGE    = 0x00000002;
    internal const int SDL_AUDIO_ALLOW_CHANNELS_CHANGE  = 0x00000004;
    internal const int SDL_AUDIO_ALLOW_SAMPLES_CHANGE   = 0x00000008;
    internal const int SDL_AUDIO_ALLOW_ANY_CHANGE       = SDL_AUDIO_ALLOW_FREQUENCY_CHANGE|SDL_AUDIO_ALLOW_FORMAT_CHANGE|SDL_AUDIO_ALLOW_CHANNELS_CHANGE|SDL_AUDIO_ALLOW_SAMPLES_CHANGE;

    // not specifically related to audio, but we would like to be able to initialize the audio subsystem separately
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_InitSubSystem(UInt32 flags);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string SDL_GetError();

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_GetNumAudioDrivers();

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    [return: MarshalUsing(typeof(NoFreeUTF8Marshaller))]
    internal static partial string SDL_GetAudioDriver(int index);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    [return: MarshalUsing(typeof(NoFreeUTF8Marshaller))]
    internal static partial string SDL_GetCurrentAudioDriver();

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_GetNumAudioDevices(int iscapture);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string SDL_GetAudioDeviceName(int index, int iscapture);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_GetAudioDeviceSpec(int index, int iscapture, [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDL_AudioSpec spec);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_GetDefaultAudioInfo(ref IntPtr /* char** */ name, [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDL_AudioSpec spec, int iscapture);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial ushort /* SDL_AudioDeviceID */ SDL_OpenAudioDevice(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string device,
        int iscapture,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDL_AudioSpec desired,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDL_AudioSpec obtained,
        int allowed_changes
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial SDL_AudioStatus SDL_GetAudioDeviceStatus(ushort /* SDL_AudioDeviceID */ dev);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_PauseAudioDevice(ushort /* SDL_AudioDeviceID */ dev, int pause_on);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr SDL_RWFromFile(string file, string mode);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial IntPtr SDL_LoadWAV_RW(
        IntPtr /* SDL_RWops* */ src,
        int freesrc,
        [MarshalUsing(typeof(SDLAudioSpecMarshaller))] ref SDL_AudioSpec spec,
        ref IntPtr /* Uint8 ** */ audio_buf,
        ref UInt32 /* Uint32 */ audio_len
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_FreeWAV(IntPtr /* Uint8* */ audio_buf);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_BuildAudioCVT(
        [MarshalUsing(typeof(SDLAudioCVTMarshaller))] ref SDL_AudioCVT cvt,
        ushort /* SDL_AudioFormat */ src_format,
        byte src_channels,
        int src_rate,
        ushort /* SDL_AudioFormat */ dst_format,
        byte dst_channels,
        int dst_rate
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_ConvertAudio([MarshalUsing(typeof(SDLAudioCVTMarshaller))] ref SDL_AudioCVT cvt);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial IntPtr /* SDL_AudioStream* */ SDL_NewAudioStream(
        ushort /* SDL_AudioFormat */ src_format,
        byte src_channels,
        int src_rate,
        ushort /* SDL_AudioFormat */ dst_format,
        byte dst_channels,
        int dst_rate
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_AudioStreamPut(IntPtr /* SDL_AudioStream* */ stream, IntPtr /* void* */ buf, int len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_AudioStreamGet(IntPtr /* SDL_AudioStream* */ stream, IntPtr /* void* */ buf, int len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_AudioStreamAvailable(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_AudioStreamFlush(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_AudioStreamClear(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_FreeAudioStream(IntPtr /* SDL_AudioStream* */ stream);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_MixAudioFormat(
        IntPtr /* Uint8* */ dst,
        IntPtr /* Uint8* */ src,
        ushort /* SDL_AudioFormat */ format,
        UInt32 len,
        int volume
    );

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial int SDL_QueueAudio(ushort /* SDL_AudioDeviceID */ dev, IntPtr /* void* */ data, UInt32 len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial UInt32 SDL_DequeueAudio(ushort /* SDL_AudioDeviceID */ dev, IntPtr /* void* */ data, UInt32 len);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial UInt32 SDL_GetQueuedAudioSize(ushort /* SDL_AudioDeviceID */ dev);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_ClearQueuedAudio(ushort /* SDL_AudioDeviceID */ dev);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_LockAudioDevice(ushort /* SDL_AudioDeviceID */ dev);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_UnlockAudioDevice(ushort /* SDL_AudioDeviceID */ dev);

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [LibraryImport(LIB_NAME)]
    internal static partial void SDL_CloseAudioDevice(ushort /* SDL_AudioDeviceID */ dev);
}
