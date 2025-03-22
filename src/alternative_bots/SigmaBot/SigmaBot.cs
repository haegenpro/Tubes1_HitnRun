using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class SigmaBot : Bot
{   
    private bool evaluationMode = true;

    static void Main(string[] args)
    {
        new SigmaBot().Start();
    }

    SigmaBot() : base(BotInfo.FromFile("SigmaBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;

        while (IsRunning)
        {
            Forward(100); Back(100); Fire(1);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (evaluationMode)
        {
            
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    /* Read the documentation for more events and methods */
}
