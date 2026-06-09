namespace SDLAudio.Internal;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(SDLAudioSpec), MarshalMode.ManagedToUnmanagedRef, typeof(SDLAudioSpecMarshaller))]
[CustomMarshaller(typeof(SDLAudioSpec), MarshalMode.ManagedToUnmanagedOut, typeof(SDLAudioSpecMarshaller))]
internal static unsafe class SDLAudioSpecMarshaller {
    internal struct Unmanaged {
        public int Freq;
        public SDLAudioFormat Format;
        public byte Channels;
        public byte Silence;
        public ushort Samples;
        public ushort Padding;
        public uint Size;
        public IntPtr Callback;
        public void* Userdata;
    }

    public static Unmanaged ConvertToUnmanaged(SDLAudioSpec managed) =>
        new() {
            Freq = managed.Freq,
            Format = managed.Format,
            Channels = managed.Channels,
            Silence = managed.Silence,
            Samples = managed.Samples,
            Padding = 0,
            Callback = managed.Callback switch {
                null => 0,
                SDLAudioCallback cb => Marshal.GetFunctionPointerForDelegate(cb)
            },
            Userdata = null
        };

    public static SDLAudioSpec ConvertToManaged(Unmanaged unmanaged) =>
        new() {
            Freq = unmanaged.Freq,
            Format = unmanaged.Format,
            Channels = unmanaged.Channels,
            Silence = unmanaged.Silence,
            Samples = unmanaged.Samples,
            Callback = unmanaged.Callback == 0
                ? default
                : Marshal.GetDelegateForFunctionPointer<SDLAudioCallback>(unmanaged.Callback)
        };
}
