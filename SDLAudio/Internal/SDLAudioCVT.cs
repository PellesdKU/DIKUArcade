namespace SDLAudio.Internal;

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SDLAudioCVT {
    public int Needed;
    public AudioFormat Src_format;
    public AudioFormat Dst_format;
    public double Rate_incr;
    public IntPtr /* Uint8* */ Buf;
    public int Len;
    public int Len_cvt;
    public int Len_mult;
    public double Len_ratio;
    public IntPtr[] /* SDL_AudioFilter */ Filters;
    public int Filter_index;
}
