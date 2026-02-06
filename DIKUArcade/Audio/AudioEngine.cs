namespace DIKUArcade.Audio;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Internal;

/// Roadmap:
/// - Multiple channels with independent volumes
/// - Procedural sound
/// - Support for more file formats

/// <summary>
/// A simple audio engine that can play "one-shot" sounds, loop sounds, pause and cancel.
/// It works on an asynchronous callback, so doesn't need to be updated explicitly.
/// </summary>
public class AudioEngine {
    private bool valid;
    private ushort devId;
    private SDL_AudioSpec spec;
    private float masterVolume;
    private List<PlayingSound> playingSounds;
    private List<PlayingSound> loopingSounds;
    static bool sdlInitialized = false;

    /// <summary>
    /// A helper class representing a sound that is being played.
    /// </summary>
    public class PlayingSound {
        public Sound Sound { get; private set; }
        public uint Playhead;
        public float Volume;
        public bool Paused;
        public bool Looping;

        /// <summary>
        /// Initialize a new instance of the <see cref="PlayingSound"/> class.
        /// </summary>
        /// <param name="sound">The sound being played</param>
        /// <param name="volume">A value between 0 and 1 (clamped) representing the volume at which to play the sound</param>
        /// <param name="paused">Whether the sound is paused</param>
        /// <param name="looping">Whether the sound is looping</param>
        public PlayingSound(Sound sound, float volume, bool paused, bool looping) {
            Sound = sound;
            Volume = Math.Clamp(volume, 0f, 1f);
            Playhead = 0;
            Paused = paused;
            Looping = looping;
        }
    }

    // This functions is called at a certain frequency by SDL, and its job is to fill the provided buffer
    // with audio data.
    private void Callback(IntPtr userdata, IntPtr stream, int len) {
        byte[] zeros = new byte[len];
        Marshal.Copy(zeros, 0, stream, len);

        foreach (PlayingSound playingSound in playingSounds) {
            uint remaining = playingSound.Sound.Length - playingSound.Playhead;
            uint bytesToPlay = Math.Min(remaining, (uint) len);

            if (bytesToPlay == 0 || playingSound.Paused) { continue; }

            // this is not a recommended way to use this function, but it seems to work fine, at least
            // when there are just a few sounds. I don't want to write custom mixing logic for all the
            // different formats...
            SDLAudioInternal.SDL_MixAudioFormat(
                stream,
                (IntPtr) (playingSound.Sound.AudioData + playingSound.Playhead),
                spec.Format,
                bytesToPlay,
                (int) (playingSound.Volume*masterVolume*128)
            );

            playingSound.Playhead += bytesToPlay;
        }

        foreach (PlayingSound loopingSound in loopingSounds) {
            if (loopingSound.Paused) { continue; }
            byte[] patternBuf = new byte[len];

            uint patternHead = 0;
            while (patternHead < len) {
                uint soundRemaining = loopingSound.Sound.Length - loopingSound.Playhead;
                uint patternBufRemaining = (uint) (len - patternHead);

                uint toMix = Math.Min(soundRemaining, patternBufRemaining);

                SDLAudioInternal.SDL_MixAudioFormat(
                    stream,
                    (IntPtr) (loopingSound.Sound.AudioData + loopingSound.Playhead),
                    spec.Format,
                    toMix,
                    (int) (loopingSound.Volume*masterVolume*128)
                );

                loopingSound.Playhead = (loopingSound.Playhead + toMix) % loopingSound.Sound.Length;
                patternHead += toMix;
            }
            if (patternHead > len) {
                Console.WriteLine("Uhhhh");
            }
        }

        // this may be too heavy to do here. Probably okay.
        playingSounds.RemoveAll(playingSound => playingSound.Playhead >= playingSound.Sound.Length);
    }

