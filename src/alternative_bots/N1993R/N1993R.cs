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

    public static readonly Random rnd = new Random();

    static void Main(string[] args)
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

        TurnRadarLeft(360);
        while (true)
        {
            SetTurnRadarLeft(720);

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
        SetTurnRadarLeft(360);
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

        Console.WriteLine("Tembok terdekat: " + minEntry.Key);

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
        SetTurnRadarLeft(360);
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
                    if (alignHeading != 90)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToTopWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);


                }

                // JIKA UP MEPET
                else
                {
                    if (alignHeading == 90)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToBottomWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);


                }
            }

            // Kasus DOWN
            else
            {

                // Jika DOWN TIDAK MEPET
                if (distanceToBottomWall > SafeDistance)
                {
                    if (alignHeading != 270)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToBottomWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);

                }

                // Jika DOWN MEPET
                else
                {
                    if (alignHeading == 270)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToTopWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);
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
                    if (alignHeading != 180)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToLeftWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);

                }

                // JIKA LEFT MEPET
                else
                {
                    if (alignHeading == 180)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToRightWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);

                }
            }

            // Kasus RIGHT
            else
            {
                // JIKA RIGHT TIDAK MEPET
                if (distanceToRightWall > SafeDistance)
                {
                    if (alignHeading != 0)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToRightWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);



                }

                // JIKA RIGHT MEPET
                else
                {
                    if (alignHeading == 0)
                    {
                        TurnRight(180);
                    }

                    double deltaDistance = distanceToLeftWall - SafeDistance;
                    double move = rnd.Next(35, (int)deltaDistance);
                    Forward(move);
                }
            }
        }
    }


    public override void OnScannedBot(ScannedBotEvent e)
    {

        double get_radar_angle = RadarBearingTo(e.X, e.Y);
        double get_gun_angle = GunBearingTo(e.X, e.Y);
        double get_body_angle = BearingTo(e.X, e.Y);
        double distance = DistanceTo(e.X, e.Y);


        SetTurnRadarLeft(get_radar_angle);
        SetTurnGunLeft(get_gun_angle);


        if (distance <= 5)
        {

            SetFire(4);

        }
        else if (distance <= 50)
        {

            SetFire(3);

        }
        else if (distance <= 200 && Energy >= 10)
        {

            SetFire(1.7);

            if (Hostile)
            {
                isAligning = false;
            }
            Hostile = false;
        }

        else
        {

            SetFire(0.7);

            if (Hostile)
            {
                isAligning = false;
            }
            Hostile = false;

        }

        if (distance <= 150)
        {
            SetTurnLeft(get_body_angle);
            SetForward(Math.Min(distance / 2, 150));
            Hostile = true;
        }
        Rescan();

    }

    public override void OnHitBot(HitBotEvent e)
    {
        double get_radar_angle = RadarBearingTo(e.X, e.Y);
        double get_gun_angle = GunBearingTo(e.X, e.Y);
        SetTurnRadarLeft(get_radar_angle);
        SetTurnGunLeft(get_gun_angle);

        SetFire(3);

        SetForward(50);
    }

}