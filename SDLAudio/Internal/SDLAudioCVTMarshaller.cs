namespace SDLAudio.Internal;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(SDLAudioCVT), MarshalMode.ManagedToUnmanagedRef, typeof(SDLAudioCVTMarshaller))]
[CustomMarshaller(typeof(SDLAudioCVT), MarshalMode.ManagedToUnmanagedOut, typeof(SDLAudioCVTMarshaller))]
internal static unsafe class SDLAudioCVTMarshaller {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Unmanaged {
        public int Needed;
        public SDLAudioFormat Src_format;
        public SDLAudioFormat Dst_format;
        public double Rate_incr;
        public byte* Buf;
        public int Len;
        public int Len_cvt;
        public int Len_mult;
        public double Len_ratio;
        public void* Filter1;
        public void* Filter2;
        public void* Filter3;
        public void* Filter4;
        public void* Filter5;
        public void* Filter6;
        public void* Filter7;
        public void* Filter8;
        public void* Filter9;
        public void* Filter10;
        public int Filter_index;
    }

    public static Unmanaged ConvertToUnmanaged(SDLAudioCVT managed) => new() {
        Needed = managed.Needed,
        Src_format = managed.Src_format,
        Dst_format = managed.Dst_format,
        Rate_incr = managed.Rate_incr,
        Buf = (byte*) managed.Buf,
        Len = managed.Len,
        Len_cvt = managed.Len_cvt,
        Len_mult = managed.Len_mult,
        Len_ratio = managed.Len_ratio,
        Filter1 = (void*) managed.Filters[0],
        Filter2 = (void*) managed.Filters[1],
        Filter3 = (void*) managed.Filters[2],
        Filter4 = (void*) managed.Filters[3],
        Filter5 = (void*) managed.Filters[4],
        Filter6 = (void*) managed.Filters[5],
        Filter7 = (void*) managed.Filters[6],
        Filter8 = (void*) managed.Filters[7],
        Filter9 = (void*) managed.Filters[8],
        Filter10 = (void*) managed.Filters[9],
        Filter_index = managed.Filter_index
    };

    public static SDLAudioCVT ConvertToManaged(Unmanaged unmanaged) => new() {
        Needed = unmanaged.Needed,
        Src_format = unmanaged.Src_format,
        Dst_format = unmanaged.Dst_format,
        Rate_incr = unmanaged.Rate_incr,
        Buf = (IntPtr) unmanaged.Buf,
        Len = unmanaged.Len,
        Len_cvt = unmanaged.Len_cvt,
        Len_mult = unmanaged.Len_mult,
        Len_ratio = unmanaged.Len_ratio,
        Filters = new IntPtr[] {
            (IntPtr)unmanaged.Filter1,
            (IntPtr)unmanaged.Filter2,
            (IntPtr)unmanaged.Filter3,
            (IntPtr)unmanaged.Filter4,
            (IntPtr)unmanaged.Filter5,
            (IntPtr)unmanaged.Filter6,
            (IntPtr)unmanaged.Filter7,
            (IntPtr)unmanaged.Filter8,
            (IntPtr)unmanaged.Filter9,
            (IntPtr)unmanaged.Filter10
        },
        Filter_index = unmanaged.Filter_index
    };
}
