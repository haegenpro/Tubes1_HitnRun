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
        // Set colors for visual distinction
        BodyColor = Color.Green;
        TurretColor = Color.DarkGreen;
        RadarColor = Color.DarkRed;
        BulletColor = Color.Yellow;
        ScanColor = Color.LightYellow;

        // Config to match the gun and radar position to the body
        RadarTurnRate = 20;
        GunTurnRate = 20;
        //AddCustomEvent(new NearWallCondition(this));
        
        while (IsRunning)
        {
            SetForward(600);
            TurnRight(90);
            TurnRadarRight(360);
            TurnGunRight(360);
            Go();
        }
    }

    // Checks if the bot is near any arena wall (within a threshold of 50 units)
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
            SetBack(300);
            movingForward = false;
        }
        else
        {
            SetForward(300);
            movingForward = true;
        }
    }

    private void SetDirection()
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
    }

    // When an enemy is scanned, align gun and radar to the target and fire, and align radar to the enemy's next position
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Calculate the angle to the enemy
        double angleToEnemy = DirectionTo(e.X, e.Y);
        double distance = DistanceTo(e.X, e.Y);
        double enemyXNext = e.X + e.Speed * Math.Cos(angleToEnemy * Math.PI / 180);
        double enemyYNext = e.Y + e.Speed * Math.Sin(angleToEnemy * Math.PI / 180);
        double angleToEnemyNext = DirectionTo(enemyXNext, enemyYNext);
        // Calculate the relative angle for the gun.       
        double gunTurn = CalcGunBearing(angleToEnemyNext);
        TurnGunLeft(gunTurn);
        double radarTurn = CalcRadarBearing(angleToEnemyNext);
        TurnRadarLeft(radarTurn);
        if (GunHeat == 0 && Energy > 3)
        {
            if (distance < 10) Fire(3);
            else{
                if (Energy < 10) Fire(0.5);
                else if (Energy < 20) Fire(1);
                else Fire(3);
            }
        }
        /*// Move towards the enemy
        double turn = NormalizeAngle(angleToEnemy - Direction);
        TurnRight(turn);
        SetForward(1000);*/
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
            Fire(Energy < 20 ? 1 : 3);
        }
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

    // Helper method: normalizes an angle to the range [-180, 180]
    private double NormalizeAngle(double angle)
    {
        angle %= 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
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
