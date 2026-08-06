namespace DIKUArcade.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Result;
using SDLAudio;
using SDLAudio.Device;
using SDLAudio.Sounds;

/// <summary>
/// Class representing an open audio device on which sound can be played. Audio clips can be played
/// from .wav files embedded in the assembly. The clips are converted to the native format of the
/// audio device and cached.
/// You don't need to save the IPlayingSound's returned by the various methods unless you need
/// to adjust the volume, cancel, etc.
/// </summary>
public class SoundPlayer : IDisposable {
    private readonly SDLAudio sdlAudio;
    private readonly AudioDevice device;

    /// <summary>
    /// Determine whether the sound player is still valid. This may not be the case if for example
    /// the device was unplugged. When a sound player becomes invalid it should be discarded and a
    /// new one should be openened.
    /// </summary>
    public bool Invalid => device.Stopped;

    private readonly Dictionary<string, Clip> clips;

    internal SoundPlayer(SDLAudio sdlAudio, AudioDevice device) {
        clips = new();
        this.sdlAudio = sdlAudio;
        this.device = device;
    }

    /// <summary>
    /// Play a procedurally generated sound.
    /// </summary>
    /// <param name="generator">Function from the time to the generated sample.</param>
    /// <param name="isDone">Function from time to a bool indicating whether the sound is done.</param>
    /// <returns>An IPlayingSound representing the procedurally generated sound.</returns>
    public Result<PlayingSound, string> PlaySound(
        ISound sound,
        SoundEventHandler onDone,
        float volume = 1f
    ) => device.PlaySound(sound, onDone, volume);

    /// <summary>
    /// Play a clip
    /// </summary>
    /// <param name="manifestResourceName"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public Result<PlayingSound, string> PlayClip(
        string manifestResourceName,
        SoundEventHandler onDone,
        float volume = 1f
    ) => GetClip(manifestResourceName, Assembly.GetCallingAssembly())
        .AndThen(sound => device.PlaySound(sound, onDone, volume));

    public Result<Clip, string> GetClip(string manifestResourceName) =>
        GetClip(manifestResourceName, Assembly.GetCallingAssembly());

    private Result<Clip, string> GetClip(string manifestResourceName, Assembly assembly) {
        if (clips.TryGetValue(manifestResourceName, out Clip? value)) {
            return new(value);
        }

        try {
            using Stream stream = assembly.GetManifestResourceStream(manifestResourceName)
            ?? throw new Exception($"{manifestResourceName} not found. " +
                                    "Make sure that the file is being embedded");

            return sdlAudio.LoadWAV(sdlAudio, stream, device.SampleRate, device.Channels)
                .DoIfOk(sound => clips[manifestResourceName] = sound);
        } catch (Exception err) {
            return new(err.ToString());
        }
    }

    ~SoundPlayer() {
        Dispose();
    }

    public void Dispose() {
        device.Dispose();
        GC.SuppressFinalize(this);
    }
}
