namespace SDLAudio.Internal;

internal enum SDLAllowChange : int {
    FREQUENCY = 0x1,
    FORMAT    = 0x2,
    CHANNELS  = 0x4,
    SAMPLES   = 0x8,
    ANY       = FREQUENCY|FORMAT|CHANNELS|SAMPLES
}
