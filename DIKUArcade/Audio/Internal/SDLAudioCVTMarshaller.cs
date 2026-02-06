namespace DIKUArcade.Audio.Internal;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(SDL_AudioCVT), MarshalMode.ManagedToUnmanagedRef, typeof(SDLAudioCVTMarshaller))]
internal static unsafe class SDLAudioCVTMarshaller {
    [StructLayout(LayoutKind.Explicit)]
    internal struct SDL_AudioCVTUnmanaged {
        [FieldOffset(0)] public int Needed;
        [FieldOffset(4)] public ushort Src_format;
        [FieldOffset(6)] public ushort Dst_format;
        [FieldOffset(8)] public double Rate_incr;
        [FieldOffset(16)] public byte* Buf;
        [FieldOffset(24)] public int Len;
        [FieldOffset(28)] public int Len_cvt;
        [FieldOffset(32)] public int Len_mult;
        [FieldOffset(36)] public double Len_ratio;
        [FieldOffset(44)] public void* Filters;
        [FieldOffset(44 + 10*8)] public int Filter_index;
    }

    public static SDL_AudioCVTUnmanaged ConvertToUnmanaged(SDL_AudioCVT managed) {
        SDL_AudioCVTUnmanaged ret;
        ret.Needed = managed.Needed;
        ret.Src_format = managed.Src_format;
        ret.Dst_format = managed.Dst_format;
        ret.Rate_incr = managed.Rate_incr;
        ret.Buf = (byte*) managed.Buf;
        ret.Len = managed.Len;
        ret.Len_cvt = managed.Len_cvt;
        ret.Len_mult = managed.Len_mult;
        ret.Len_ratio = managed.Len_ratio;
        ret.Filters = (void*) managed.Filters;
        ret.Filter_index = managed.Filter_index;
        return ret;
    }

    public static SDL_AudioCVT ConvertToManaged(SDL_AudioCVTUnmanaged unmanaged) {
        SDL_AudioCVT ret;
        ret.Needed = unmanaged.Needed;
        ret.Src_format = unmanaged.Src_format;
        ret.Dst_format = unmanaged.Dst_format;
        ret.Rate_incr = unmanaged.Rate_incr;
        ret.Buf = (IntPtr) unmanaged.Buf;
        ret.Len = unmanaged.Len;
        ret.Len_cvt = unmanaged.Len_cvt;
        ret.Len_mult = unmanaged.Len_mult;
        ret.Len_ratio = unmanaged.Len_ratio;
        ret.Filters = (IntPtr) unmanaged.Filters;
        ret.Filter_index = unmanaged.Filter_index;
        return ret;
    }
}