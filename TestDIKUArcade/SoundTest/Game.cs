namespace TestDIKUArcade.SoundTest;

using System;
using System.Linq;
using DIKUArcade;
using DIKUArcade.Audio;
using DIKUArcade.GUI;
using DIKUArcade.Input;

public class Game : DIKUGame {
    private SoundPlayer soundPlayer;
    private readonly AudioManager manager;

    public Game(WindowArgs windowArgs, AudioManager manager, SoundPlayer soundPlayer) : base(windowArgs) {
        this.soundPlayer = soundPlayer;
        this.manager = manager;
    }

    public override void KeyHandler(KeyboardAction action, KeyboardKey key) {
        if (action == KeyboardAction.KeyRelease) { return; }

        soundPlayer.PlaySound("TestDIKUArcade.Assets.bounce.wav")
            .DoIfErr(err => Console.WriteLine("Couldn't play sound: {0}", err));
    }

    public override void Render(WindowContext context) {}

    public override void Update() {
        if (soundPlayer.Invalid) {
            Console.WriteLine("Reinitializing sound player.");
            manager.RefreshPlaybackDevices();
            var devices = manager.GetPlaybackDevices();

            if (devices.Any()) {
                manager.PlayerWithDevice(devices.Last())
                    .DoOrElse(
                        player => soundPlayer = player,
                        err => Console.WriteLine("Couldn't reinitialize sound player: {0}", err)
                    );
            }
        }
    }
}
