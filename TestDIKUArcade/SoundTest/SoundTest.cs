namespace TestDIKUArcade.SoundTest;

using System;
using System.Linq;
using DIKUArcade.Audio;
using DIKUArcade.GUI;

public class SoundTest : ITestable {
    public void RunTest() {
        var windowArgs = new WindowArgs() {
            Title = "SoundTest"
        };

        // This is just a test. Please don't use unwrap in production code!
        var audioManager = AudioManager.Create().Unwrap();

        uint i = 1;
        Console.WriteLine("Please pick an audio device");
        var devices = audioManager.GetPlaybackDevices().ToList();
        Console.WriteLine("0: Default device");
        foreach (var dev in devices) {
            Console.WriteLine($"{i}: {dev}");
            i++;
        }

        var index = Convert.ToInt32(Console.ReadLine());

        var playerRes = index switch {
            0 => audioManager.PlayerWithDefaultDevice(),
            _ => audioManager.PlayerWithDevice(devices[index-1])
        };

        var player = playerRes
            .DoIfErr(err => Console.WriteLine("Couldn't open player: {0}", err))
            .Unwrap();

        var game = new Game(windowArgs, audioManager, player);
        game.Run();
    }

    public void Help() {
        Console.WriteLine("Press any button to play a sound.");
    }
}
