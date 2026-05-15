using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
 
public class Sweepy : Bot
{
    static void Main(string[] args)
    {
        new Sweepy().Start();
    }
 
    Sweepy() : base(BotInfo.FromFile("Sweepy.json")) { }
    
    private ScannedBotEvent sc = null;
    private int initialBotCount = 0;
    private bool aggressiveMode = false;

    public override void Run(){
        BodyColor = Color.Purple;
        TurretColor = Color.Black;
        RadarColor = Color.Magenta;
        BulletColor = Color.Black;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);

        AdjustGunForBodyTurn = true;
        initialBotCount = EnemyCount + 1;

        while (IsRunning) {
            

            SetForward(150);
            SetTurnGunLeft(360);
            Go();
        }
    }
 
    public void OnScannedBot(){
        if (sc != null){
            SetTurnLeft(BearingTo(sc.X, sc.Y));
            SetTurnGunLeft(GunBearingTo(sc.X, sc.Y));
            SetTurnRadarLeft(RadarBearingTo(sc.X, sc.Y));
            double dist = DistanceTo(sc.X, sc.Y);
 
            if (aggressiveMode){
                SetForward(dist);
            } else{
                SetForward(dist);
                if (dist < 150)
                    Fire(2);
                else
                    Fire(1);
            }
        } else{
            SetForward(150);
            SetTurnRadarLeft(360);
            Go();
        }
    }
 
    public override void OnBotDeath(BotDeathEvent BotDeathEvent)
    {
        if (sc != null && BotDeathEvent.VictimId == sc.ScannedBotId)
        {
            sc = null;
        }
    }
 
    public override void OnHitBot(HitBotEvent HitBotEvent){
        if (HitBotEvent.IsRammed)
        {
            SetTurnGunLeft(180);
            Fire(3);
        } else{
            Fire(3);
        }
    }
 
    public override void OnHitWall(HitWallEvent HitWallEvent)
    {
        Back(20);
        TurnRight(90);
        SetForward(150);
        Go();
    }
}