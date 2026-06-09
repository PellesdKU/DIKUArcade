namespace SDLAudio.Device;

public interface IPlayingSound {
    public bool Paused { get; }
    public float Volume { get; }
    void SetVolume(float volume);
    void Pause(bool paused);
    void Cancel();
}
