namespace SDLAudio.Sound;

using System;
using System.IO;
using Internal;
using Result;

/// <summary>
/// A bit of sound that can be played.
/// </summary>
public class SoundClip : ISound {
    /// <summary>
    /// The raw audio data of the sound.
    /// </summary>
    private readonly byte[] audioData;

    /// <summary>
    /// Length of the audio data (in bytes)
    /// </summary>
    public uint Length => (uint)audioData.Length;

    /// <summary>
    /// Specification of the audio data.
    /// </summary>
    internal AudioSpec Spec { get; private set; }

    /// <summary>
    /// Load a sound from a stream of a .wav file.
    /// </summary>
    /// <param name="stream">Stream of a .wav file.</param>
    /// <returns>A Result of a Sound or a loading error.</returns>
    public static Result<SoundClip, string> FromWAV(SDLAudio sdlAudio, Stream stream) {
        // I can't be bothered to implement an SDL_RWops marshaller, so we just load the whole
        // file into memory and create an RWops* with SDL_RWFromMem().
        byte[] fileData = new byte[stream.Length];
        stream.Read(fileData.AsSpan());

        unsafe {
            fixed (void* ptr = fileData) {
                IntPtr memoryRWops = SDLInternal.RWFromMem((IntPtr)ptr, fileData.Length);

                if(sdlAudio.LoadWAV_RW(
                    memoryRWops, true,
                    out AudioSpec spec,
                    out IntPtr wavData,
                    out uint len) == 0) {
                    return new(SDLInternal.GetError());
                }

                byte[] soundData = new byte[len];
                unsafe {
                    var src = new ReadOnlySpan<byte>((void*)wavData, (int)len);
                    var dst = new Span<byte>(soundData);
                    src.CopyTo(dst);
                }

                sdlAudio.FreeWAV(wavData);

                return new(new SoundClip(spec, soundData));
            }
        }
    }

    private SoundClip(AudioSpec spec, byte[] audioData) {
        Spec = spec;
        this.audioData = audioData;
    }

    /// <summary>
    /// Convert the sound to another format.
    /// </summary>
    /// <param name="targetSpec">The audio spec describing the target format of the conversion.</param>
    /// <returns>Whether the conversion was successful.</returns>
    /// <exception cref="Exception" />
    internal Result<SoundClip, string> Convert(SDLAudio sdlAudio, AudioSpec targetSpec) {
        int res = SDLInternal.BuildAudioCVT(
            out SDLAudioCVT converter,
            Spec.Format,
            Spec.Channels,
            Spec.Freq,
            targetSpec.Format,
            targetSpec.Channels,
            targetSpec.Freq
        );

        if (res == 0) {
            return new(this);
        } else if (res < 0) {
            return new(SDLInternal.GetError());
        }

        converter.Len = audioData.Length;

        int cvtBufLen = converter.Len*converter.Len_mult;
        int finalLen = (int)(Length*converter.Len_ratio);
        byte[] buf = new byte[cvtBufLen];
        byte[] finalBuf = new byte[finalLen];

        unsafe {
            fixed (byte* bufPtr = &buf[0]) {
                converter.Buf = (IntPtr)bufPtr;

                var original = audioData.AsSpan();
                var conversionBuffer = new Span<byte>((void*)converter.Buf, converter.Len);
                original.CopyTo(conversionBuffer);

                if (sdlAudio.ConvertAudio(ref converter) != 0) {
                    return new(SDLInternal.GetError());
                }

                var convertedBuffer = new Span<byte>((void*)converter.Buf, finalLen);
                var newBuffer = finalBuf.AsSpan();
                convertedBuffer.CopyTo(newBuffer);
            }
        }

        return new(new SoundClip(targetSpec, finalBuf));
    }

    public bool IsDone(uint playhead) => playhead >= Length;

    public ReadOnlySpan<byte> GetSamples(uint playhead, uint len) {
        if (playhead + len >= Length) {
            return new();
        }

        return audioData.AsSpan((int)playhead, (int)uint.Min(Length - playhead, len));
    }
}
