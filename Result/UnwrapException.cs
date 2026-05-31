namespace Result;

using System;

/// <summary>
/// Exception thrown when an Err Result is unwrapped.
/// </summary>
public class UnwrapException : Exception {
    public UnwrapException(string message) : base(message) { }
}
