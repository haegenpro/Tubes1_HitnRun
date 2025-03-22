using System;
using System.Linq;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class N1993R : Bot
{
    public const double SafeDistance = 35;
    public bool isAligning = false;
    public bool Hostile = false;
    public string currentWall;

    public double firePower = 3;

    private long lastScannedTime = 0;
    private const long scanTimeout = 8;

    public static readonly Random rnd = new Random();

    static void Main()
    {
        new N1993R().Start();
    }

    N1993R() : base(BotInfo.FromFile("N1993R.json")) { }

    public override void Run()
    {
        // Set warna bot (opsional)
        BodyColor = System.Drawing.Color.Black;
        GunColor = System.Drawing.Color.Black;
        RadarColor = System.Drawing.Color.Black;
        TurretColor = System.Drawing.Color.Black;
        BulletColor = System.Drawing.Color.Black;
        ScanColor = System.Drawing.Color.Black;

        isAligning = false;
        lastScannedTime = Environment.TickCount;
        SetTurnRadarLeft(360);

        while (IsRunning)
        {
            if (!isAligning)
            {
                AlignWithWall();
            }
            MoveAlongWall();
            Go();
        }

    }

    public void AlignWithWall()
    {
        if (Environment.TickCount - lastScannedTime > scanTimeout)
        {
            SetTurnRadarLeft(360);
        }
        // Mencari jarak terdekat ke dinding
        double distanceToLeftWall = X;
        double distanceToRightWall = ArenaWidth - X;
        double distanceToBottomWall = Y;
        double distanceToTopWall = ArenaHeight - Y;

        //Menentukan BOT harus mendekat ke tembok yang mana 
        var distances = new Dictionary<string, double>
        {
            { "Left", distanceToLeftWall },
            { "Right", distanceToRightWall },
            { "Top", distanceToTopWall },
            { "Bottom", distanceToBottomWall }
        };

        var minEntry = distances.Aggregate((l, r) => l.Value < r.Value ? l : r);
        currentWall = minEntry.Key;

        // Mencari arah BOT sekarang
        double currentHeading = Direction;

        // JIKA BOT dekat ke kanan
        if (minEntry.Key == "Right")
        {
            if (currentHeading <= 180)
            {
                TurnRight(currentHeading);
            }
            else
            {
                TurnLeft(360 - currentHeading);
            }

            // Saat sudah menghadap ke tembok, maju ke depan

            if (distanceToRightWall > SafeDistance)
            {
                Forward(distanceToRightWall - SafeDistance);
            }

            TurnRight(90);

        }

        else if (minEntry.Key == "Left")
        {
            if (currentHeading <= 180)
            {
                TurnLeft(180 - currentHeading);

            }
            else
            {
                TurnRight(currentHeading - 180);

            }

            // Saat sudah menghadap ke tembok, maju ke depan

            if (distanceToLeftWall > SafeDistance)
            {
                Forward(distanceToLeftWall - SafeDistance);
            }

            TurnRight(90);

        }

        else if (minEntry.Key == "Top")
        {
            if (currentHeading <= 90)
            {
                TurnLeft(90 - currentHeading);

            }
            else if (currentHeading >= 270)
            {
                TurnLeft(360 - currentHeading + 90);

            }
            else
            {
                TurnRight(currentHeading - 90);

            }

            // Saat sudah menghadap ke tembok, maju ke depan

            if (distanceToTopWall > SafeDistance)
            {
                Forward(distanceToTopWall - SafeDistance);
            }

            TurnRight(90);

        }

        else if (minEntry.Key == "Bottom")
        {
            if (currentHeading >= 270)
            {
                TurnRight(currentHeading - 270);

            }

            else if (currentHeading <= 90)
            {
                TurnRight(currentHeading + 90);

            }
            else
            {
                TurnLeft(270 - currentHeading);
            }

            // Saat sudah menghadap ke tembok, maju ke depan

            if (distanceToBottomWall > SafeDistance)
            {
                Forward(distanceToBottomWall - SafeDistance);
            }

            TurnRight(90);


        }

        isAligning = true;
    }

    public void MoveAlongWall()
    {
        if (Environment.TickCount - lastScannedTime > scanTimeout)
        {
            SetTurnRadarLeft(360);
        }
        double alignHeading = Direction;

        // Kasus DINDING KANAN ATAU KIRI
        if (currentWall == "Right" || currentWall == "Left")
        {
            double distanceToTopWall = ArenaHeight - Y;
            double distanceToBottomWall = Y;

            //Randomize UP or DOWN
            double totalWeight = distanceToTopWall + distanceToBottomWall;
            double randomValue = rnd.NextDouble() * totalWeight;
            int UpOrDown = randomValue < distanceToTopWall ? 0 : 1;

            // Kasus UP
            if (UpOrDown == 0)
            {
                // JIKA UP TIDAK MEPET
                if (distanceToTopWall > SafeDistance)
                {
                    double deltaDistance = distanceToTopWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);

                    if (alignHeading != 90)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }

                }

                // JIKA UP MEPET
                else
                {
                    double deltaDistance = distanceToBottomWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);

                    if (alignHeading == 90)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }
                }
            }

            // Kasus DOWN
            else
            {

                // Jika DOWN TIDAK MEPET
                if (distanceToBottomWall > SafeDistance)
                {
                    double deltaDistance = distanceToBottomWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    if (alignHeading != 270)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }
                }

                // Jika DOWN MEPET
                else
                {
                    double deltaDistance = distanceToTopWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);

                    if (alignHeading == 270)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }
                }
            }
        }

        //Kasus DINDING ATAS ATAU BAWAH
        else
        {
            double distanceToLeftWall = X;
            double distanceToRightWall = ArenaWidth - X;

            //Randomize LEFT or RIGHT
            double totalWeight = distanceToLeftWall + distanceToRightWall;
            double randomValue = rnd.NextDouble() * totalWeight;
            int LeftOrRight = randomValue < distanceToLeftWall ? 0 : 1;


            // Kasus LEFT
            if (LeftOrRight == 0)
            {
                // JIKA LEFT TIDAK MEPET
                if (distanceToLeftWall > SafeDistance)
                {
                    double deltaDistance = distanceToLeftWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    if (alignHeading != 180)
                    {
                        Forward(-move);
                    }
                    else
                    {

                        Forward(move);
                    }

                }

                // JIKA LEFT MEPET
                else
                {
                    double deltaDistance = distanceToRightWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    if (alignHeading == 180)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }

                }
            }

            // Kasus RIGHT
            else
            {
                // JIKA RIGHT TIDAK MEPET
                if (distanceToRightWall > SafeDistance)
                {
                    double deltaDistance = distanceToRightWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    if (alignHeading != 0)
                    {
                        Forward(-move);
                    }

                    else
                    {
                        Forward(move);
                    }


                }

                // JIKA RIGHT MEPET
                else
                {
                    double deltaDistance = distanceToLeftWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    if (alignHeading == 0)
                    {
                        Forward(-move);
                    }
                    else
                    {
                        Forward(move);
                    }
                }
            }
        }
    }


    public override void OnScannedBot(ScannedBotEvent e)
    {
        lastScannedTime = Environment.TickCount;
        double distance = DistanceTo(e.X, e.Y);
        double nextX = e.X + e.Speed * Math.Cos(e.Direction * Math.PI / 180);
        double nextY = e.Y + e.Speed * Math.Sin(e.Direction * Math.PI / 180);
        double radarLockAngle = DirectionTo(nextX, nextY);
        double radarTurn = CalcRadarBearing(radarLockAngle);
        double predictedAngle = AngleProjection(e);
        double gunTurn = CalcGunBearing(predictedAngle);
        double Turn = BearingTo(e.X, e.Y);
        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);

        if (gunTurn < 5)
        {
            SetFire(firePower);
        }


        if (distance > 200)
        {

            if (Hostile)
            {
                isAligning = false;
            }
            Hostile = false;
        }


        if (distance <= 200)
        {
            SetTurnLeft(Turn);
            SetForward(Math.Min(distance / 2, 150));
            Hostile = true;
        }
        Rescan();
        ClearEvents();
    }

    public double AngleProjection(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        if (distance < 10)
        {
            firePower = (Energy > 3) ? 3 : Energy;
        }
        else
        {
            if (Energy < 10 || distance > 200)
                firePower = 1;
            else if (distance > 150)
                firePower = 2;
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


    public override void OnHitWall(HitWallEvent e)
    {
        isAligning = false;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double get_radar_angle = RadarBearingTo(e.X, e.Y);
        double get_gun_angle = GunBearingTo(e.X, e.Y);
        SetTurnRadarLeft(get_radar_angle);
        SetTurnGunLeft(get_gun_angle);

        SetFire(3);

        Forward(50);
    }

}