namespace DIKUArcade.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Result;
using SDLAudio;
using SDLAudio.Device;
using SDLAudio.Sound;

/// <summary>
/// Class representing an open audio device on which sound can be played. Audio clips can be played
/// from .wav files embedded in the assembly. The clips are converted to the native format of the
/// audio device and cached.
/// You don't need to save the IPlayingSound's returned by the various methods unless you need
/// to adjust the volume, cancel, etc.
/// </summary>
public class SoundPlayer : IDisposable {
    private readonly SDLAudio sdlAudio;
    private readonly IAudioDevice device;

    /// <summary>
    /// Determine whether the sound player is still valid. This may not be the case if for example
    /// the device was unplugged. When a sound player becomes invalid it should be discarded and a
    /// new one should be openened.
    /// </summary>
    public bool Invalid => device.Stopped;

    private readonly Dictionary<string, ISound> clips;

    internal SoundPlayer(SDLAudio sdlAudio, IAudioDevice device) {
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
    public IPlayingSound PlayProcedural(
        SoundGenerator generator,
        Func<float, bool> isDone
    ) => device.PlaySound(new ProceduralSound(generator, isDone, device.SampleRate, 1))
        // A procedural sound is converted with a ConvertingSound, creation of which cannot fail.
        // Thus, it should be safe to unwrap here.
        .Unwrap();

    /// <summary>
    /// Play a looping clip.
    /// </summary>
    /// <param name="manifestResourceName">Manifest resource name of the sound clip to play.</param>
    /// <param name="volume">Volume to play the clip at (defaults to 1)</param>
    /// <returns>An IPlayingSound representing the clip.</returns>
    public Result<IPlayingSound, string> PlayLooping(
        string manifestResourceName,
        float volume = 1f
    ) => GetClip(manifestResourceName, Assembly.GetCallingAssembly())
        .AndThen(device.Looping)
        .AndThen(looping => device.PlaySound(looping, volume));

    /// <summary>
    /// Play a clip
    /// </summary>
    /// <param name="manifestResourceName"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public Result<IPlayingSound, string> PlayClip(
        string manifestResourceName,
        float volume = 1f
    ) => GetClip(manifestResourceName, Assembly.GetCallingAssembly())
        .AndThen(sound => device.PlaySound(sound, volume));

    private Result<ISound, string> GetClip(string manifestResourceName, Assembly assembly) {
        if (clips.TryGetValue(manifestResourceName, out ISound? value)) {
            return new(value);
        }

        try {
            using Stream stream = assembly.GetManifestResourceStream(manifestResourceName)
            ?? throw new Exception($"{manifestResourceName} not found. " +
                                    "Make sure that the file is being embedded");

            return sdlAudio.LoadWAV(sdlAudio, stream)
                .AndThen(device.Convert)
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
