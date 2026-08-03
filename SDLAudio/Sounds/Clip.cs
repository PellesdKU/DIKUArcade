namespace SDLAudio.Sounds;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Result;
using Internal;

/// <summary>
/// A clip of audio of certain length.
/// </summary>
public class Clip : Sound, IEnumerable<float> {
    private readonly float[] data;

    /// <summary>
    /// Length of the clip in samples. This is across all channels, so a stereo clip will have
    /// twice the Length of a mono clip of the same duration.
    /// </summary>
    public uint Length => (uint)data.Length;

    public override bool IsDone(ulong playhead) => playhead > Length;

    /// <summary>
    /// Get a number of samples after some index (playhead). May return fewer samples than requested
    /// if, for example, the end of the clip has been reached.
    /// </summary>
    /// <param name="playhead">The index from which to return samples.</param>
    /// <param name="samples">The maximum number of samples to return.</param>
    public override ReadOnlySpan<float> GetSamples(ulong playhead, uint samples) {
        if ((int)playhead > data.Length) {
            return new();
        }

        int index = int.Min((int)playhead, data.Length);
        int remaining = (int)Length - index;
        int len = int.Min(remaining, (int)samples);

        return data.AsSpan().Slice(index, len);
    }

    public override Result<Sound, string> Convert(
        SDLAudio sdlAudio, uint dstRate, byte dstChannels
    ) => sdlAudio.Convert(
        MemoryMarshal.Cast<float, byte>(data),
        SDLAudioFormat.F32Native,
        Channels,
        SampleRate,
        SDLAudioFormat.F32Native,
        dstChannels,
        dstRate
    ).MapOk(newData => new Clip(
        MemoryMarshal.Cast<byte, float>(newData).ToArray(),
        dstRate,
        dstChannels
    ) as Sound);

    public Clip(
        float[] audioData,
        uint sampleRate,
        byte channels
    ) : base(sampleRate, channels) {
        data = audioData;
    }

    public IEnumerator<float> GetEnumerator() => new AudioEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class AudioEnumerator : IEnumerator<float> {
        private readonly Clip audio;
        private int index = -1;

        public AudioEnumerator(Clip audio) {
            this.audio = audio;
        }

        public float Current => audio.data[index];

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
