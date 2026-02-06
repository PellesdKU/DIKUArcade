namespace DIKUArcade.Audio.Internal;

using System.Runtime.InteropServices.Marshalling;

/// <summary>
/// SDL somtimes returns strings that we shouldn't (and can't) free.
/// This marshaller just uses the default UTF8 string marshaller, but doesn't free the raw string after conversion.
/// </summary>
[CustomMarshaller(typeof(string), MarshalMode.UnmanagedToManagedIn, typeof(NoFreeUTF8Marshaller))]
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(NoFreeUTF8Marshaller))]
internal static unsafe class NoFreeUTF8Marshaller {
    public static string? ConvertToManaged(byte* unmanaged)
        => Utf8StringMarshaller.ConvertToManaged(unmanaged);
}