using System;
using System.Drawing;
using static System.Math;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class WildBot : Bot
{
    bool movingForward;
    private static Random random = new Random();

    double firePower = 3;
    static void Main()
    {
        new WildBot().Start();
    }
    WildBot() : base(BotInfo.FromFile("WildBot.json")) { }
        public override void Run()
    {
        BodyColor = Color.Green;
        TurretColor = Color.DarkGreen;
        RadarColor = Color.DarkRed;
        BulletColor = Color.Yellow;
        ScanColor = Color.LightYellow;

        RadarTurnRate = 20;
        GunTurnRate = 20;
        SetTurnGunLeft(Double.PositiveInfinity);
        SetTurnRadarLeft(Double.PositiveInfinity);
        
        while (IsRunning)
        {
            TurnRadarLeft(360);
        }
    }
    private void ReverseDirection()
    {
        if (movingForward)
        {
            Back(300);
            movingForward = false;
        }
        else
        {
            Forward(300);
            movingForward = true;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double nextX = e.X + e.Speed * Math.Cos(e.Direction * Math.PI / 180);
        double nextY = e.Y + e.Speed * Math.Sin(e.Direction * Math.PI / 180);
        double radarLockAngle = DirectionTo(nextX, nextY);
        double radarTurn = CalcRadarBearing(radarLockAngle);
        double predictedAngle = AngleProjection(e);
        double gunTurn = CalcGunBearing(predictedAngle);
        double Turn = CalcBearing(radarLockAngle);
        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);
        if (GunHeat == 0)
        {
            Fire(firePower);
        }
        SetTurnLeft(Turn);
        SetForward(Math.Min(distance / 5, 50));
        Rescan();
    }
    private double AngleProjection(ScannedBotEvent e){
        double distance = DistanceTo(e.X, e.Y);
        if (distance < 10)
        {
            firePower = (Energy > 3) ? 3 : Energy;
        }
        else
        {
            if (Energy < 10 || distance > 500)
                firePower = 1;
            else
                firePower = 3;
        }
        double bulletSpeed = CalcBulletSpeed(firePower);
        double enemyVX = e.Speed * Math.Cos(e.Direction * Math.PI / 180);
        double enemyVY = e.Speed * Math.Sin(e.Direction * Math.PI / 180);
        
        double dx = e.X - X;
        double dy = e.Y - Y;
        double a = enemyVX * enemyVX + enemyVY * enemyVY - bulletSpeed * bulletSpeed;
        double b = 2 * (dx * enemyVX + dy * enemyVY);
        double c = dx * dx + dy * dy;
        double discriminant = b * b - 4 * a * c;

        double t = 0;
        if (a != 0 && discriminant >= 0)
        {
            double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            t = (t1 > 0) ? t1 : (t2 > 0) ? t2 : 0;
        }
        else
        {
            t = distance / bulletSpeed;
        }
        double enemyXPredicted = e.X + enemyVX * t;
        double enemyYPredicted = e.Y + enemyVY * t;
        return DirectionTo(enemyXPredicted, enemyYPredicted);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        double angleToEnemy = DirectionTo(e.X, e.Y);
        double gunTurn = CalcGunBearing(angleToEnemy);
        double radarTurn = CalcRadarBearing(angleToEnemy);
        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);
        if (GunHeat == 0)
        {
            Fire(Energy < 3 ? Energy : 3);
        }
        SetForward(10);
    }
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }
}
