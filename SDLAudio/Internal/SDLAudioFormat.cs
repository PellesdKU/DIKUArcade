namespace SDLAudio.Internal;

using System;
using System.Text;
using Device;
using Format;
using Result;
using Sound;

/// <summary>
/// An audio format. A thin wrapper around SDL_AudioFormat, with some helper functions.
/// </summary>
internal readonly struct SDLAudioFormat {
    public static SDLAudioFormat U8 => new(0x0008);
    public static SDLAudioFormat S8 => new(0x8008);
    public static SDLAudioFormat U16LE => new(0x0010);
    public static SDLAudioFormat S16LE => new(0x8010);
    public static SDLAudioFormat U16BE => new(0x1010);
    public static SDLAudioFormat S16BE => new(0x9010);
    public static SDLAudioFormat S32LE => new(0x8020);
    public static SDLAudioFormat S32BE => new(0x9020);
    public static SDLAudioFormat F32LE => new(0x8120);
    public static SDLAudioFormat F32BE => new(0x9120);

    // https://wiki.libsdl.org/SDL2/SDL_AudioFormat
    private readonly ushort sdlFormat;

    public readonly bool IsSigned => sdlFormat >> 15 == 1;
    public readonly bool IsBigEndian => ((sdlFormat >> 12) & 0b1) == 1;
    public readonly bool IsOppositeEndian => IsBigEndian == BitConverter.IsLittleEndian;
    public readonly bool IsFloat => ((sdlFormat >> 8) & 0b1) == 1;
    public readonly byte SampleSize => (byte)(sdlFormat & 0b11111111);

    public SDLAudioFormat(ushort sdlFormat) {
        this.sdlFormat = sdlFormat;
    }

    public Result<IAudioDevice, string> MakeDevice(
        SDLAudio audio,
        SDLAudioDeviceID id,
        DeviceCallbackProxy proxy,
        uint sampleRate,
        byte channels
    ) => IsFloat switch {
        true => IsBigEndian
            ? new(new Device<float, double, FloatBEFormat>(audio, id, proxy, sampleRate, channels))
            : new(new Device<float, double, FloatLEFormat>(audio, id, proxy, sampleRate, channels)),
        false => IsSigned switch {
            true => SampleSize switch {
                8 => new(new Device<sbyte, short, S8Format>(audio, id, proxy, sampleRate, channels)),
                16 => IsBigEndian
                    ? new(new Device<short, int, S16BEFormat>(audio, id, proxy, sampleRate, channels))
                    : new(new Device<short, int, S16LEFormat>(audio, id, proxy, sampleRate, channels)),
                32 => IsBigEndian
                    ? new(new Device<int, long, S32BEFormat>(audio, id, proxy, sampleRate, channels))
                    : new(new Device<int, long, S32LEFormat>(audio, id, proxy, sampleRate, channels)),
                _ => new($"Unsupported format {this}"),
            },
            false => SampleSize switch {
                8 => new(new Device<byte, ushort, U8Format>(audio, id, proxy, sampleRate, channels)),
                16 => IsBigEndian
                    ? new(new Device<ushort, uint, U16BEFormat>(audio, id, proxy, sampleRate, channels))
                    : new(new Device<ushort, uint, U16LEFormat>(audio, id, proxy, sampleRate, channels)),
                _ => new($"Unsupported format {this}"),
            }
        }
    };

    public Result<ISound, string> MakeClip(
        byte[] audioData,
        uint sampleRate,
        byte channels
    ) => IsFloat switch {
        true => IsBigEndian
            ? new(new Clip<float, double, FloatBEFormat>(audioData, sampleRate, channels))
            : new(new Clip<float, double, FloatLEFormat>(audioData, sampleRate, channels)),
        false => IsSigned switch {
            true => SampleSize switch {
                8 => new(new Clip<sbyte, short, S8Format>(audioData, sampleRate, channels)),
                16 => IsBigEndian
                    ? new(new Clip<short, int, S16BEFormat>(audioData, sampleRate, channels))
                    : new(new Clip<short, int, S16LEFormat>(audioData, sampleRate, channels)),
                32 => IsBigEndian
                    ? new(new Clip<int, long, S32BEFormat>(audioData, sampleRate, channels))
                    : new(new Clip<int, long, S32LEFormat>(audioData, sampleRate, channels)),
                _ => new($"Unsupported format {this}"),
            },
            false => SampleSize switch {
                8 => new(new Clip<byte, ushort, U8Format>(audioData, sampleRate, channels)),
                16 => IsBigEndian
                    ? new(new Clip<ushort, uint, U16BEFormat>(audioData, sampleRate, channels))
                    : new(new Clip<ushort, uint, U16LEFormat>(audioData, sampleRate, channels)),
                _ => new($"Unsupported format {this}"),
            }
        }
    };

    public override readonly string ToString() => new StringBuilder()
        .Append(IsSigned ? "signed " : "unsigned ")
        .Append(IsBigEndian ? "big endian " : "little endian ")
        .Append(SampleSize).Append(" bit ")
        .Append(IsFloat ? "floating point" : "integer")
        .ToString();
}
