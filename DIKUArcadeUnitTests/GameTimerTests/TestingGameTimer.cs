namespace DIKUArcadeUnitTests.GameTimerTests;

using DIKUArcade.Timers;
using NUnit.Framework;

[TestFixture]
public class TestingGameTimer {

    [Repeat(3)]
    [TestCase(0u)]
    [TestCase(1u)]
    [TestCase(5u)]
    [TestCase(10u)]
    [TestCase(30u)]
    [TestCase(60u)]
    public void TestCapturedUpdates(uint ups) {
        var timer = new GameTimer(ups);
        var updates = 0;

        while (!timer.ShouldReset()) {
            if (timer.ShouldUpdate()) {
                updates++;
            }
            timer.Yield();
        }
        // can we count the number of updates
        Assert.AreEqual(updates, timer.CapturedUpdates);

        // Allow some tolerance for timing imprecision
        // Target is ups, but we might get a bit more or less
        if (ups > 0) {
            Assert.GreaterOrEqual(timer.CapturedUpdates, ups - 1);
        }
        Assert.LessOrEqual(timer.CapturedUpdates, ups + 5);
    }

    [Repeat(3)]
    [TestCase(1u)]
    [TestCase(5u)]
    [TestCase(10u)]
    [TestCase(30u)]
    [TestCase(60u)]
    public void TestCapturedFrames(uint fps) {
        var timer = new GameTimer(30, fps);
        var frames = 0;

        while (!timer.ShouldReset()) {
            if (timer.ShouldRender()) {
                frames++;
            }
            timer.Yield();
        }
        // can we count the number of frames
        Assert.AreEqual(frames, timer.CapturedFrames);

        // Allow some tolerance for timing imprecision
        Assert.GreaterOrEqual(timer.CapturedFrames, fps - 1);
        Assert.LessOrEqual(timer.CapturedFrames, fps + 5);
    }
}
