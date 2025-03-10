using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using static System.Math; // Allows you to call Sqrt, Sin, Cos directly

// ------------------------------------------------------------------
// WildBot
// ------------------------------------------------------------------
// This bot fires 0.5 bullets using a linear projection of the enemy position, maximizing the number of bullets fired.
// ------------------------------------------------------------------
public class WildBot : Bot
{
    bool movingForward;

    static void Main()
    {
        new WildBot().Start();
    }
    WildBot() : base(BotInfo.FromFile("WildBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        TurretColor = Color.FromArgb(0x00, 0x96, 0x32);   // green
        RadarColor = Color.FromArgb(0x00, 0x64, 0x64);    // dark cyan
        BulletColor = Color.FromArgb(0x00, 0xC8, 0x00);
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);     // light red
        movingForward = true;

        Random rand = new Random();
        while (IsRunning)
        {
            if (movingForward)
                Forward(1000);
            else
                Back(1000);
            WaitFor(new TurnCompleteCondition(this));
            int random = rand.Next(0, 8);
            if (random == 0)
                SetTurnRight(45);
            else if (random == 1)
                SetTurnLeft(45);
            else if (random == 2)
                SetTurnRight(90);
            else if (random == 3)
                SetTurnLeft(90);
            else if (random == 4)
                SetTurnRight(180);
            else if (random == 5)
                SetTurnLeft(180);
            else if (random == 6)
                SetTurnRight(360);
            else if (random == 7)
                SetTurnLeft(360);
            TurnGunRight(360);
            TurnRadarRight(360);
        }
    }
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }
    public void ReverseDirection()
    {
        movingForward = !movingForward;
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
        SmartFire(e);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        Back(1);
        SmartFire(e);
        Back(100);
    }
    private void SmartFire(ScannedBotEvent e)
    {
        double damage;
        double distance = Sqrt((e.X - X) * (e.X - X) + (e.Y - Y) * (e.Y - Y));
        if (distance < 25 && Energy > 25){
            damage = 3;
        } else{
            damage = 0.5;
        }
        double bulletspeed = 20 - damage * 3;
        double velocityx = e.Speed * Cos(e.Direction);
        double velocityy = e.Speed * Sin(e.Direction);
        double angle = CalculateLeadAngle(X, Y, e.X, e.Y, velocityx, velocityy, bulletspeed);
        double angleDiff = (angle - GunDirection) % 360;
        if (angleDiff > 180) angleDiff -= 360;
        if (angleDiff < -180) angleDiff += 360;
        if (angleDiff < 0)
        {
            TurnGunLeft(-angleDiff);
        }
        else
        {
            TurnGunRight(angleDiff);
        }
        Fire(damage);
    }
    private void SmartFire(HitBotEvent e)
    {
        double damage;
        double distance = Sqrt((e.X - X) * (e.X - X) + (e.Y - Y) * (e.Y - Y));
        if (distance < 25 && Energy > 25)
        {
            damage = 3;
        }
        else
        {
            damage = 0.5;
        }
        double angle = CalculateLeadAngle(X, Y, e.X, e.Y);
        double angleDiff = (angle - GunDirection) % 360;
        if (angleDiff > 180) angleDiff -= 360;
        if (angleDiff < -180) angleDiff += 360;
        if (angleDiff < 0)
        {
            TurnGunLeft(-angleDiff);
        }
        else
        {
            TurnGunRight(angleDiff);
        }
        Fire(damage);
    }

    private double CalculateLeadAngle(double thisX, double thisY, double targetX, double targetY, double targetVx, double targetVy, double bulletSpeed)
    {
        // Angle from shooter to target (line-of-sight)
        double angleToTarget = Atan2(targetY - thisY, targetX - thisX);
        // Angle of the target's velocity vector
        double targetVelocityAngle = Atan2(targetVy, targetVx);
        double phi = targetVelocityAngle - angleToTarget;
        // Normalize phi to the range [-π, π]
        phi = ((phi + PI) % (2 * PI)) - PI;
        // Calculate the target's speed (magnitude of the velocity vector)
        double targetSpeed = Sqrt(targetVx * targetVx + targetVy * targetVy);
        double leadAngle = Atan2(targetSpeed * Sin(phi),
                                 bulletSpeed - targetSpeed * Cos(phi));
        return leadAngle * 180.0 / PI; // result in degrees
    }

    private double CalculateLeadAngle(double thisX, double thisY, double targetX, double targetY)
    {
        double angleToTarget = Atan2(targetY - thisY, targetX - thisX);
        return angleToTarget * 180.0 / PI; // convert to degrees
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        return bot.TurnRemaining == 0;
    }
}