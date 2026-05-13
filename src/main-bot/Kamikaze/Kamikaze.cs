using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Kamikaze : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new Kamikaze().Start();
    }

    Kamikaze() : base(BotInfo.FromFile("Kamikaze.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);

        while (IsRunning)
        {
            SetForward(100); 
            SetTurnGunLeft(360);
            Go(); 
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
    
        double angle = BearingTo(e.X, e.Y);
        double bearing = GunBearingTo(e.X, e.Y);


        SetTurnLeft(angle);
        SetTurnGunLeft(bearing);

        Fire(3);

        SetForward(100);
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
        TurnRight(180);
    }
}