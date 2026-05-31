namespace SDLAudio.Sound;

using System;

public interface ISound {
    public bool IsDone(uint playhead);
    public ReadOnlySpan<byte> GetSamples(uint playhead, uint len);
}
