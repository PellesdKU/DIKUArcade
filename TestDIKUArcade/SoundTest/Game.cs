namespace TestDIKUArcade.SoundTest;

using System;
using System.Linq;
using DIKUArcade;
using DIKUArcade.Audio;
using DIKUArcade.GUI;
using DIKUArcade.Input;
using SDLAudio.Device;
using SDLAudio.Effects;
using SDLAudio.Sounds;

public class Game : DIKUGame {
    private SoundPlayer soundPlayer;
    private readonly AudioManager manager;
    private PlayingSound? loopingSound = null;

    public Game(WindowArgs windowArgs, AudioManager manager, SoundPlayer soundPlayer)
    : base(windowArgs) {
        this.soundPlayer = soundPlayer;
        this.manager = manager;

        PlayContinous();
    }

    private void PlayLooping() {
        soundPlayer.GetClip("TestDIKUArcade.Assets.block.wav")
            .AndThen(clip => soundPlayer.PlaySound(clip, _ => PlayLooping()))
            .DoOrElse(sound => loopingSound = sound,
                err => Console.WriteLine("Couldn't play sound: {0}", err)
            );
    }

    private void PlayContinous() {
        soundPlayer.PlaySound(new EffectSound(new ProceduralSound(
            (t,c) => (c+1)*0.025f*float.Sin(t*440f*(c+1)*2f*float.Pi),
            _ => false
        )).AddEffect(new LinearFadeEffect(0f, 1f, 0f, 5f)), _ => {}
        ).DoIfOk(
            sound => sound.Pause(false)
        );

        PlayLooping();
    }

    public override void KeyHandler(KeyboardAction action, KeyboardKey key) {
        if (action == KeyboardAction.KeyRelease) { return; }

        if (key == KeyboardKey.Space) {
            loopingSound?.Pause(!loopingSound.Paused);
            return;
        }

        soundPlayer.GetClip("TestDIKUArcade.Assets.bounce.wav")
            .AndThen(clip => soundPlayer.PlaySound(
                new EffectSound(clip)
                    .AddEffect(new PanEffect(0, 0.9f)),
                _ => {}
            ))
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
