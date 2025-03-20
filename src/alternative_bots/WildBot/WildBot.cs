using System;
using System.Drawing;
using static System.Math;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class WildBot : Bot
{
    bool movingForward;
    private static Random random = new Random();
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
        SetTurnGunRight(Double.PositiveInfinity);
        SetTurnRadarRight(Double.PositiveInfinity);
        //AddCustomEvent(new NearWallCondition(this));
        
        while (IsRunning)
        {
            SetForward(400);
            SetTurnRight(60);
            SetForward(400);
            SetTurnRight(60);
            SetForward(400);
            SetTurnRight(60);
            Go();
        }
    }

    private bool IsNearWall()
    {
        int threshold = 30;
        return (X < threshold || X > ArenaWidth - threshold ||
                Y < threshold || Y > ArenaHeight - threshold);
    }

    // Reverses the bot's movement direction to avoid walls
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

    /*private void SetDirection()
    {
        int num = random.Next(0, 4);
        int targetX = 50, targetY = 50;
        switch (num)
        {
            case 0:
                targetX = 50;
                targetY = ArenaHeight/2;
                break;
            case 1:
                targetX = ArenaWidth/2;
                targetY = 50;
                break;
            case 2:
                targetX = ArenaWidth/2;
                targetY = ArenaHeight - 50;
                break;
            case 3:
                targetX = ArenaWidth - 50;
                targetY = ArenaHeight/2;
                break;
        }
        double angle = DirectionTo(targetX, targetY);
        double turnangle = NormalizeAngle(angle - Direction);
        TurnRight(turnangle);
        movingForward = true;
        SetForward(1000);
    }*/

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Calculate the current distance to the enemy.
        double distance = DistanceTo(e.X, e.Y);
        
        double firePower;
        if (distance < 10)
        {
            firePower = 3;
        }
        else
        {
            if (Energy < 10)
                firePower = 0.5;
            else if (distance > 300)
                firePower = 1;
            else if (distance > 150)
                firePower = 2;
            else
                firePower = 3;
        }
        double bulletSpeed = CalcBulletSpeed(firePower);
        double timeToTarget = distance / bulletSpeed;
        double enemyXPredicted = e.X + e.Speed * Math.Cos(e.Direction * Math.PI / 180) * timeToTarget;
        double enemyYPredicted = e.Y + e.Speed * Math.Sin(e.Direction * Math.PI / 180) * timeToTarget;
        double nextX = e.X + e.Speed * Math.Cos(e.Direction * Math.PI / 180);
        double nextY = e.Y + e.Speed * Math.Sin(e.Direction * Math.PI / 180);
        double thisnextX = X + Speed * Math.Cos(Direction * Math.PI / 180);
        double thisnextY = Y + Speed * Math.Sin(Direction * Math.PI / 180);
        double predictedAngle = DirectionTo(enemyXPredicted, enemyYPredicted);
        double radarLockAngle = DirectionTo(nextX, nextY);
        double gunTurn = CalcGunBearing(predictedAngle);
        if (gunTurn < 0)
        {
            TurnGunRight(-gunTurn);
        }
        else
        {
            TurnGunLeft(gunTurn);
        }
        if (GunHeat == 0 && Energy > 3)
        {
            Fire(firePower);
        }
        double radarTurn = CalcRadarBearing(radarLockAngle);
        if (radarTurn < 0)
        {
            TurnRadarRight(-radarTurn);
        }
        else
        {
            TurnRadarLeft(radarTurn);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // If we hit another bot, turn the radar and gun, to continuously fire
        double angleToEnemy = DirectionTo(e.X, e.Y);
        double gunTurn = CalcGunBearing(angleToEnemy);
        TurnGunLeft(gunTurn);
        double radarTurn = CalcRadarBearing(angleToEnemy);
        TurnRadarLeft(radarTurn);
        if (GunHeat == 0)
        {
            Fire(Energy < 3 ? Energy : 3);
        }
        Forward(50);
    }
    /*public override void OnCustomEvent(CustomEvent e)
    {
        if (e.Condition is NearWallCondition)
        {
            ReverseDirection();
        }
    }
*/

    // When hitting a wall, reverse direction to avoid damage
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }
}
/*
public class NearWallCondition : Condition
{
    private readonly WildBot bot;

    public NearWallCondition(WildBot bot)
    {
        this.bot = bot;
    }
    public override bool Test()
    {
        int threshold = 30;
        return (bot.X < threshold || bot.X > bot.ArenaWidth - threshold ||
                bot.Y < threshold || bot.Y > bot.ArenaHeight - threshold);
    }
}*/
