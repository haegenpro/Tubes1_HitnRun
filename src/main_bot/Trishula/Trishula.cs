using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Trishula : Bot
{   
    private const double DegToRadConst = Math.PI / 180; // Konstanta untuk mengubah derajat menjadi radians
    private Dictionary<int, Enemy> enemies; // Dictionary guna menyimpan data musuh saat scan
    private Enemy target; // Target saat ini
    private PointD curPos, nextPos;
    private double MIN_POWER = 0.1;
    private double MAX_POWER = 3.0;
    private double BASE_DISTANCE = 150.0;
    private int stopAndGoCounter = 0; // Counter untuk metode gerak stop and go
    private int currentInterval = 0;

    // Interval gerak dan diam saat 1v1
    private const int MinMoveInterval = 30;
    private const int MaxMoveInterval = 180;
    private const int MinStopInterval = 5;
    private const int MaxStopInterval = 10;
    private bool isMoving = true;
    private static Random rand = new Random();
    static void Main(string[] args)
    {
        new Trishula().Start();
    }

    Trishula() : base(BotInfo.FromFile("Trishula.json")) { }

    public override void Run()
    {
        // Memastikan radar dan gun independen dari tubuh bot
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        /* Bot colors */
        BodyColor = Color.FromArgb(230, 230, 230);
        TurretColor = Color.FromArgb(30, 40, 90);
        RadarColor = Color.FromArgb(230, 140, 80);
        BulletColor = Color.FromArgb(120, 180, 255);
        ScanColor = Color.FromArgb(150, 200, 255);
        TracksColor = Color.FromArgb(30, 40, 90);
        GunColor = Color.FromArgb(230, 230, 230);

        // Inisialisasi variabel
        target = new Enemy(0, new PointD(X, Y), 0, 0, 0, 0);
        target.active = false;
        enemies = new Dictionary<int, Enemy>();
        curPos = new PointD(X, Y);
        nextPos = curPos;
        RectangleD battlefield = new RectangleD(45, 45, ArenaWidth - 90, ArenaHeight - 90);
        
        while (IsRunning) // Main loop
        {
            // Radar berputar di awal match untuk mendapatkan data semua musuh
            // Kemudian dia akan mengarah ke musuh yang paling lama tidak discan
            SetTurnRadarLeft(60);
            try {
                int stalestTime = int.MaxValue;
                foreach (Enemy en in enemies.Values) {
                    if (TurnNumber > 20 && en.active && en.scanTime < stalestTime) {
                        stalestTime = en.scanTime;
                        SetTurnRadarLeft(Math.Sign(RadarBearingTo(en.location.X, en.location.Y)) * 60);
                    }
                }
            } catch (NullReferenceException e) { }

            // Cari target terdekat
            FindNewTarget();
            curPos.X = X;
            curPos.Y = Y;
            // Bergerak setelah setidaknya data semua musuh telah didapatkan
            if (TurnNumber > 9 && target.active) {
                Move(battlefield);
            }
            Go();
        }
    }

    private void Move(RectangleD battlefield) {
        bool isOneVsOne = EnemyCount == 1; // Mengecek apakah 1v1

        // Jika 1v1, pergerakan akan secara random berhenti untuk mengecohkan musuh
        if (isOneVsOne && DistanceTo(target.location.X, target.location.Y) > 200) {
            if (stopAndGoCounter >= currentInterval) {
                isMoving = !isMoving;
                stopAndGoCounter = 0;

                if (isMoving) {
                    currentInterval = rand.Next(MinMoveInterval, MaxMoveInterval + 1);
                } else {
                    currentInterval = rand.Next(MinStopInterval, MaxStopInterval + 1);
                }
            }

            if (isMoving) {
                double distanceToDest = DistanceTo(nextPos.X, nextPos.Y);
                if (distanceToDest < 15) {
                    GeneratePoint(DistanceTo(target.location.X, target.location.Y), battlefield);
                } else {
                    double angle = BearingTo(nextPos.X, nextPos.Y);
                    double direction = 1;
                    
                    if (Math.Cos(DegToRadConst * angle) < 0) {
                        angle -= 180;
                        direction = -1;
                    }

                    SetTurnLeft(angle);
                    SetForward(distanceToDest * direction);
                    TargetSpeed = Math.Abs(angle) > 60 ? 0 : 8;
                }
            } else {
                SetForward(0);
                TargetSpeed = 0;
            }

            stopAndGoCounter++;
        } else {
            double distanceToDest = DistanceTo(nextPos.X, nextPos.Y);

            // Mencari titik destinasi baru
            if (distanceToDest < 15) {
                GeneratePoint(DistanceTo(target.location.X, target.location.Y), battlefield);
            } else {
                // Bergerak ke arah tersebut
                double angle = BearingTo(nextPos.X, nextPos.Y); // Cari sudut
                double direction = 1;
                
                if (Math.Cos(DegToRadConst * angle) < 0) { // Menentukan apakah bergerak ke depan atau kebelakang
                    angle -= 180;
                    direction = -1;
                }

                SetTurnLeft(angle);
                SetForward(distanceToDest * direction);
                TargetSpeed = Math.Abs(angle) > 60 ? 0 : 8; // Kecepatan maksimum jika sudut < 60
            }
        }
    }

    // Menghasilkan titik-titik di sekitar bot
    private void GeneratePoint(double distanceToTarget, RectangleD battlefield) {
        PointD test;
        double risk = double.PositiveInfinity;
        double currentRisk;

        int i = 0;
        do {
            test = CalcPoint(curPos, Math.Min(0.7 * distanceToTarget, 100 + 150 * rand.NextDouble()), 2 * Math.PI * rand.NextDouble());
            currentRisk = RiskFunction(test);
            if (battlefield.contains(test.X, test.Y) && currentRisk < risk && !IsPathBlocked(curPos, test)) {
                risk = currentRisk;
                nextPos = test;
            }
            i++;
        } while (i < 360); // Mencoba 360 titik di sekitar bot
    }

    // Menentukan resiko tiap titik
    private double RiskFunction(PointD dest) {
        double risk = 0.08 / distanceSquared(dest, curPos); // Menambah risk ke posisi saat ini agar tidak diam di tempat
        foreach (var enemy in enemies.Values) {
            double energyRatio = Math.Min(enemy.energy / Energy, 2); // Rasio energi
            double perpendicularity = Math.Abs(Math.Cos(CalcAngleP(curPos, dest) - CalcAngleP(enemy.location, dest))); //Tegak lurus
            double distanceFactor = distanceSquared(dest, enemy.location); // Jarak
            risk += energyRatio * (1 + perpendicularity) / distanceFactor;
        }
        return risk;
    }

    // Mencari musuh terdekat
    private void FindNewTarget()
    {
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
        // Memasukkan musuh ke dalam dictionary
        Enemy en;
        if (!enemies.TryGetValue(e.ScannedBotId, out en)) {
            en = new Enemy(e.ScannedBotId, new PointD(e.X, e.Y), e.Energy, e.Speed, e.Direction, e.TurnNumber);
            enemies.Add(e.ScannedBotId, en);
        } else {
            // Mengupdate data
            enemies[e.ScannedBotId].location.X = e.X;
            enemies[e.ScannedBotId].location.Y = e.Y;
            enemies[e.ScannedBotId].energy = e.Energy;
            enemies[e.ScannedBotId].speed = e.Speed;
            enemies[e.ScannedBotId].direction = e.Direction;
            enemies[e.ScannedBotId].scanTime = TurnNumber;
        }
        
        FindNewTarget(); // Cari target terdekat
        if (target.id == e.ScannedBotId) {
            // Menentukan power dan toleransi angle dari tembakan
            double safeDistance = Math.Max(DistanceTo(target.location.X, target.location.Y), 1.0);
            double distanceComponent = 0.65 + (BASE_DISTANCE/safeDistance);
            double enemyEnergyLimit = target.energy * 0.3;
            double selfEnergyLimit = Energy * 0.2;  
            double bulletPower = Math.Min(Math.Min(enemyEnergyLimit, selfEnergyLimit), distanceComponent);
            bulletPower = Math.Min(MAX_POWER, Math.Max(MIN_POWER, bulletPower));
            if (Energy < 10) { // Menghemat energi ketika energi rendah
                bulletPower = MIN_POWER;
            }
            double maxAngleTolerance = 5.0;
            double minDistanceForMaxTolerance = 50.0;
            double maxDistanceForZeroTolerance = 200.0;
            double angleTolerance;
            if (safeDistance <= minDistanceForMaxTolerance)
            {
                angleTolerance = maxAngleTolerance;
            }
            else if (safeDistance <= maxDistanceForZeroTolerance)
            {
                double scale = (maxDistanceForZeroTolerance - safeDistance) / (maxDistanceForZeroTolerance - minDistanceForMaxTolerance);
                angleTolerance = maxAngleTolerance * scale;
            }
            else
            {
                angleTolerance = 0.0;
            }
            if (DistanceTo(target.location.X, target.location.Y) < 150 && target.active) {
                bulletPower = 3;
                angleTolerance = 5;
            }
            if (Energy > 5) { // Stop menembak saat energi sedikit
                LinearTargeting(bulletPower, angleTolerance);
            }
        }
    }

    private void LinearTargeting(double bulletPower, double angleTolerance){
        if (target.speed == 0) { // Jika diam, langsung saja tembak posisinya
            HeadOnTargeting(bulletPower, angleTolerance);
            return;
        } 

        // Mengekstrapolasi posisi musuh dengan asumsi bahwa kecepatan linier
        double distance = DistanceTo(target.location.X, target.location.Y);
        double bulletSpeed = CalcBulletSpeed(bulletPower);
        double enemyVX = target.speed * Math.Cos(target.direction * Math.PI / 180);
        double enemyVY = target.speed * Math.Sin(target.direction * Math.PI / 180);
        double dx = target.location.X - X;
        double dy = target.location.Y - Y;
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
        double enemyXPredicted = target.location.X + enemyVX * t;
        double enemyYPredicted = target.location.Y + enemyVY * t;
        enemyXPredicted = Math.Max(0, Math.Min(enemyXPredicted, ArenaWidth));
        enemyYPredicted = Math.Max(0, Math.Min(enemyYPredicted, ArenaHeight));
        double angle = GunBearingTo(enemyXPredicted, enemyYPredicted);
        SetTurnGunLeft(angle);
        if (GunHeat == 0 && GunTurnRemaining <= 5) {
            SetFire(bulletPower);
        }
    }

    // Menembak langsung ke posisi musuh saat ini
    public void HeadOnTargeting(double bulletPower, double angleTolerance) {
        SetTurnGunLeft(GunBearingTo(target.location.X, target.location.Y));
        if (GunHeat == 0 && GunTurnRemaining <= angleTolerance) {
            SetFire(bulletPower);
        }
    }
    public override void OnBotDeath(BotDeathEvent botDeathEvent)
    {
        enemies[botDeathEvent.VictimId].active = false; // Menonaktifkan boolean active di musuh
        FindNewTarget();
    }


    public override void OnHitBot(HitBotEvent e)
    {
        target = enemies[e.VictimId];
        HeadOnTargeting(3, 5); // Membalas tabrakan dari musuh
        RectangleD battlefield = new RectangleD(45, 45, ArenaWidth - 90, ArenaHeight - 90);
        GeneratePoint(double.MaxValue, battlefield); // Mencari titik baru
    }

    public override void OnHitByBullet(HitByBulletEvent bulletHitBotEvent)
    {
        if (DistanceTo(target.location.X, target.location.Y) < 100) {
            HeadOnTargeting(3, 5); // Jika musuh dekat, tembak dengan full power
        }
        if (bulletHitBotEvent.Bullet.Power > 1.5) { // Mencari titik baru jika tembakan kuat
            RectangleD battlefield = new RectangleD(45, 45, ArenaWidth - 90, ArenaHeight - 90);
            GeneratePoint(double.MaxValue, battlefield);
        }
    }

    // Utils
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

    // Menentukan apakah ada musuh yang menghalang jalan
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
        public bool active; // Menentukan apakah musuh masih aktif atau tidak
        public int id; // ID musuh
        public PointD location; // Lokasi musuh
        public double energy, speed, direction;
        public int scanTime; // Timestamp waktu scan
    
    
        // Constructor
        public Enemy(int id, PointD location, double energy, double speed, double direction, int scanTime) {
            active = true;
            this.id = id;
            this.location = location;
            this.energy = energy;
            this.speed = speed;
            this.direction = direction;
            this.scanTime = scanTime;
        }
        
    }

    // Kelas Point Double
    public class PointD {
        public double X;
        public double Y;

        // Constructor
        public PointD(double X, double Y) {
            this.X = X;
            this.Y = Y;
        }

        public double DistanceTo(double x, double y) 
        {
            var dx = x - X;
            var dy = y - Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

    }
    
    // Kelas Rectangle Double
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
