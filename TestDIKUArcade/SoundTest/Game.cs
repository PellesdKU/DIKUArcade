namespace TestDIKUArcade.SoundTest;

using System;
using System.Linq;
using DIKUArcade;
using DIKUArcade.Audio;
using DIKUArcade.GUI;
using DIKUArcade.Input;
using SDLAudio.Device;

public class Game : DIKUGame {
    private SoundPlayer soundPlayer;
    private readonly AudioManager manager;
    private IPlayingSound? loopingSound = null;

    public Game(WindowArgs windowArgs, AudioManager manager, SoundPlayer soundPlayer) : base(windowArgs) {
        this.soundPlayer = soundPlayer;
        this.manager = manager;

        PlayContinous();
    }

    private void PlayContinous() {
        soundPlayer.PlayProcedural(
            t => 0.1f*float.Sin(t*440f*2f*float.Pi),
            _ => false
        );

        soundPlayer.PlayLooping("TestDIKUArcade.Assets.block.wav")
            .DoOrElse(
                sound => loopingSound = sound,
                err => Console.WriteLine("Couldn't play sound: {0}", err)
            );
    }

    public override void KeyHandler(KeyboardAction action, KeyboardKey key) {
        if (action == KeyboardAction.KeyRelease) { return; }

        if (key == KeyboardKey.Space) {
            loopingSound?.Pause(!loopingSound.Paused);
            return;
        }

        soundPlayer.PlayClip("TestDIKUArcade.Assets.bounce.wav")
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
                        player => {
                            soundPlayer = player;
                            PlayContinous();
                        },
                        err => Console.WriteLine("Couldn't reinitialize sound player: {0}", err)
                    );
            }
        }
    }
}
