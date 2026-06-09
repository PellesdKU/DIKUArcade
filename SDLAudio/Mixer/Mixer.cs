namespace SDLAudio.Mixer;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Format;

internal struct Mixer<Sample, Accu, Format>
    where Sample : struct
    where Accu : IAdditionOperators<Accu, Accu, Accu>
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

    public readonly void AddChannel(ReadOnlySpan<Sample> audio, float volume) {
        uint i = 0;

        if (oppositeEndian) {
            Sample[] temp = audio.ToArray();
            ReverseEndianness(temp);
            audio = temp.AsSpan();
        }

        foreach (Sample sample in audio) {
            buf[i] += Format.VolumeAdjust(sample, volume);
            i++;
        }
    }

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

    public void ClearSamples(uint samples) {
        if (buf.Length < samples) {
            buf = new Accu[samples];
        }

        buf.AsSpan().Clear();
    }
}
