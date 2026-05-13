using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class RotasiBumi : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new RotasiBumi().Start();
    }

    RotasiBumi() : base(BotInfo.FromFile("RotasiBumi.json")) { }

    public override void Run()
    {
        BodyColor = Color.White;
        TurretColor = Color.White;
        RadarColor = Color.Yellow;
        BulletColor = Color.Yellow;
        ScanColor = Color.Green;
        
        int zigzagDirection = 1;
        AdjustGunForBodyTurn = true;

        while (IsRunning)
        {
            SetForward(100); 
            SetTurnGunLeft(360);
            Go(); 
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
    
        double bearing = GunBearingTo(e.X, e.Y);
        double distance = DistanceTo(e.X, e.Y);
        
        SetTurnGunLeft(bearing);

        if(Math.Abs(bearing) < 10 && GunHeat == 0)
        {
            Fire(Math.Min(3, 400 / distance));
        }
        

        SetTurnLeft(20);
        SetForward(20);
        Go();
        
        
    }

    public override void OnHitBot(HitBotEvent e)
    {   
        SetForward(100);
        Fire(3); 
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Back(60);
        TurnLeft(180);
    }
}