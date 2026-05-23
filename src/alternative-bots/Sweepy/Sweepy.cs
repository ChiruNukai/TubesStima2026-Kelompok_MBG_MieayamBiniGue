using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Sweepy : Bot
{
    static void Main(string[] args) => new Sweepy().Start();
    Sweepy() : base(BotInfo.FromFile("Sweepy.json")) { }

    private ScannedBotEvent sc = null;

    public override void Run()
    {
        BodyColor = Color.Purple;
        TurretColor = Color.Black;
        RadarColor = Color.Magenta;
        BulletColor = Color.Black;
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);

        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        TurnLeft(CalcAngleToNearestWall());

        while (IsRunning)
        {
            SetForward(1000);
            SetTurnRadarLeft(360);
            Go();
        }
    }

    private double CalcAngleToNearestWall()
    {
        double distLeft   = X;
        double distRight  = ArenaWidth - X;
        double distBottom = Y;
        double distTop    = ArenaHeight - Y;
        double minDist    = Math.Min(Math.Min(distLeft, distRight), Math.Min(distBottom, distTop));

        double targetHeading;
        if      (minDist == distLeft)   targetHeading = 270;
        else if (minDist == distRight)  targetHeading = 90;
        else if (minDist == distBottom) targetHeading = 180;
        else                            targetHeading = 0;

        double bearing = targetHeading - Direction;
        if (bearing > 180)  bearing -= 360;
        if (bearing < -180) bearing += 360;
        return bearing;
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        sc = e;

        double dist        = DistanceTo(e.X, e.Y);
        double bulletPower = dist < 150 ? 3 : dist < 300 ? 2 : 1;

        SetTurnGunLeft(GunBearingTo(e.X, e.Y));
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));

        if (GunHeat == 0)
            Fire(bulletPower);
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (sc != null && e.VictimId == sc.ScannedBotId)
            sc = null;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y));
        if (GunHeat == 0)
            Fire(3);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TurnLeft(90);
    }
}