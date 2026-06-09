namespace SDLAudio.Internal;

using System.Text;
using Result;
using Device;
using Sound.Clip;

/// <summary>
/// An SDL_AudioSpec <see href="https://wiki.libsdl.org/SDL2/SDL_AudioSpec"/>
/// </summary>
internal struct SDLAudioSpec {
    public int Freq;
    public SDLAudioFormat Format;
    public byte Channels;
    public byte Silence;
    public ushort Samples;
#pragma warning disable CS0649
    public uint Size;
#pragma warning restore CS0649
    internal SDLAudioCallback? Callback;

    public readonly Result<IClip, string> MakeClip(SDLAudio sdlAudio, byte[] audioData)
        => Format.MakeClip(sdlAudio, audioData, (uint)Freq, Channels);

    public readonly Result<IAudioDevice, string> MakeDevice(
        SDLAudio audio,
        SDLAudioDeviceID id,
        DeviceCallbackProxy proxy
    ) => Format.MakeDevice(audio, id, proxy, (uint)Freq, Channels);

    public override readonly string ToString() => new StringBuilder()
        .Append(Format.ToString()).Append(' ')
        .Append(Freq).Append(" Hz ")
        .Append(Channels).Append(" channel(s) ")
        .Append(Samples).Append(" sample(s) ")
        .Append(Size).Append(" byte(s) buffer ")
        .ToString();
}
