namespace DIKUArcade.Audio;

using System.Collections.Generic;
using Result;
using SDLAudio;

public class AudioManager {
    private readonly SDLAudio sdlAudio;

    public void RefreshPlaybackDevices() => sdlAudio.RefreshPlaybackDevices();

    public IEnumerable<string> GetPlaybackDevices() => sdlAudio.GetPlaybackDevices();

    public Result<string, string> GetDefaultDevice() => sdlAudio.GetDefaultDevice();

    public Result<SoundPlayer, string> PlayerWithDefaultDevice()
        => sdlAudio.OpenDefaultPlaybackDevice().MapOk(dev => new SoundPlayer(sdlAudio, dev));

    public Result<SoundPlayer, string> PlayerWithDevice(string device)
        => sdlAudio.OpenPlaybackDevice(device).MapOk(dev => new SoundPlayer(sdlAudio, dev));

    private AudioManager(SDLAudio sdlAudio) {
        this.sdlAudio = sdlAudio;
    }

    public static Result<AudioManager, string> Create() => SDLAudio.Create().MapOk(
        sdlAudio => new AudioManager(sdlAudio)
    );
}
