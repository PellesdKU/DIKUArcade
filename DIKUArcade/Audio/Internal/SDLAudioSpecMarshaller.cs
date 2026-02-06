namespace DIKUArcade.Audio.Internal;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(SDL_AudioSpec), MarshalMode.ManagedToUnmanagedRef, typeof(SDLAudioSpecMarshaller))]
[CustomMarshaller(typeof(SDL_AudioSpec), MarshalMode.UnmanagedToManagedRef, typeof(SDLAudioSpecMarshaller))]

internal static unsafe class SDLAudioSpecMarshaller {
    internal struct SDL_AudioSpecUnmanaged {
        public int Freq;
        public ushort Format;
        public byte Channels;
        public byte Silence;
        public byte Samples;
        public byte Padding;
        public delegate*<void*, byte*, int, void> Callback;
        public IntPtr Userdata;
    }

    public static SDL_AudioSpecUnmanaged ConvertToUnmanaged(SDL_AudioSpec managed) {
        SDL_AudioSpecUnmanaged ret;
        ret.Freq = managed.Freq;
        ret.Format = managed.Format;
        ret.Channels = managed.Channels;
        ret.Silence = managed.Silence;
        ret.Samples = managed.Samples;
        ret.Padding = managed.Padding;
        ret.Callback = managed.Callback == null ? null : (delegate*<void*, byte*, int, void>) Marshal.GetFunctionPointerForDelegate(managed.Callback);
        ret.Userdata = managed.Userdata;
        return ret;
    }

    public static SDL_AudioSpec ConvertToManaged(SDL_AudioSpecUnmanaged unmanaged) {
        SDL_AudioSpec ret;
        ret.Freq = unmanaged.Freq;
        ret.Format = unmanaged.Format;
        ret.Channels = unmanaged.Channels;
        ret.Silence = unmanaged.Silence;
        ret.Samples = unmanaged.Samples;
        ret.Padding = unmanaged.Padding;
#pragma warning disable CS8601 // Possible null reference assignment.
        ret.Callback = unmanaged.Callback == null ? default : Marshal.GetDelegateForFunctionPointer<SDL_AudioCallback>((IntPtr)unmanaged.Callback);
#pragma warning restore CS8601 // Possible null reference assignment.
        ret.Userdata = unmanaged.Userdata;
        return ret;
    }
}