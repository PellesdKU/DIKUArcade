namespace DIKUArcade.Timers;

using System;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// A class for scheduling updates and renders in a game.
///
/// Updates are scheduled on a fixed frequency, by always calculating the next time based on the previous.
/// As such, even if the actual update happens a little bit into one period, it doesn't affect the next one.
///
/// If rendering falls more than one frame behind, the missed frames are simply skipped. By contrast,
/// updates will not be skipped, and will be executed as fast as possible until they catch up to the time.
///
/// </summary>
public class GameTimer {
    private TimeSpan nextUpdate;
    private TimeSpan nextRender;
    private TimeSpan nextReset;
    private TimeSpan updatePeriod;
    private TimeSpan renderPeriod;
    private TimeSpan resetPeriod;

    /// <summary>
    /// Get the last observed UPS count
    /// </summary>
    public int CapturedUpdates {
        get; private set;
    }
    /// <summary>
    /// Get the last observed FPS count
    /// </summary>
    public int CapturedFrames {
        get; private set;
    }

    private int updates;
    private int frames;

    private bool unlimitedFps;
    private bool doUpdate;

    private Stopwatch stopwatch;

    public GameTimer() : this(30, 30) { }

    public GameTimer(uint ups, uint fps = 0) {
        unlimitedFps = fps == 0;
        doUpdate = ups != 0;

        // 1 TimeSpan tick is 100ns, of which there are 10 million in a second
        updatePeriod = new TimeSpan(doUpdate ? 10_000_000 / ups : 0);
        renderPeriod = new TimeSpan(unlimitedFps ? 0 : 10_000_000 / fps);
        resetPeriod = new TimeSpan(10_000_000); // reset is always once per second

        stopwatch = new Stopwatch();
        stopwatch.Start();

        TimeSpan now = stopwatch.Elapsed;
        (nextUpdate, nextRender, nextReset) = (now + updatePeriod, now + renderPeriod, now + resetPeriod);

        frames = 0;
        updates = 0;
        CapturedFrames = 0;
        CapturedUpdates = 0;
    }

    public bool ShouldUpdate() {
        if (!doUpdate) {
            return false;
        }
        var now = stopwatch.Elapsed;
        var update = now >= nextUpdate;
        if (update) {
            updates++;
            nextUpdate += updatePeriod;
        }
        return update;
    }

    public bool ShouldRender() {
        if (unlimitedFps) {
            frames++;
            return true;
        }

        var now = stopwatch.Elapsed;
        var render = now >= nextRender;
        if (render) {
            frames++;
            // skip missed frames, for example if we are vsync limited or rendering is taking too long
            while (now >= nextRender) {
                nextRender += renderPeriod;
            }
        }
        return render;
    }

    /// <summary>
    /// The timer will reset if 1 second has passed.
    /// This information can be used to update game logic in any way desireable.
    /// </summary>
    public bool ShouldReset() {
        var now = stopwatch.Elapsed;
        var reset = now >= nextReset;
        if (reset) {
            // skip missed resets, as we want this to run as close to every second as possible
            while (now >= nextReset) {
                nextReset += resetPeriod;
            }

            CapturedUpdates = updates;
            CapturedFrames = frames;
            updates = 0;
            frames = 0;
        }
        return reset;
    }

    /// <summary>
    /// Get the shortest of two time spans
    /// </summary>
    /// <returns>The shortest time span</returns>
    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;

    /// <summary>
    /// Sleeps until it's time to perform the next action so the OS can do other things in the meantime.
    /// Might sleep a bit too long (don't we all sometimes), but probably not so much that it's a problem.
    /// The next update times are calculated by the previous time and the period, so the average update
    /// frequency over a period should be accurate.
    /// </summary>
    public void Yield() {
        if (unlimitedFps) {
            return;
        }

        var nextAction = Min(nextRender, nextReset);
        if (doUpdate) {
            nextAction = Min(nextAction, nextUpdate);
        }

        // return immediately if we have pending actions
        if (stopwatch.Elapsed >= nextAction) {
            return;
        }

        var timeDelta = nextAction - stopwatch.Elapsed;
        Thread.Sleep(timeDelta);
    }
}