namespace SDLAudio.Device;

using System;
using Result;
using Sound;

public interface IAudioDevice : IDisposable {
    bool Stopped { get; }
    bool Paused { get; }

    uint SampleRate { get; }
    byte Channels { get; }

    Result<IPlayingSound, string> PlaySound(ISound sound, float volume = 1f);

    Result<ISound, string> Convert(ISound sound);
    Result<ISound, string> Looping(ISound src);

    void Lock();
    void Unlock();
    void Pause(bool pause = true);

    /// <summary>
    /// Set the audio engine master volume. Clamps between 0 and 1.
    /// </summary>
    /// <param name="vol">The new volume</param>
    void SetVolume(float volume);
}
