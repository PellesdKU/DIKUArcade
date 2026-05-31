namespace SDLAudio.Internal;

internal struct SDLAudioDeviceID {
#pragma warning disable CS0649
    public ushort DeviceID;
#pragma warning restore CS0649

    public readonly bool IsInvalid => DeviceID == 0;
}
