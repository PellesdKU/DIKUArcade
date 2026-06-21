namespace DIKUArcade.Audio;

using System.Collections.Generic;
using Result;
using SDLAudio;

/// <summary>
/// A class for managing audio. It makes sure that the SDL audio subsystem is initalized before
/// letting you use any of the related functions.
/// </summary>
public class AudioManager {
    private readonly SDLAudio sdlAudio;

    /// <summary>
    /// Refresh the list of available playback devices
    /// </summary>
    public void RefreshPlaybackDevices() => sdlAudio.RefreshPlaybackDevices();

    /// <summary>
    /// Get the list of available playback devices. Note that this is not updated until
    /// <see cref="RefreshPlaybackDevices" /> has been called, even if new devices are plugged in.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetPlaybackDevices() => sdlAudio.GetPlaybackDevices();

    /// <summary>
    /// Get the name of the default audio device. Note that this is not necessarily the same
    /// device as will be used by <see cref="PlayerWithDefaultDevice" />.
    /// </summary>
    /// <returns>Name of the default device, or an error message.</returns>
    public Result<string, string> GetDefaultDevice() => sdlAudio.GetDefaultDevice();

    /// <summary>
    /// Open a sound player with the default device. This is probably the function you want to use
    /// over <see cref="PlayerWithDevice" />, as it should automatically switch when the default
    /// device changes (for example if a device is plugged in/unplugged or the default is switched
    /// by the user).
    /// </summary>
    /// <returns>A sound player with the default device, or an error message.</returns>
    public Result<SoundPlayer, string> PlayerWithDefaultDevice()
        => sdlAudio.OpenDefaultPlaybackDevice().MapOk(dev => new SoundPlayer(sdlAudio, dev));

    /// <summary>
    /// Open a sound player with a specific device. Must be one of the devices returned by
    /// <see cref="GetPlaybackDevices" /> or <see cref="GetDefaultDevice" />. Note that you most
    /// likely want to use <see cref="PlayerWithDefaultDevice" /> instead, unless you really
    /// need to use a specific device.
    /// </summary>
    /// <param name="device">Name of the device to use</param>
    /// <returns>A sound player with the specified device, or an error message.</returns>
    public Result<SoundPlayer, string> PlayerWithDevice(string device)
        => sdlAudio.OpenPlaybackDevice(device).MapOk(dev => new SoundPlayer(sdlAudio, dev));

    private AudioManager(SDLAudio sdlAudio) {
        this.sdlAudio = sdlAudio;
    }

    /// <summary>
    /// Create an audio
    /// </summary>
    /// <returns></returns>
    public static Result<AudioManager, string> Create() => SDLAudio.Create().MapOk(
        sdlAudio => new AudioManager(sdlAudio)
    );
}
