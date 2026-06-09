namespace SDLAudio.Sound;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Result;
using Format;

internal class Clip<Sample, Accu, Format> : Sound<Sample, Accu, Format>, IEnumerable<Sample>
    where Sample : struct
    where Accu : struct
    where Format : IAudioFormat<Sample, Accu>
{
    private readonly byte[] rawData;
    private ReadOnlySpan<Sample> Data => MemoryMarshal.Cast<byte, Sample>(rawData);

    public uint Length => (uint)Data.Length;

    public override bool IsDone(ulong playhead) => playhead > Length;

    public override ReadOnlySpan<Sample> GetSamples(ulong playhead, uint samples) {
        if ((int)playhead > Data.Length) {
            return new();
        }

        int index = int.Min((int)playhead, Data.Length);
        int remaining = (int)Length - index;
        int len = int.Min(remaining, (int)samples);

        return Data.Slice(index, len);
    }

    protected override Result<Sound<SampleTo, AccuTo, FormatTo>, string> Convert<SampleTo, AccuTo, FormatTo>(
        SDLAudio sdlAudio, uint dstRate, byte dstChannels
    )
        where SampleTo : struct
        where AccuTo : struct
    => sdlAudio.Convert(
        rawData,
        Format.SDLFormat,
        Channels,
        SampleRate,
        FormatTo.SDLFormat,
        dstChannels,
        dstRate
    ).MapOk(newData => new Clip<SampleTo, AccuTo, FormatTo>(
        newData,
        dstRate,
        dstChannels
    ) as Sound<SampleTo, AccuTo, FormatTo>);

    public Clip(
        byte[] audioData,
        uint sampleRate,
        byte channels
    ) : base(sampleRate, channels) {
        rawData = audioData;
    }

    public IEnumerator<Sample> GetEnumerator() => new AudioEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class AudioEnumerator : IEnumerator<Sample> {
        private readonly Clip<Sample, Accu, Format> audio;
        private int index = -1;

        public AudioEnumerator(Clip<Sample, Accu, Format> audio) {
            this.audio = audio;
        }

        public Sample Current => audio.Data[index];

        object IEnumerator.Current => Current;

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public bool MoveNext() {
            index++;

            return index < audio.Length;
        }

        public void Reset() {
            index = -1;
        }
    }
}
