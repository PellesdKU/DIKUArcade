namespace SDLAudio.Device;

using System;
using Result;
using Sound;
using Sound.Clip;

public interface IAudioDevice : IDisposable {
    bool Stopped { get; }
    bool Paused { get; }

    uint SampleRate { get; }
    byte Channels { get; }

    Result<IPlayingSound, string> PlaySound(ISound sound, float volume = 1f);
    Result<IPlayingSound, string> PlayClip(IClip clip, float volume);

    Result<IClip, string> ConvertClip(IClip clip);
    Result<ISound, string> Looping(IClip src);

    void Lock();
    void Unlock();
    void Pause(bool pause = true);

    /// <summary>
    /// Set the audio engine master volume. Clamps between 0 and 1.
    /// </summary>
    /// <param name="vol">The new volume</param>
    void SetVolume(float volume);
}
