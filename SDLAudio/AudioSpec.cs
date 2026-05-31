namespace SDLAudio;

using System.Text;

/// <summary>
/// An SDL_AudioSpec <see href="https://wiki.libsdl.org/SDL2/SDL_AudioSpec"/>
/// </summary>
internal struct AudioSpec {
    public int Freq;
    public AudioFormat Format;
    public byte Channels;
    public byte Silence;
    public ushort Samples;
    public uint Size;
    public SDLAudioCallback? Callback;

    public override readonly string ToString() => new StringBuilder()
        .Append(Format.ToString()).Append(' ')
        .Append(Freq).Append(" Hz ")
        .Append(Channels).Append(" channel(s) ")
        .Append(Samples).Append(" sample(s) ")
        .Append(Size).Append(" byte(s) buffer ")
        .ToString();
}
