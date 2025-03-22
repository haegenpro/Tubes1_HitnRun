using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Forza_Ferrari : Bot
{   
    bool movingForward;
    bool evaluationMode = true;
    double firePower = 3;
    List<Enemy> enemies;
    Enemy target;
    public class Enemy
    {
        public bool active;
        public int id;
        public double ex, ey, energy, speed, direction;
        public Enemy(int id, double ex, double ey, double energy, double speed, double direction) {
            active = true;
            this.id = id;
            this.ex = ex;
            this.ey = ey;
            this.energy = energy;
            this.speed = speed;
            this.direction = direction;
        }
    }
    static void Main()
    {
        new Forza_Ferrari().Start();
    }
    Forza_Ferrari() : base(BotInfo.FromFile("Forza_Ferrari.json")) { }
    public override void Run()
    {
        BodyColor = Color.FromArgb(202, 0, 42);
        TurretColor = Color.FromArgb(216, 31, 42);
        RadarColor = Color.FromArgb(255, 88, 79);
        BulletColor = Color.FromArgb(255, 17, 0);
        ScanColor = Color.FromArgb(181, 40, 48);
        TracksColor = Color.FromArgb(256, 242, 0);
        GunColor = Color.FromArgb(230, 230, 230);

        enemies = new List<Enemy>();
        target = null;
        while (IsRunning)
        {
            if (!evaluationMode){
                evaluationMode = true;
                SetTurnRadarLeft(360);
                Evaluate();
            }
            else
            {
                SetTurnRadarLeft(360);
            }
        }
    }
    private void Evaluate()
    {
        Enemy bestCandidate = null;
        double maxAvgDistance = double.MinValue;
        foreach (var candidate in enemies)
        {
            if (!candidate.active) continue;
            double totalDistance = 0;
            int count = 0;
            foreach (var other in enemies)
            {
                if (other.id == candidate.id || !other.active) continue;
                double dx = candidate.ex - other.ex;
                double dy = candidate.ey - other.ey;
                totalDistance += Math.Sqrt(dx * dx + dy * dy);
                count++;
            }
            double avgDistance = (count > 0) ? totalDistance / count : double.MaxValue;
            if (avgDistance > maxAvgDistance)
            {
                maxAvgDistance = avgDistance;
                bestCandidate = candidate;
            }
        }
        if (bestCandidate != null)
        {
            target = bestCandidate;
        }
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
    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (evaluationMode)
        {
            enemies[e.ScannedBotId] = new Enemy(e.ScannedBotId, e.X, e.Y, e.Energy, e.Speed, e.Direction);
        }
        else
        {
            if (e.ScannedBotId == target.id)
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
                    SetFire(firePower);
                }
                SetTurnLeft(Turn);
                SetForward(Math.Min(distance / 4, 30));
                Rescan();
            }
        }
    }
    public override void OnHitBot(HitBotEvent e)
    {
        double angleToEnemy = DirectionTo(e.X, e.Y);
        double gunTurn = CalcGunBearing(angleToEnemy);
        double radarTurn = CalcRadarBearing(angleToEnemy);
        double turn = CalcBearing(angleToEnemy);
        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);
        if (GunHeat == 0)
        {
            SetFire(Energy < 3 ? Energy : 3);
        }
        SetTurnLeft(turn);
        SetForward(10);
    }
    public override void OnHitWall(HitWallEvent e)
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
    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId == target.id)
        {
            target.active = false;
            target = null;
            evaluationMode = true;
        }
        else
        {
            foreach (var enemy in enemies)
            {
                if (enemy.id == e.VictimId)
                {
                    enemy.active = false;
                    break;
                }
            }
        }
    }
}
