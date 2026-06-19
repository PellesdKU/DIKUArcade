namespace SDLAudio.Sound;

using System.Numerics;
using Format;
using Result;

/// <summary>
/// A sound that can be played on a device.
/// </summary>
public interface ISound {
    internal Result<Sound<Sample, Accu, Format>, string> Convert<Sample, Accu, Format>(
        SDLAudio sdlAudio,
        uint dstRate,
        byte dstChannels
    )
        where Format : IAudioFormat<Sample, Accu>
        where Sample : struct
        where Accu : struct, IAdditionOperators<Accu, Accu, Accu>;
}
