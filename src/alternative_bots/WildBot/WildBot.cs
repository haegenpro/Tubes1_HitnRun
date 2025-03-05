using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Crazy
// ------------------------------------------------------------------
// A sample bot original made for Robocode by Mathew Nelson.
// Ported to Robocode Tank Royale by Flemming N. Larsen.
//
// This bot moves around in a crazy pattern.
// ------------------------------------------------------------------
public class WildBot : Bot
{
    bool movingForward;

    // The main method starts our bot
    static void Main()
    {
        new WildBot().Start();
    }

    // Constructor, which loads the bot config file
    WildBot() : base(BotInfo.FromFile("WildBot.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        TurretColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        RadarColor = Color.FromArgb(0x00, 0x64, 0x64);  // dark cyan
        BulletColor = Color.FromArgb(0x00, 0xC8, 0x00);
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);   // light red
        movingForward = true;

        while (IsRunning)
    {
        if (movingForward)
            Forward(1000);  // move forward 100 units
        else
            Back(1000);     // move backward 100 units

        // Rotate gun (and optionally radar) to scan for opponents.
        TurnGunRight(360);
        TurnRadarRight(360);
        Go();
    }
    }

    // We collided with a wall -> reverse the direction
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    // ReverseDirection: Switch from ahead to back & vice versa
    public void ReverseDirection()
    {
        movingForward = !movingForward;
    }
    
    // We scanned another bot -> fire based on the bot speed
    public override void OnScannedBot(ScannedBotEvent e)
    {
        SmartFire(e);
    }

    // We hit another bot -> back up!
    public override void OnHitBot(HitBotEvent e)
    {
        
    }
    private void SmartFire(ScannedBotEvent e)
    {
        int damage;
        double distance = DistanceTo(e.X, e.Y);
        if (distance > 200 || Energy < 5)
            damage = 1;
        else
            damage = 3;
        
    }
    private void ContinuousFire(ScannedBotEvent e)
    
}

// Condition that is triggered when the turning is complete
public class TurnCompleteCondition : Condition
{
    
}

// Condition that the bot is near a wall
public class NearWallCondition : Condition
{
    public bool NearWall(Bot bot)
    {
        return bot.X < 25 || bot.Y < 25 || bot.BattleFieldWidth - bot.X < 25 ||  bot.BattleFieldHeight - bot.Y < 25;    
    }
    public override void OnNearWall()
    {
        if (NearWall(bot))
        {
            ReverseDirection();
        }
    }
}