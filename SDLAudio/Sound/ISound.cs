namespace SDLAudio.Sound;

using Format;
using Result;

public interface ISound {
    uint SampleRate { get; }
    byte Channels { get; }

    bool IsDone(ulong playhead);

    internal Result<Sound<Sample, Accu, Format>, string> Convert<Sample, Accu, Format>(
        SDLAudio sdlAudio,
        uint dstRate,
        byte dstChannels
    )
        where Format : IAudioFormat<Sample, Accu>
        where Sample : struct
        where Accu : struct;
}
