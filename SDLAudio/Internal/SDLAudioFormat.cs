namespace SDLAudio.Internal;

using System;
using System.Text;

/// <summary>
/// An audio format. A thin wrapper around SDL_AudioFormat, with some helper functions.
/// </summary>
internal readonly struct SDLAudioFormat {
    public static SDLAudioFormat U8 => new(0x0008);
    public static SDLAudioFormat S8 => new(0x8008);
    public static SDLAudioFormat U16LE => new(0x0010);
    public static SDLAudioFormat S16LE => new(0x8010);
    public static SDLAudioFormat U16BE => new(0x1010);
    public static SDLAudioFormat S16BE => new(0x9010);
    public static SDLAudioFormat S32LE => new(0x8020);
    public static SDLAudioFormat S32BE => new(0x9020);
    public static SDLAudioFormat F32LE => new(0x8120);
    public static SDLAudioFormat F32BE => new(0x9120);
    public static SDLAudioFormat F32Native => BitConverter.IsLittleEndian ? F32LE : F32BE;

    // https://wiki.libsdl.org/SDL2/SDL_AudioFormat
    private readonly ushort sdlFormat;

    public readonly bool IsSigned => sdlFormat >> 15 == 1;
    public readonly bool IsBigEndian => ((sdlFormat >> 12) & 0b1) == 1;
    public readonly bool IsOppositeEndian => IsBigEndian == BitConverter.IsLittleEndian;
    public readonly bool IsFloat => ((sdlFormat >> 8) & 0b1) == 1;
    public readonly byte SampleSize => (byte)(sdlFormat & 0b11111111);

    public SDLAudioFormat(ushort sdlFormat) {
        this.sdlFormat = sdlFormat;
    }

    public override readonly string ToString() => new StringBuilder()
        .Append(IsSigned ? "signed " : "unsigned ")
        .Append(IsBigEndian ? "big endian " : "little endian ")
        .Append(SampleSize).Append(" bit ")
        .Append(IsFloat ? "floating point" : "integer")
        .ToString();
}
