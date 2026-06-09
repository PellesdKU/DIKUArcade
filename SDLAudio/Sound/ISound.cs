namespace SDLAudio.Sound;

using System;
using Internal;

public interface ISound {
    internal SDLAudioFormat Format { get; }

    uint SampleRate { get; }
    byte Channels { get; }

    bool IsDone(ulong playhead);
    ReadOnlySpan<byte> GetSamples(ulong playhead, uint len);
}
