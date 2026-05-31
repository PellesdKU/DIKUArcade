namespace SDLAudio.Internal;

using System;

internal delegate void AudioCallback(Span<byte> stream);

// We need to give SDL the callback when opening the device,
// but the Device object has not been constructed at that
// point, so we construct an object of this class and give
// SDL its Listener() method. We then then connect the
// Callback to Device.Callback() in the device constructor.
internal class AudioCallbackProxy {
    public AudioCallback? Callback = null;

    public void Listener(IntPtr _, IntPtr stream, int len) {
        if (Callback is AudioCallback cb) {
            unsafe {
                Span<byte> span = new((void*)stream, len);
                cb(span);
            }
        }
    }
}
