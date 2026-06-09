namespace SDLAudio.Sound;

using System;
using System.Runtime.InteropServices;
using Format;
using Internal;

public abstract class Sound<Sample, Accu, Format> : ISound
    where Sample : struct
    where Accu : struct
    where Format : IAudioFormat<Sample, Accu>
{
    SDLAudioFormat ISound.Format => Format.SDLFormat;

    public uint SampleRate { get; private init; }
    public byte Channels { get; private init; }

    public Sound(uint sampleRate, byte channels) {
        SampleRate = sampleRate;
        Channels = channels;
    }

    public abstract bool IsDone(ulong playhead);
    public abstract ReadOnlySpan<Sample> GetSamples(ulong playhead, uint samples);

    ReadOnlySpan<byte> ISound.GetSamples(ulong playhead, uint len) =>
        MemoryMarshal.Cast<Sample, byte>(GetSamples(playhead, len));
}
