namespace SDLAudio.Mixer;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Format;

/// <summary>
/// A stateful sound mixer generic over audio format.
/// </summary>
/// <typeparam name="Sample">Sample type of the audio to mix.</typeparam>
/// <typeparam name="Accu">Accumulator type for temporary storage.</typeparam>
/// <typeparam name="Format">Format of audio to mix.</typeparam>
internal struct Mixer<Sample, Accu, Format>
    where Sample : struct
    where Accu : struct, IAdditionOperators<Accu, Accu, Accu>
    where Format : IAudioFormat<Sample, Accu>
{
    private static readonly uint monoSampleSize = (uint)Unsafe.SizeOf<Sample>();
    private Accu[] buf;
    private readonly bool oppositeEndian;

    public Mixer() {
        oppositeEndian = Format.SDLFormat.IsOppositeEndian;
        buf = Array.Empty<Accu>();
    }

    private static void ReverseEndianness(Span<Sample> samples) {
        Span<byte> byteSpan = MemoryMarshal.Cast<Sample, byte>(samples);
        for (uint i = 0; i < samples.Length; i++) {
            byteSpan.Slice((int)(i*monoSampleSize), (int)monoSampleSize).Reverse();
        }
    }

    /// <summary>
    /// Add a new channel to the mix. Note that ClearSamples() must have been called with a length
    /// greater than or equal to the Length of <paramref name="audio" />.
    /// </summary>
    /// <param name="audio">Audio data to mix in.</param>
    /// <param name="volume">Volume to adjust audio with.</param>
    public readonly void AddChannel(ReadOnlySpan<Sample> audio, float volume) {
        if (oppositeEndian) {
            Sample[] temp = audio.ToArray();
            ReverseEndianness(temp);
            audio = temp.AsSpan();
        }

        uint i = 0;
        foreach (Sample sample in audio) {
            buf[i] += Format.VolumeAdjust(sample, volume);
            i++;
        }
    }

    /// <summary>
    /// Write the mixed audio to a span, clamping the mixed audio to the range of the format.
    /// Note that ClearSamples() must have been called with a length greater than or equal to
    /// the Length of <paramref name="dst" />.
    /// </summary>
    /// <param name="dst">Destination span to write to.</param>
    public readonly void Mix(Span<Sample> dst) {
        Sample[] mix = new Sample[buf.Length];

        for (int i = 0; i < mix.Length; i++) {
            mix[i] = Format.Clamp(buf[i]);
        }

        Span<Sample> mixSpan = mix.AsSpan();

        if (oppositeEndian) {
            ReverseEndianness(mixSpan);
        }

        mixSpan.CopyTo(dst);
    }

    /// <summary>
    /// Clear the mix buffer, and expand it to <paramref name="length" />
    /// </summary>
    /// <param name="length">Length to expand the buffer to.</param>
    public void ClearSamples(uint length) {
        if (buf.Length < length) {
            buf = new Accu[length];
        }

        buf.AsSpan().Clear();
    }
}
