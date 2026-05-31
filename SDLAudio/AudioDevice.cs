namespace SDLAudio;

using System;
using System.Collections.Generic;
using Sound;
using Internal;
using Result;

/// <summary>
/// An open audio device
/// </summary>
public class AudioDevice : IDisposable {
    internal readonly SDLAudio sdlAudio;
    internal readonly SDLAudioDeviceID id;

    internal AudioSpec Spec { get; private set; }

    private float masterVolume = 1;
    private readonly List<PlayingSound> playing;

    public bool Stopped => sdlAudio.GetAudioDeviceStatus(this) == SDLAudioStatus.STOPPED;
    public bool Paused => sdlAudio.GetAudioDeviceStatus(this) == SDLAudioStatus.PAUSED;

    private void Callback(Span<byte> stream) {
        stream.Clear();

        if (playing.Count == 0) { return; }

        foreach (PlayingSound playingSound in playing) {
            ReadOnlySpan<byte> span = playingSound.PlaySamples((uint)stream.Length);

            if (span.Length == 0 || playingSound.Paused()) { continue; }

            // this is not a recommended way to use this function, but it seems to work fine, at least
            // when there are just a few sounds. I don't want to write custom mixing logic for all the
            // different formats...
            unsafe {
                fixed (byte* bufPtr = &span[0], streamPtr = &stream[0]) {
                    sdlAudio.MixAudioFormat(
                        (IntPtr)streamPtr,
                        (IntPtr)bufPtr,
                        Spec.Format,
                        (uint)span.Length,
                        playingSound.Volume*masterVolume
                    );
                }
            }
        }

        // foreach (PlayingSound loopingSound in loopingSounds) {
        //     if (loopingSound.paused) { continue; }
        //     byte[] patternBuf = new byte[len];

        //     uint patternHead = 0;
        //     while (patternHead < len) {
        //         uint patternBufRemaining = (uint) (len - patternHead);

        //         uint toMix = Math.Min(loopingSound.Remaining, patternBufRemaining);
        //         if (toMix == 0) { continue; }

        //         unsafe {
        //             fixed (byte* bufPtr = &loopingSound.Sound.AudioData[0]) {
        //                 SDLInternal.MixAudioFormat(
        //                     stream,
        //                     (IntPtr) (bufPtr + loopingSound.playhead),
        //                     Spec.Format,
        //                     toMix,
        //                     (int)(loopingSound.volume*masterVolume*128)
        //                 );
        //             }
        //         }

        //         loopingSound.playhead = (loopingSound.playhead + toMix) % loopingSound.Sound.Length;
        //         patternHead += toMix;
        //     }
        // }
    }

    public Result<PlayingSound, string> PlaySound(
        SoundClip clip,
        float volume = 1f
    ) => ConvertClip(clip)
            .AndThen<PlayingSound>(converted => {
                if (Stopped) { return new("Device stopped."); }

                PlayingSound playingSound = new(this, converted, volume, false);

                Lock();
                playing.Add(playingSound);
                playing.RemoveAll(p => p.Done);
                Unlock();

                return new(playingSound);
            });

    /// <summary>
    /// Convert a sound clip to the format of this device.
    /// </summary>
    /// <param name="sound">The sound clip to convert</param>
    /// <returns>Converted sound clip, or an error</returns>
    public Result<SoundClip, string> ConvertClip(SoundClip sound) => sound.Convert(sdlAudio, Spec);

    internal AudioDevice(
        SDLAudio sdlAudio,
        SDLAudioDeviceID deviceId,
        AudioCallbackProxy proxy,
        AudioSpec spec
    ) {
        playing = new();
        this.sdlAudio = sdlAudio;
        id = deviceId;
        proxy.Callback = Callback;
        Spec = spec;

        Pause(false);
    }

    ~AudioDevice() => Dispose();

    public void Lock() => sdlAudio.LockAudioDevice(this);
    public void Unlock() => sdlAudio.UnlockAudioDevice(this);

    public void Pause(bool pause = true) => sdlAudio.PauseAudioDevice(this, pause);

    /// <summary>
    /// Set the audio engine master volume. Clamps between 0 and 1.
    /// </summary>
    /// <param name="vol">The new volume</param>
    public void SetVolume(float vol) {
        Lock();
        masterVolume = Math.Clamp(vol, 0f, 1f);
        Unlock();
    }

    public void Dispose() {
        sdlAudio.CloseAudioDevice(this);
        GC.SuppressFinalize(this);
    }
}
