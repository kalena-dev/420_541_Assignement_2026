using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RunTests
{
    [UnitySetUp]
    public IEnumerator StartFresh()
    {
        GameManager.Instance.StartRun();
        yield return new WaitForSeconds(.8f);
    }

    [UnityTest]
    public IEnumerator ScoreSurvivesScenesAndRestartClearsRun()
    {
        GameManager game = GameManager.Instance;
        game.Powerups.Collect(PickupType.Score);
        Assert.AreEqual(50, game.Score);
        game.Powerups.Collect(PickupType.ScoreBoost);
        game.Powerups.Collect(PickupType.Score);
        Assert.AreEqual(150, game.Score);
        for (int level = 2; level <= 3; level++)
        {
            DisableScoreTriggers();
            game.Player.GetComponent<Rigidbody>().position = Object.FindAnyObjectByType<LevelGoal>().transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(.7f);
            Assert.AreSame(game, GameManager.Instance);
            Assert.AreEqual(level, game.Level.levelNumber);
            Assert.AreEqual(150, game.Score);
        }
        DisableScoreTriggers();
        game.Player.GetComponent<Rigidbody>().position = Object.FindAnyObjectByType<LevelGoal>().transform.position;
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(.7f);
        Assert.IsTrue(game.Finished);
        Assert.AreEqual(150, game.Score);
        game.StartRun();
        yield return new WaitForSeconds(.7f);
        Assert.AreEqual(0, game.Score);
        Assert.AreEqual(1, game.Level.levelNumber);
    }

    static void DisableScoreTriggers()
    {
        // Isolate persistence from extra points collected beside the exit frames.
        foreach (Pickup pickup in Object.FindObjectsByType<Pickup>())
            if (pickup.type == PickupType.Score) pickup.GetComponent<Collider>().enabled = false;
    }

    [UnityTest]
    public IEnumerator JumpWaitsForReleaseAndOnlyOnePoweredFlipIsAllowed()
    {
        CharacterMovement player = GameManager.Instance.Player;
        Rigidbody body = player.GetComponent<Rigidbody>();
        Assert.IsTrue(player.Grounded);
        player.BeginJump();
        yield return new WaitForSeconds(.15f);
        Assert.Less(body.linearVelocity.y, .5f);
        Assert.Greater(player.Charge, 0f);
        player.ReleaseJump();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.Greater(body.linearVelocity.y, 4f);
        int flips = 0;
        player.DoubleJumped += () => flips++;
        yield return new WaitForSeconds(.15f);
        player.BeginJump();
        Assert.AreEqual(0, flips);
        GameManager.Instance.Powerups.Collect(PickupType.Jump);
        player.BeginJump();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(1, flips);
        Assert.Greater(body.linearVelocity.y, 8f);
        yield return new WaitForSeconds(.15f);
        player.BeginJump();
        Assert.AreEqual(1, flips);
    }

    [UnityTest]
    public IEnumerator TrapGracePeriodAndDeathRestoreLevel()
    {
        GameManager game = GameManager.Instance;
        PlayerHealth health = game.Health;
        game.AddScore(50);
        health.Damage();
        health.Damage();
        Assert.AreEqual(2, health.Health);
        yield return new WaitForSeconds(1.15f);
        health.Damage();
        Assert.AreEqual(1, health.Health);
        yield return new WaitForSeconds(1.15f);
        health.Damage();
        yield return new WaitForSeconds(.7f);
        Assert.AreSame(game, GameManager.Instance);
        Assert.AreEqual(3, game.Health.Health);
        Assert.AreEqual(0, game.Score);
    }

    [UnityTest]
    public IEnumerator PauseFreezesClockAndPowerupDuration()
    {
        GameManager game = GameManager.Instance;
        game.Powerups.Collect(PickupType.Speed);
        game.Powerups.Collect(PickupType.Jump);
        game.Powerups.Collect(PickupType.ScoreBoost);
        Assert.That(game.Powerups.SpeedRemaining, Is.InRange(4.9f, 5f));
        Assert.That(game.Powerups.JumpRemaining, Is.InRange(29.9f, 30f));
        Assert.That(game.Powerups.ScoreRemaining, Is.InRange(9.9f, 10f));
        game.SetPaused(true);
        float clock = game.RunTime;
        float buff = game.Powerups.SpeedRemaining;
        yield return new WaitForSecondsRealtime(.25f);
        Assert.AreEqual(clock, game.RunTime, .001f);
        Assert.AreEqual(buff, game.Powerups.SpeedRemaining, .001f);
        game.SetPaused(false);
        yield return new WaitForSeconds(5.1f);
        Assert.IsFalse(game.Powerups.SpeedActive);
        Assert.IsTrue(game.Powerups.DoubleJumpActive);
    }

    [UnityTest]
    public IEnumerator PickupTriggersRespawnAndFallingRestartsLevel()
    {
        GameManager game = GameManager.Instance;
        Rigidbody body = game.Player.GetComponent<Rigidbody>();
        Pickup score = null;
        Pickup speed = null;
        foreach (Pickup pickup in Object.FindObjectsByType<Pickup>())
        {
            if (pickup.type == PickupType.Score && score == null) score = pickup;
            if (pickup.type == PickupType.Speed) speed = pickup;
        }
        Assert.IsNotNull(score);
        Assert.IsNotNull(speed);
        body.position = score.transform.position - Vector3.up;
        body.linearVelocity = Vector3.zero;
        yield return new WaitForFixedUpdate();
        yield return null;
        Assert.IsTrue(score == null);
        Assert.AreEqual(50, game.Score);

        Assert.AreEqual(30f, speed.respawnSeconds);
        speed.respawnSeconds = .3f;
        body.position = speed.transform.position - Vector3.up;
        body.linearVelocity = Vector3.zero;
        yield return new WaitForFixedUpdate();
        yield return null;
        Assert.IsFalse(speed.GetComponent<Collider>().enabled);
        Assert.IsFalse(speed.visual.gameObject.activeSelf);
        Assert.IsTrue(game.Powerups.SpeedActive);
        body.position = new Vector3(0f, .06f, -5f);
        body.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(.4f);
        Assert.IsTrue(speed.GetComponent<Collider>().enabled);
        Assert.IsTrue(speed.visual.gameObject.activeSelf);

        body.position = new Vector3(0f, -15f, 0f);
        yield return new WaitForSeconds(.8f);
        Assert.AreSame(game, GameManager.Instance);
        Assert.AreEqual(1, game.Level.levelNumber);
        Assert.AreEqual(3, game.Health.Health);
        Assert.AreEqual(0, game.Score);
    }
}
