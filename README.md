# IF2230 Strategi Algoritma

## Overview
This repository contains multiple bots developed for the Robocode game using C#. Each bot leverages a unique greedy algorithm to make quick, locally optimal decisions during battles. With the Robocode.TankRoyale.BotApi, the bots are built to navigate, target, and attack opponents effectively using this API.

## Greedy Algorithm Implementation

### Trishula
- Data Collection: Aggregates data from all detected enemies.
- Position Evaluation: Calculates the average distance to all enemies on a point.
- Optimal Movement: Choose the point that is furthest to all enemies by averaging.

### HitnRunBot
- Ramming: Moves toward the first scanned opponent to ram it.
- Real-Time Prediction: Estimates enemy movement based on their speed and direction.
- Local Decision Making: Adjusts gun and movement commands in real-time to maximize attack effectiveness, firing as frequent as possible.

### Forza_Ferrari
- Data Collection: Aggregates data from all detected enemies.
- Target Evaluation: Calculates the average distance between each enemy and all others.
- Optimal Targeting: Chooses the enemy with the highest average distance as the target, ensuring the bot pursues the most advantageous opponent.

### N1993R
- Target Evaluation: Determines if the target is hostile towards the bot (within a certain distance).
- Optimal Movement: Chooses a random point to turn or interchange between walls. 

## System Requirements and Installation
- Programming Language: C#
- Framework: .NET Framework / .NET Core (depending on your repository configuration)
- IDE/Compiler: Visual Studio or the .NET CLI (command-line interface)
- Dependencies:
  - Robocode.TankRoyale.BotApi
  Ensure that all required libraries and dependencies are correctly installed and referenced in the repository.

## Build and Compilation Instructions

1. Open the Repository:
   - Fork/download this repository.
   - Navigate to the main bot/alternative bots directory via the command line.
   - The repository structure can be seen on the section below.

2. Build the Bot Files:
   - Using Visual Studio: Select Build > Build Solution.
   - Using .NET CLI: Run the following command:
     dotnet build

3. Run the Program:
   - Ensure that the bot configuration files (e.g., "Trishula.json", "Forza_Ferrari.json") are available in the repository directory as required.
   - Using the command line, run the .jar file using this command:
   ```bash
   java -jar robocode-tankroyale-gui-0.30.0.jar
   ```
   - Once inside the app, go to Battle -> StartBattle.
   - Boot all the selected bots that will be tested/played in the match.
   ![Booting](img\image.png)
   - Add all the booted bots before starting the match.

## Repository Structure
```bash
.
├── LICENSE
├── README.md
├── config.properties
├── doc
│   └── laporan.pdf
├── games.properties
├── img
│   └── image.png
├── robocode-tankroyale-gui-0.30.0.jar
├── server.properties
└── src
    ├── alternative_bots
    │   ├── Forza_Ferrari
    │   │   ├── Forza_Ferrari.cmd
    │   │   ├── Forza_Ferrari.cs
    │   │   ├── Forza_Ferrari.csproj
    │   │   ├── Forza_Ferrari.json
    │   │   ├── Forza_Ferrari.sh
    │   │   ├── bin
    │   │   └── obj
    │   ├── HitnRunBot
    │   │   ├── HitnRunBot.cmd
    │   │   ├── HitnRunBot.cs
    │   │   ├── HitnRunBot.csproj
    │   │   ├── HitnRunBot.json
    │   │   ├── HitnRunBot.sh
    │   │   ├── bin
    │   │   └── obj
    │   └── N1993R
    │       ├── N1993R.cmd
    │       ├── N1993R.cs
    │       ├── N1993R.csproj
    │       ├── N1993R.json
    │       ├── N1993R.sh
    │       ├── bin
    │       └── obj
    └── main_bot
        └── Trishula
            ├── Trishula.cmd
            ├── Trishula.cs
            ├── Trishula.csproj
            ├── Trishula.json
            ├── Trishula.sh
            ├── bin
            └── obj
```

## Author
Daniel Pedrosa Wu (13523099)
Steven Owen Liauw (13523103)
Haegen Quinston (13523109)
