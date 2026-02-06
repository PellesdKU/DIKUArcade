namespace DIKUArcade.Audio;

using System;
using System.Runtime.InteropServices;
using DIKUArcade.Audio.Internal;

public class Sound {
    public IntPtr AudioData { get; private set; }
    public UInt32 Length { get; private set; }
    private SDL_AudioSpec spec;

    /// <summary>
    /// Loads sound from a WAV file
    /// </summary>
    /// <param name="filename">The filename of the WAV file.</param>
    /// <returns>A <see cref="Sound"/> object containing the WAV file data, or null if it couldn't be loaded.</returns>
    public static Sound? FromWAV(string filename) {
        IntPtr soundFile = SDLAudioInternal.SDL_RWFromFile(filename, "r");

        SDL_AudioSpec spec = default;
        UInt32 len = default;
        IntPtr wavData = default;
        if(SDLAudioInternal.SDL_LoadWAV_RW(soundFile, 0, ref spec, ref wavData, ref len) == 0) {
            return null;
        }

        // copy the sound data into a new buffer so the wav buffer can be freed
        IntPtr soundData = Marshal.AllocCoTaskMem((int) len);

        // FIXME: surely there is a simple, direct way to copy from one unmanaged buffer to another???
        byte[] temp = new byte[len];
        Marshal.Copy(wavData, temp, 0, (int) len);
        Marshal.Copy(temp, 0, soundData, (int) len);

        SDLAudioInternal.SDL_FreeWAV(wavData);

        return new Sound(spec, soundData, len);
    }

    private Sound(SDL_AudioSpec spec, IntPtr audioData, UInt32 len) {
        this.spec = spec;
        AudioData = audioData;
        Length = len;
    }

    /// <summary>
    /// Convert the sound to another format.
    /// </summary>
    /// <param name="targetSpec">The audio spec describing the target format of the conversion.</param>
    /// <returns>Whether the conversion was successful.</returns>
    public bool Convert(SDL_AudioSpec targetSpec) {
        SDL_AudioCVT cvt = default;
        int res = SDLAudioInternal.SDL_BuildAudioCVT(ref cvt, spec.Format, spec.Channels, spec.Freq, targetSpec.Format, targetSpec.Channels, targetSpec.Freq);
        if (res == 0) { return true; } // no conversion needed
        else if (res < 0) { return false; } // error

        cvt.Len = (int) Length;

        int cvtBufLen = cvt.Len*cvt.Len_mult;
        cvt.Buf = Marshal.AllocCoTaskMem(cvtBufLen);

        // FIXME: surely there is a simple, direct way to copy from one unmanaged buffer to another???
        byte[] temp = new byte[Length];
        Marshal.Copy(AudioData, temp, 0, cvt.Len);
        Marshal.Copy(temp, 0, cvt.Buf, cvt.Len);

        res = SDLAudioInternal.SDL_ConvertAudio(ref cvt);
        if (res != 0) { return false; } // error

        // free old audio data and use the converted buffer as the new
        Marshal.FreeCoTaskMem(AudioData);

        AudioData = cvt.Buf;
        Length = cvt.Len_cvt < 0 ? 0 : (uint) cvt.Len_cvt;
        spec = targetSpec;

        return true;
    }

    ~Sound() {
        if (AudioData == 0) { return; }
        Marshal.FreeCoTaskMem(AudioData);
    }
}
