using System;
using System.Drawing;
using System.Dynamic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class RotasiBumi : Bot
{   
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
        
        //Memutar Senjata Kearah Musuh
        SetTurnGunLeft(bearing);

        if(Math.Abs(bearing) < 5 && GunHeat == 0)
        {
            Fire(Math.Min(3, 400 / distance)); //Menembak musuh dengan kekuatan tembakan disesuaikan dengan jarak
        }
        
        if(distance > 300)
        {
            //Merubah arah badan tank ke arah musuh lalu maju
            double angle = BearingTo(e.X, e.Y);
            SetTurnLeft(angle);
            SetForward(distance - 150);
        }
        else if (distance > 200)
        {
            //Berputar saat jarak sudah lumayan dekat
            SetTurnLeft(20);
            SetForward(100);
        }
        else
        {   
            //Berputar dengan sudut yang lebih tajam ketika sangat dekat
            SetTurnLeft(90);
            SetForward(200);
        }
        Go();
        
        
    }

    public override void OnHitBot(HitBotEvent e)
    {   
        //Maju terus saat menabrak musuh
        SetForward(100);
        Fire(3); 
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        //Mundur dan berputar saat menabrak tembok
        Back(60);
        TurnLeft(180);
    }
}