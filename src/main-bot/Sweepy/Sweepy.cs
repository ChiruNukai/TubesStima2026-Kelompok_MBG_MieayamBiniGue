using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Sweepy : Bot
{   
    /* A bot that follows specific combat rules */
    private ScannedBotEvent target;
    private bool atCorner = false;
    private bool wallMode = false;
    private bool ramMode = false;

    static void Main(string[] args)
    {
        new Sweepy().Start();
    }   

    Sweepy() : base(BotInfo.FromFile("Sweepy.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);

        if (!atCorner)
        {
            // Move to corner (near 0,0 to avoid hitting wall immediately)
            double bearing = BearingTo(18, 18);
            SetTurnLeft(bearing);
            SetForward(DistanceTo(18, 18));
            Go();
            atCorner = true;
        }

        while (IsRunning)
        {
            bool hasTarget = target != null;

            if (hasTarget && Energy < 50)
            {
                ramMode = true;
                wallMode = false;
            }
            else if (hasTarget)
            {
                wallMode = false;
                ramMode = true;
            }
            else
            {
                wallMode = false;
                ramMode = false;
            }

            if (wallMode)
            {
                SetForward(100);
                Go();
            }
            else if (ramMode && target != null)
            {
                double angle = BearingTo(target.X, target.Y);
                SetTurnLeft(angle);
                SetForward(DistanceTo(target.X, target.Y));
                Go();
            }
            else
            {
                SetForward(100);
                SetTurnGunLeft(360);
                Go();
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        target = e;

        double angle = BearingTo(e.X, e.Y);
        double bearing = GunBearingTo(e.X, e.Y);

        SetTurnLeft(angle);
        SetTurnGunLeft(bearing);

        double dist = DistanceTo(e.X, e.Y);
        if (dist < 50)
        {
            Fire(3);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {   
        Fire(3);
        SetForward(100);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        if (wallMode)
        {
            TurnRight(90);
            SetForward(100);
            Go();
        }
        else
        {
            Back(60);
            TurnRight(180);
        }
    }

}