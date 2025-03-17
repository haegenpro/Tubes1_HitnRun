using System;
using System.Drawing;
using static System.Math;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

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
        // Set colors for visual distinction
        BodyColor = Color.Green;
        TurretColor = Color.DarkGreen;
        RadarColor = Color.DarkRed;
        BulletColor = Color.Yellow;
        ScanColor = Color.LightYellow;

        // Config to match the gun and radar position to the body
        RadarTurnRate = 20;
        GunTurnRate = 20;
        TurnRate = 10;
        // Main loop: move forward and continuously turn
        while (IsRunning)
        {
            // Keep moving forward a long distance (similar to Crazy.cs and SpinBot.cs)
            SetForward(10000);
            // Slowly turn the body to follow a curved path (inspired by SpinBot)
            SetTurnRight(90);
            if (IsNearWall())
            {
                ReverseDirection();
            }
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
            SetBack(10000);
            movingForward = false;
        }
        else
        {
            SetForward(10000);
            movingForward = true;
        }
    }

    // When an enemy is scanned, align gun and radar to the target and fire, and align radar to the enemy's next position
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // --- Predict Future Positions ---
        double myDirRad = Direction * Math.PI / 180;
        double enemyDirRad = e.Direction * Math.PI / 180;
        double myXNext = X + Speed * Math.Cos(myDirRad);
        double myYNext = Y + Speed * Math.Sin(myDirRad);
        double enemyXNext = e.X + e.Speed * Math.Cos(enemyDirRad);
        double enemyYNext = e.Y + e.Speed * Math.Sin(enemyDirRad);
        
        // Compute the predicted absolute angle from our predicted position to the enemy's predicted position.
        double predictedEnemyAngle = Math.Atan2(enemyYNext - myYNext, enemyXNext - myXNext) * 180 / Math.PI;

        // Calculate how far off our body is from facing the predicted enemy angle.
        double bodyError = NormalizeAngle(predictedEnemyAngle - Direction);
        
        // If our body isn't aligned within a threshold (case b):
        if (Math.Abs(bodyError) > 10)
        {
            // Turn the body toward the enemy.
            if (bodyError > 0)
                TurnRight(bodyError);
            else
                TurnLeft(-bodyError);

            double radarTarget = NormalizeAngle(predictedEnemyAngle - Direction);
            TurnRadarRight(radarTarget);
            return;
        }
        // --- Case c: Body is properly aligned ---
        else
        {
            if (GunHeat == 0)
            {
                double distance = Math.Sqrt(Math.Pow(e.X - X, 2) + Math.Pow(e.Y - Y, 2));
                Fire(distance < 100 ? 3 : 1);
            }
        }
    }

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