    /// <summary>
    /// Initializes the sound subsystem of SDL. Must be called before most sound-related functions can be used.
    /// </summary>
    /// <returns>Whether the sound subsystem was succesfully initialized or not</returns>
    public static bool Initialize() {
        int res = SDLAudioInternal.SDL_InitSubSystem(SDLAudioInternal.SDL_INIT_AUDIO);
        sdlInitialized = res == 0;
        return sdlInitialized;
    }

    /// <summary>
    /// Make a new <see cref="AudioEngine"/> outputting to the given device.
    /// </summary>
    /// <param name="dev">The device to use for outputting sound</param>
    /// <returns>An <see cref="AudioEngine"/>, or null if one couldn't be created</returns>
    public static AudioEngine? MakeAudioEngine(Device dev) {
        if (!sdlInitialized) { return null; }

        AudioEngine engine = new AudioEngine(dev);
        if (!engine.valid) { return null; }
        return engine;
    }

    /// <summary>
    /// Convert a sound to this <see cref="AudioEngine"/>s spec, so it can be played properly
    /// </summary>
    /// <param name="sound">The sound to be converted in place</param>
    /// <returns>Whether the conversion was successful.</returns>
    public bool ConvertSound(Sound sound) => sound.Convert(spec);

    public PlayingSound PlaySound(Sound sound, float volume, bool looping = false) {
        SDLAudioInternal.SDL_LockAudioDevice(devId);

        PlayingSound playingSound = new PlayingSound(sound, volume, false, looping);
        (looping ? loopingSounds : playingSounds).Add(playingSound);

        SDLAudioInternal.SDL_UnlockAudioDevice(devId);

        return playingSound;
    }

    /// <summary>
    /// Cancel a playing sound.
    /// </summary>
    /// <param name="playingSound">The playing sound to cancel</param>
    public void CancelSound(PlayingSound playingSound) {
        SDLAudioInternal.SDL_LockAudioDevice(devId);

        (playingSound.Looping ? loopingSounds : playingSounds).Remove(playingSound);

        SDLAudioInternal.SDL_UnlockAudioDevice(devId);
    }

    /// <summary>
    /// Pause playing all sound.
    /// </summary>
    /// <param name="pause">True to pause, false to unpause. Defaults to true.</param>
    public void Pause(bool pause = true) {
        SDLAudioInternal.SDL_PauseAudioDevice(devId, pause ? 1 : 0);
    }

    /// <summary>
    /// Set the audio engine master volume. Clamps between 0 and 1.
    /// </summary>
    /// <param name="vol">The new volume</param>
    public void SetVolume(float vol) {
        masterVolume = Math.Clamp(vol, 0f, 1f);
    }

    /// <summary>
    /// Get the master volume.
    /// </summary>
    /// <returns>Master volume</returns>
    public float GetVolume() {
        return masterVolume;
    }

    /// <summary>
    /// Check whether the <see cref="AudioEngine"/> is paused.
    /// </summary>
    /// <returns>Whether the <see cref="AudioEngine"/> is paused</returns>
    public bool Paused() => SDLAudioInternal.SDL_GetAudioDeviceStatus(devId) == SDL_AudioStatus.SDL_AUDIO_PAUSED;

    private AudioEngine(Device dev) {
        valid = false;

        masterVolume = 1f;
        playingSounds = new List<PlayingSound>();
        loopingSounds = new List<PlayingSound>();

        SDL_AudioSpec desiredSpec = dev.Spec;
        SDL_AudioSpec obtainedSpec = default;

        desiredSpec.Callback = Callback;

        ushort devId = SDLAudioInternal.SDL_OpenAudioDevice(dev.Name, 0, ref desiredSpec, ref obtainedSpec, SDLAudioInternal.SDL_AUDIO_ALLOW_ANY_CHANGE);
        if (devId <= 0) {
            string err = SDLAudioInternal.SDL_GetError();
            Console.WriteLine($"Couldn't open audio device: {err}");
            return;
        }

        this.devId = devId;
        spec = obtainedSpec;

        SDLAudioInternal.SDL_PauseAudioDevice(devId, 0);
        valid = true;
    }
}