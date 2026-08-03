namespace SDLAudio.Device;

using System;

internal delegate void DeviceCallback(Span<float> stream);

// We need to give SDL the callback when opening the device,
// but the Device object has not been constructed at that
// point, so we construct an object of this class and give
// SDL its Listener() method. We then then connect the
// Callback to Device.Callback() in the device constructor.
internal class DeviceCallbackProxy {
    public readonly SDLAudioCallback Listener;

    public DeviceCallbackProxy() {
        Listener = Listen;
    }

    public DeviceCallback? Callback = null;

    private void Listen(IntPtr _, IntPtr stream, int len) {
        if (Callback is DeviceCallback cb) {
            unsafe {
                Span<float> span = new((void*)stream, len/sizeof(float));
                cb(span);
            }
        }
    }
}
