using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using SvgNet.Types;

public class Trishula : Bot
{   
    private Queue<PointD> moveHistory = new Queue<PointD>();
    private const int HistorySize = 5;
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
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        /* Bot colors */
        BodyColor = Color.FromArgb(230, 230, 230);
        TurretColor = Color.FromArgb(30, 40, 90);
        RadarColor = Color.FromArgb(230, 140, 80);
        BulletColor = Color.FromArgb(120, 180, 255);
        ScanColor = Color.FromArgb(150, 200, 255);
        TracksColor = Color.FromArgb(30, 40, 90);
        GunColor = Color.FromArgb(230, 230, 230);

        enemies = new Dictionary<int, Enemy>();
        target = null;
        curPos = new PointD(X, Y);
        nextPos = curPos;
        prevPos = curPos;
        SetTurnRadarRight(Double.PositiveInfinity);

        RectangleD battlefield = new RectangleD(36, 36, ArenaWidth - 72, ArenaHeight - 72);
        while (IsRunning)
        {
            if (target == null || !target.active)
            {
                FindNewTarget();
            }
            curPos.X = X;
            curPos.Y = Y;
            Move(battlefield);
            if (target != null) {
                CircularTargeting(2);
            }
            Go();
        }
    }

    private void Move(RectangleD battlefield) {
        double distanceToDest = DistanceTo(nextPos.X, nextPos.Y);

        Console.WriteLine($"Pos({X}, {Y})");
        double distanceToTarget = target != null ? DistanceTo(target.location.X, target.location.Y) : double.MaxValue;
        // GeneratePoint(distanceToTarget, battlefield);
        // Console.WriteLine($"NextPos({nextPos.X}, {nextPos.Y})");
        
        if (distanceToDest < 15) {
            // Console.Write($"Pos({X}, {Y})");
            GeneratePoint(distanceToTarget, battlefield);
            Console.Write($"NextPos({nextPos.X}, {nextPos.Y})");
        } else {
            double angle = BearingTo(nextPos.X, nextPos.Y);
            double direction = 1;
            
            if(Math.Cos(DegToRadConst * angle) < 0) {
                direction = -1;
            }

            SetTurnLeft(angle);
            SetForward(distanceToDest * direction);
            TargetSpeed = Math.Abs(angle) > 60 ? 0 : 8;
            Console.WriteLine($"Speed:{Speed}");
        }
    }

    private void GeneratePoint(double distanceToTarget, RectangleD battlefield) {
        PointD test;
        double risk = double.PositiveInfinity;
        double currentRisk;
        for (double i = 0; i < Math.PI * 2; i += 0.01 * Math.PI) {
            test = CalcPoint(curPos, Math.Min(0.5 * distanceToTarget, 100 + 150 * rand.NextDouble()), i);
            currentRisk = RiskFunction(test);
            if (battlefield.contains(test.X, test.Y) && currentRisk < risk && !IsPathBlocked(curPos, test)) {
                risk = currentRisk;
                nextPos = test;
            }
        }
        prevPos = curPos;
        
        moveHistory.Enqueue(curPos);
        if (moveHistory.Count > HistorySize) {
            moveHistory.Dequeue();
        }
        // int i = 0;
        // do {
        //     test = CalcPoint(curPos, Math.Min(0.7 * distanceToTarget, 100 + 150 * rand.NextDouble()), 2 * Math.PI * rand.NextDouble());
        //     if (battlefield.contains(test.X, test.Y) && RiskFunction(test) < RiskFunction(nextPos) && !IsPathBlocked(curPos, test)) {
        //         nextPos = test;
        //     }
        //     i++;
        // } while (i < 200);
        // prevPos = curPos;
        // moveHistory.Enqueue(curPos);
        // if (moveHistory.Count > HistorySize) {
        //     moveHistory.Dequeue();
        // }
    }

    private double RiskFunction(PointD dest) {
        double risk = 0.08 / distanceSquared(dest, prevPos);
        foreach (PointD past in moveHistory) {
            risk += 0.08 / (distanceSquared(dest, past) + 1);
        }
        foreach (var enemy in enemies.Values) {
            double energyRatio = Math.Min(enemy.energy / Energy, 2);
            double perpendicularity = Math.Abs(Math.Cos(CalcAngleP(curPos, dest) - CalcAngleP(enemy.location, curPos)));
            double distanceFactor = distanceSquared(dest, enemy.location) + 50;
            risk += 0.08 * energyRatio *(1 + perpendicularity) / distanceFactor;
        }
        return risk;
    }

    private void FindNewTarget()
    {
        target = null;
        double minDistance = double.MaxValue;
        
        foreach (var enemy in enemies.Values)
        {
            if (!enemy.active) continue;
            
            double distance = DistanceTo(enemy.location.X, enemy.location.Y);
            if (distance < minDistance)
            {
                minDistance = distance;
                target = enemy;
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Enemy en;
        if (!enemies.TryGetValue(e.ScannedBotId, out en)) {
            en = new Enemy(e.ScannedBotId, new PointD(e.X, e.Y), e.Energy, e.Speed, e.Direction);
            enemies.Add(e.ScannedBotId, en);
        } else {
            enemies[e.ScannedBotId].prevLocation.X = enemies[e.ScannedBotId].location.X;
            enemies[e.ScannedBotId].prevLocation.Y = enemies[e.ScannedBotId].location.Y;
            enemies[e.ScannedBotId].prevEnergy = enemies[e.ScannedBotId].energy;
            enemies[e.ScannedBotId].prevSpeed = enemies[e.ScannedBotId].speed;
            enemies[e.ScannedBotId].prevDirection = enemies[e.ScannedBotId].direction;
            enemies[e.ScannedBotId].location.X = e.X;
            enemies[e.ScannedBotId].location.Y = e.Y;
            enemies[e.ScannedBotId].energy = e.Energy;
            enemies[e.ScannedBotId].speed = e.Speed;
            enemies[e.ScannedBotId].direction = e.Direction;
            en.active = true;
        }

        if (en.active && (target == null || DistanceTo(e.X, e.Y) < DistanceTo(target.location.X, target.location.Y)))
        {
            target = en;
        }
    }

    public void CircularTargeting(double bulletPower) {
        // Console.WriteLine(target.id);
        double enemyDirection = target.direction;

        double enemyPrevDirection = target.prevDirection;

        double directionChange = enemyDirection - enemyPrevDirection;
        double i = 0;
        PointD predictedPosition = new PointD(target.location.X, target.location.Y);
        while ((++i) * (20.0 - 3.0 * bulletPower) < DistanceTo(predictedPosition.X, predictedPosition.Y)) {
            predictedPosition = CalcPoint(predictedPosition, target.speed, enemyDirection);
            enemyDirection += directionChange;
            if (predictedPosition.X < 18 || predictedPosition.Y < 18 || predictedPosition.X > ArenaWidth - 18 || predictedPosition.Y > ArenaHeight - 18) {
                predictedPosition.X = Math.Min(Math.Max(18, predictedPosition.X), ArenaWidth - 18);
                predictedPosition.Y = Math.Min(Math.Max(18, predictedPosition.Y), ArenaHeight - 18);
                break;
            }
        }
        
        double angle = GunBearingTo(predictedPosition.X, predictedPosition.Y);
        SetTurnGunLeft(angle);
        if (GunTurnRemaining == 0) { 
            SetFire(bulletPower);
        }
    }

    public void HeadOnTargeting(double bulletPower) {
        SetTurnGunLeft(GunBearingTo(target.location.X, target.location.Y));
        SetFire(bulletPower);
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (enemies.ContainsKey(e.VictimId))
        {
            // Mark enemy as inactive
            enemies[e.VictimId].active = false;
        FindNewTarget();    
        }
    }

    public override void OnHitBot(HitBotEvent botHitBotEvent)
    {
        RectangleD battlefield = new RectangleD(36, 36, ArenaWidth - 72, ArenaHeight - 72);
        GeneratePoint(double.MaxValue, battlefield);
    }

    public override void OnHitByBullet(HitByBulletEvent bulletHitBotEvent)
    {
        RectangleD battlefield = new RectangleD(36, 36, ArenaWidth - 72, ArenaHeight - 72);
        GeneratePoint(double.MaxValue, battlefield);
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
            double enemyDist = DistanceToLineSegment(enemy.location, start, dest);
            
            if (enemyDist < 36) {
                return true;
            }
        }
        return false;
    }

    private double DistanceToLineSegment(PointD C, PointD A, PointD B) {
        double dx = B.X - A.X;
        double dy = B.Y - A.Y;
        double lengthSquared = dx * dx + dy * dy;
        
        if (lengthSquared == 0) {
            return Math.Sqrt(distanceSquared(A, C));
        }

        double t = ((C.X - A.X) * dx + (C.Y - A.Y) * dy) / lengthSquared;
        t = Math.Max(0, Math.Min(1, t));

        double closestX = A.X + t * dx;
        double closestY = A.Y + t * dy;

        return Math.Sqrt(distanceSquared(C, new PointD(closestX, closestY)));
    }

    public class Enemy {
        public bool active;
        public int id;
        public PointD location, prevLocation;
        public double energy, speed, direction, prevEnergy, prevSpeed, prevDirection;
        
        public Queue<double> directionHistory = new Queue<double>(5);
        public Queue<double> speedHistory = new Queue<double>(5);
    
    
        public Enemy(int id, PointD location, double energy, double speed, double direction) {
            active = true;
            this.id = id;
            this.location = location;
            this.energy = energy;
            this.speed = speed;
            this.direction = direction;
            prevLocation = location;
            prevEnergy = energy;
            prevDirection = direction;
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



