namespace DIKUArcade.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Result;
using SDLAudio;
using SDLAudio.Device;
using SDLAudio.Sound;

public class SoundPlayer : IDisposable {
    private readonly SDLAudio sdlAudio;
    private readonly IAudioDevice device;

    public bool Invalid => device.Stopped;

    private readonly Dictionary<string, ISound> clips;

    internal SoundPlayer(SDLAudio sdlAudio, IAudioDevice device) {
        clips = new();
        this.sdlAudio = sdlAudio;
        this.device = device;
    }

    public Result<IPlayingSound, string> PlayProcedural(
        SoundGenerator generator,
        Func<float, bool> isDone
    ) => device.PlaySound(new ProceduralSound(generator, isDone, device.SampleRate, 1));

    public Result<IPlayingSound, string> PlayLooping(
        string manifestResourceName,
        float volume = 1f
    ) => GetClip(manifestResourceName, Assembly.GetCallingAssembly())
        .AndThen(device.Looping)
        .AndThen(looping => device.PlaySound(looping, volume));

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
