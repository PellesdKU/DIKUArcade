namespace SDLAudio;

using System;
using System.Text;

/// <summary>
/// An audio format. A thin wrapper around SDL_AudioFormat, with some helper functions.
/// </summary>
internal struct AudioFormat {
    private readonly ushort sdlFormat;

    // https://wiki.libsdl.org/SDL2/SDL_AudioFormat
    public readonly bool IsSigned => sdlFormat >> 15 == 1;
    public readonly bool IsLittleEndian => ((sdlFormat >> 12) & 0b1) == 0;
    public readonly bool IsOppositeEndian => IsLittleEndian != BitConverter.IsLittleEndian;
    public readonly bool IsFloat => ((sdlFormat >> 8) & 0b1) == 1;
    public readonly byte SampleSize => (byte)(sdlFormat & 0b11111111);

    internal AudioFormat(ushort sdlFormat) {
        this.sdlFormat = sdlFormat;
    }

    public override readonly string ToString() => new StringBuilder()
        .Append(IsSigned ? "signed " : "unsigned ")
        .Append(IsLittleEndian ? "little endian " : "big endian ")
        .Append(SampleSize).Append(" bit ")
        .Append(IsFloat ? "floating point" : "integer")
        .ToString();
}
