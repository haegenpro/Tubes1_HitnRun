using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Trishula : Bot
{   
    private const double WallStick = 140;
    private Queue<PointD> moveHistory = new Queue<PointD>();
    private const int HistorySize = 10;
    private const double RadToDegConst = 180 / Math.PI;
    private const double DegToRadConst = Math.PI / 180;
    private Dictionary<int, Enemy> enemies;
    private Enemy target;
    private PointD curPos, prevPos, nextPos;

    private static Random rand = new Random();
    
    static void Main(string[] args)
    {
        new Trishula().Start();
    }

    Trishula() : base(BotInfo.FromFile("Trishula.json")) { }

    public override void Run()
    {
        
        /* Bot colors */
        BodyColor = Color.FromArgb(230, 230, 230);
        TurretColor = Color.FromArgb(30, 40, 90);
        RadarColor = Color.FromArgb(230, 140, 80);
        BulletColor = Color.FromArgb(120, 180, 255);
        ScanColor = Color.FromArgb(150, 200, 255);
        TracksColor = Color.FromArgb(30, 40, 90);
        GunColor = Color.FromArgb(230, 230, 230);

        enemies = new Dictionary<int, Enemy>();
        curPos = new PointD(X, Y);
        nextPos = curPos;
        prevPos = curPos;
        SetTurnRadarRight(Double.PositiveInfinity);

        RectangleD battlefield = new RectangleD(27, 27, ArenaWidth - 54, ArenaHeight - 54);
        while (IsRunning)
        {
            curPos.X = X;
            curPos.Y = Y;
            Move(battlefield);
            Go();
        }
    }

    private void Move(RectangleD battlefield) {
        double distanceToDest = DistanceTo(nextPos.X, nextPos.Y);
        double distanceToTarget = target != null ? DistanceTo(target.location.X, target.location.Y) : double.MaxValue;

        if (distanceToDest < 15) {
            // Console.Write($"Pos({X}, {Y})");
            AssessPoint(distanceToTarget, battlefield);
            //Console.Write($"NextPos({nextPos.X}, {nextPos.Y})");
        } else {
            double angle = BearingTo(nextPos.X, nextPos.Y);
            double direction = 1;
            
            if(Math.Cos(DegToRadConst * angle) < 0) {
                direction = -1;
            }
            
            SetForward(distanceToDest * direction);
            SetTurnLeft(angle);
            TargetSpeed = Math.Abs(angle) > 60 ? 0 : 8;
        }
    }

    private void AssessPoint(double distanceToTarget, RectangleD battlefield) {
        PointD test;
        int i = 0;
        do {
            test = CalcPoint(curPos, Math.Min(0.7 * distanceToTarget, 100 + 150 * rand.NextDouble()), 2 * Math.PI * rand.NextDouble());
            if (battlefield.contains(test.X, test.Y) && RiskFunction(test) < RiskFunction(nextPos) && !IsPathBlocked(curPos, test)) {
                nextPos = test;
            }
            i++;
        } while (i < 200);
        prevPos = curPos;
        moveHistory.Enqueue(curPos);
        if (moveHistory.Count > HistorySize) {
            moveHistory.Dequeue();
        }
    }

    private double RiskFunction(PointD dest) {
        double risk = 0.08 / distanceSquared(dest, prevPos);
        foreach (PointD past in moveHistory) {
            risk += 0.08 / (distanceSquared(dest, past) + 1);
        }
        foreach (var enemy in enemies.Values) {
            double energyRatio = Math.Min(enemy.energy / Energy, 2);
            double perpendicularity = Math.Abs(Math.Cos(CalcAngleP(curPos, dest) - CalcAngleP(enemy.location, dest)));
            double distanceFactor = distanceSquared(dest, enemy.location) + 50;
            risk += 0.08 * energyRatio *(1 + perpendicularity) / distanceFactor;
        }
        return risk;
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Enemy en;
        if (!enemies.TryGetValue(e.ScannedBotId, out en)) {
            en = new Enemy(e.ScannedBotId, new PointD(e.X, e.Y), e.Energy, e.Speed);
            enemies.Add(e.ScannedBotId, en);
        } else {
            enemies[e.ScannedBotId].location.X = e.X;
            enemies[e.ScannedBotId].location.Y = e.Y;
            enemies[e.ScannedBotId].energy = e.Energy;
            enemies[e.ScannedBotId].speed = e.Speed;
        }

        if (target == null || DistanceTo(e.X, e.Y) < DistanceTo(target.location.X, target.location.Y))
        {
            target = en;
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.Remove(e.VictimId);
    }

    public override void OnHitBot(HitBotEvent botHitBotEvent)
    {
        RectangleD battlefield = new RectangleD(18, 18, ArenaWidth - 36, ArenaHeight - 36);
        AssessPoint(double.MaxValue, battlefield);
    }

    public override void OnHitByBullet(HitByBulletEvent bulletHitBotEvent)
    {
        
    }

    private double CalcAngleP(PointD p1, PointD p2) {
        return Math.Atan2(p2.Y - p1.Y,p2.X - p1.X);
    }

    private PointD CalcPoint(PointD p, double dest, double angle) {
        return new PointD(p.X + dest * Math.Cos(angle), p.Y + dest * Math.Sin(angle));
    }

    private double distanceSquared(PointD p1, PointD p2) {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;
        return Math.Pow(dx, 2) + Math.Pow(dy, 2);
    }

    private bool IsPathBlocked(PointD start, PointD dest) {
        foreach (var enemy in enemies.Values) {
            // Get perpendicular distance from enemy to the line (start → dest)
            double enemyDist = DistanceToLineSegment(enemy.location, start, dest);
            
            // If the enemy is within a bot-size radius (e.g., 36), it's blocking the path
            if (enemyDist < 36) {
                return true;
            }
        }
        return false;
    }

    // Helper function: Distance from point C to line segment AB
    private double DistanceToLineSegment(PointD C, PointD A, PointD B) {
        double dx = B.X - A.X;
        double dy = B.Y - A.Y;
        double lengthSquared = dx * dx + dy * dy;
        
        if (lengthSquared == 0) return Math.Sqrt(distanceSquared(A, C)); // A and B are the same point

        // Project point C onto line AB, clamping to segment
        double t = ((C.X - A.X) * dx + (C.Y - A.Y) * dy) / lengthSquared;
        t = Math.Max(0, Math.Min(1, t));

        // Find closest point on the segment
        double closestX = A.X + t * dx;
        double closestY = A.Y + t * dy;

        // Return distance from C to this closest point
        return Math.Sqrt(distanceSquared(C, new PointD(closestX, closestY)));
    }

    public class Enemy {
        public int id;
        public PointD location;
        public double energy, speed;
        
        public Enemy(int id, PointD location, double energy, double speed) {
            this.id = id;
            this.location = location;
            this.energy = energy;
            this.speed = speed;
        }
    }

    public class PointD {
        public double X;
        public double Y;

        public PointD(double X, double Y) {
            this.X = X;
            this.Y = Y;
        }
    }
    public class RectangleD
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public RectangleD(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool contains(double x, double y)
        {
            return x >= this.x && x <= this.x + width && y >= this.y && y <= this.y + height;
        }
    }
} 

