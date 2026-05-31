namespace DIKUArcade.Audio;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Result;
using SDLAudio;
using SDLAudio.Sound;

public class SoundPlayer : IDisposable {
    private readonly SDLAudio sdlAudio;
    private readonly AudioDevice device;

    public bool Invalid => device.Stopped;

    private readonly Dictionary<string, SoundClip> sounds;

    internal SoundPlayer(SDLAudio sdlAudio, AudioDevice device) {
        sounds = new();
        this.sdlAudio = sdlAudio;
        this.device = device;
    }

    public Result<PlayingSound, string> PlaySound(
        string manifestResourceName,
        float volume = 1f
    ) => GetClip(manifestResourceName, Assembly.GetCallingAssembly())
            .AndThen(sound => device.PlaySound(sound, volume));

    private Result<SoundClip, string> GetClip(string manifestResourceName, Assembly assembly) {
        if (sounds.TryGetValue(manifestResourceName, out SoundClip? value)) {
            return new(value);
        }

        try {
            using Stream stream = assembly.GetManifestResourceStream(manifestResourceName)
            ?? throw new Exception($"{manifestResourceName} not found. " +
                                    "Make sure that the file is being embedded");

            return SoundClip.FromWAV(sdlAudio, stream)
                .AndThen(device.ConvertClip)
                .DoIfOk(sound => sounds[manifestResourceName] = sound);
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
