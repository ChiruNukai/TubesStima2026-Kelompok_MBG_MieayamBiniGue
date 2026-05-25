using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Kamikaze : Bot
{   
    static void Main(string[] args)
    {
        new Kamikaze().Start();
    }

    Kamikaze() : base(BotInfo.FromFile("Kamikaze.json")) { }

    public override void Run()
    {
        BodyColor = Color.White;
        TurretColor = Color.Red;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.Red;

        while (IsRunning)
        {
            //Maju Kedepan dan Scan musuh disekitar
            SetForward(100); 
            SetTurnGunLeft(360);
            Go(); 
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
    
        double angle = BearingTo(e.X, e.Y);
        double bearing = GunBearingTo(e.X, e.Y);


        SetTurnLeft(angle); //Memutar Badan Tank Kearah Musuh yang terscan
        SetTurnGunLeft(bearing); //Memutar Turret Tank Kearah Musuh yang terscan

        if (Math.Abs(bearing) < 10 && GunHeat == 0) { //Menembak dengan kekuatan penuh saat turret sudah lurus ke musuh
            Fire(3);
        }

        SetForward(100); //Maju terus
        Go();
    }

    public override void OnHitBot(HitBotEvent e)
    {   
        //Ketika menabrak musuh maju terus
        SetForward(100); 
        Fire(3); 
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        //Ketika menabrak tembok mundur dan berputar
        Back(60);
        TurnRight(180);
    }

}