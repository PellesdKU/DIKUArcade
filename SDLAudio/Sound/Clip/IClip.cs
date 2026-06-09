namespace SDLAudio.Sound.Clip;

using Result;
using Format;

public interface IClip {
    internal Result<Clip<Sample, Accu, Format>, string> ConvertTo<Sample, Accu, Format>(
        uint newSampleRate,
        byte newChannels
    )
        where Format : IAudioFormat<Sample, Accu>
        where Sample : struct
        where Accu : struct;
}
