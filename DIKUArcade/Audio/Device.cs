namespace DIKUArcade.Audio;

using System;
using System.Runtime.InteropServices.Marshalling;
using DIKUArcade.Audio.Internal;

public class Device {
    public string Name { get; protected set; }
    public SDL_AudioSpec Spec { get; protected set; }

    /// <summary>
    /// Tries to get the default audio device.
    /// </summary>
    /// <returns>The default audio device, or null if one doesn't exist</returns>
    public static Device? DefaultDevice() {
        IntPtr nameBuf = default;
        SDL_AudioSpec spec = default;

        int res = SDLAudioInternal.SDL_GetDefaultAudioInfo(ref nameBuf, ref spec, 0);
        if (res != 0) {
            Console.WriteLine(SDLAudioInternal.SDL_GetError());
            return null;
        }

        string name = "";
        unsafe {
            if (nameBuf != 0) {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                name = Utf8StringMarshaller.ConvertToManaged((byte*) nameBuf);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
        }

        return new Device(name, spec);
    }

    /// <summary>
    /// Get all audio devices.
    /// </summary>
    /// <returns>An array of all devices. Note that a device may be null if its spec couldn't be obtained.</returns>
    public static Device?[] GetDevices() {
        int nDevices = SDLAudioInternal.SDL_GetNumAudioDevices(0);
        Device?[] devices = new Device[nDevices];

        for (int i = 0; i < devices.Length; i++) {
            string devName = SDLAudioInternal.SDL_GetAudioDeviceName(i, 0);
            SDL_AudioSpec spec = default;

            int res = SDLAudioInternal.SDL_GetAudioDeviceSpec(i, 0, ref spec);
            if (res != 0) {
                devices[i] = null;
                continue;
            }

            devices[i] = new Device(devName, spec);
        }

        return devices;
    }

    private Device(string deviceName, SDL_AudioSpec spec) {
        Name = deviceName;
        Spec = spec;
    }
}
