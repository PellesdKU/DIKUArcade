namespace SDLAudio.Internal;

using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(bool), MarshalMode.ManagedToUnmanagedIn, typeof(BoolToIntMarshaller))]
internal static class BoolToIntMarshaller {
    public static int ConvertToUnmanaged(bool managed) => managed ? 1 : 0;
}
