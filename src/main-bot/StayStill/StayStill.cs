using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class StayStill : Bot
{
    private int turnCounter;
    private int lastMoveTurn;
    private double destinationX;
    private double destinationY;
    private string lockedTargetName;
    private ScannedBotEvent lockedTarget;
    private static readonly Random random = new Random();

    static void Main(string[] args)
    {
        new StayStill().Start();
    }

    StayStill() : base(BotInfo.FromFile("StayStill.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);

        ChooseNewDestination();
        lastMoveTurn = 0;

        while (IsRunning)
        {
            turnCounter++;
            bool lowEnergy = Energy < 50;
            bool moveTurn = (turnCounter - lastMoveTurn) >= 30;
            bool hasCloseTarget = lockedTarget != null && DistanceTo(lockedTarget.X, lockedTarget.Y) <= 100;

            if (moveTurn)
            {
                ChooseNewDestination();
                lastMoveTurn = turnCounter;
            }

            if (lockedTarget != null && lockedTarget.Name != lockedTargetName)
            {
                lockedTarget = null;
                lockedTargetName = null;
            }

            if (lowEnergy)
            {
                if (hasCloseTarget)
                {
                    ChaseLockedTarget();
                }
                else
                {
                    MoveToDestination();
                }
            }
            else if (lockedTarget != null)
            {
                ChaseLockedTarget();
            }
            else if (moveTurn)
            {
                MoveToDestination();
            }
            else
            {
                StayAndScan();
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        bool isNewCloseThreat = distance <= 50;

        if (lockedTargetName == null || lockedTargetName == e.Name || isNewCloseThreat)
        {
            lockedTargetName = e.Name;
            lockedTarget = e;
        }

        if (lockedTarget != null && lockedTargetName == e.Name)
        {
            lockedTarget = e;
        }

        if (distance <= 100)
        {
            if (distance <= 20)
            {
                Fire(3);
            }
            else if (distance <= 50)
            {
                Fire(2);
            }
            else
            {
                Fire(1);
            }
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        lockedTargetName = e.Name;
        lockedTarget = null;

        Fire(1);
        Fire(1);
        SetForward(100);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TurnRight(90);
        SetForward(100);
        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (lockedTargetName == e.Name)
        {
            lockedTargetName = null;
            lockedTarget = null;
        }
    }

    private void StayAndScan()
    {
        SetTurnGunLeft(360);
        Go();
    }

    private void MoveToDestination()
    {
        double angle = BearingTo(destinationX, destinationY);
        double distance = DistanceTo(destinationX, destinationY);

        if (distance < 20)
        {
            StayAndScan();
            return;
        }

        SetTurnLeft(angle);
        SetForward(distance);
        SetTurnGunLeft(360);
        Go();
    }

    private void ChaseLockedTarget()
    {
        if (lockedTarget == null)
        {
            StayAndScan();
            return;
        }

        double angle = BearingTo(lockedTarget.X, lockedTarget.Y);
        double gunAngle = GunBearingTo(lockedTarget.X, lockedTarget.Y);
        double distance = DistanceTo(lockedTarget.X, lockedTarget.Y);

        SetTurnLeft(angle);
        SetTurnGunLeft(gunAngle);
        SetForward(Math.Min(distance, 120));
        Go();
    }

    private void ChooseNewDestination()
    {
        double heading = random.NextDouble() * 360.0;
        double distance = 120.0 + random.NextDouble() * 180.0;
        destinationX = X + distance * Math.Cos(DegreesToRadians(heading));
        destinationY = Y + distance * Math.Sin(DegreesToRadians(heading));
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}