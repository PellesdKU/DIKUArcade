namespace SDLAudio.Internal;

using System.Text;
using Device;
using Sounds;

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

    public readonly Clip MakeClip(float[] audioData) =>
        new(audioData, (uint)Freq, Channels);

    public readonly AudioDevice MakeDevice(
        SDLAudio audio,
        SDLAudioDeviceID id,
        DeviceCallbackProxy proxy
    ) => new(audio, id, proxy, (uint)Freq, Channels);

    public override readonly string ToString() => new StringBuilder()
        .Append(Format.ToString()).Append(' ')
        .Append(Freq).Append(" Hz ")
        .Append(Channels).Append(" channel(s) ")
        .Append(Samples).Append(" sample(s) ")
        .Append(Size).Append(" byte(s) buffer ")
        .ToString();
}
