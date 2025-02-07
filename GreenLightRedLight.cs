using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;

public class RedLightGreenLight : Plugin {
    private static bool gameActive = false;
    private static string currentMap = "";
    private static SchedulerTask gameTask;
    private static SchedulerTask movementCheckTask;
    private static Random rand = new Random();
    private static Dictionary<string, Position> lastPositions = new Dictionary<string, Position>();
    private static Dictionary<string, bool> canMove = new Dictionary<string, bool>();

    public override string name { get { return "RedLightGreenLight"; } }
    public override string creator { get { return "Blue3dx"; } }
    public override string MCGalaxy_Version { get { return "1.9.4.0"; } }

    public static void StartGame(Player p, string mapName) {
        Player[] allPlayers = PlayerInfo.Online.Items;
        int playerCount = 0;

        foreach (Player pl in allPlayers) {
            if (pl.level.name.CaselessEq(mapName)) playerCount++;
        }

        if (playerCount >= 2) {
            Chat.MessageGlobal("&aThe Red Light Green Light game has started on " + mapName + "!");
            gameActive = true;
            currentMap = mapName;

            gameTask = Server.MainScheduler.QueueRepeat(delegate {
                if (!gameActive) return;

                string message = (rand.Next(0, 2) == 0) ? "&aGreen Light!" : "&cRed Light!";
                Chat.MessageGlobal(message);

                if (message == "&cRed Light!") {
                    foreach (Player pl in PlayerInfo.Online.Items) {
                        if (pl.level.name.CaselessEq(mapName)) {
                            canMove[pl.name] = false;
                        }
                    }
                } else {
                    foreach (Player pl in PlayerInfo.Online.Items) {
                        if (pl.level.name.CaselessEq(mapName)) {
                            canMove[pl.name] = true;
                        }
                    }
                }
            }, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(rand.Next(5, 11)));

            movementCheckTask = Server.MainScheduler.QueueRepeat(delegate {
                foreach (Player pl in PlayerInfo.Online.Items) {
                    if (!gameActive || !pl.level.name.CaselessEq(mapName)) continue;

                    if (!canMove.ContainsKey(pl.name)) canMove[pl.name] = true;
                    if (!lastPositions.ContainsKey(pl.name)) lastPositions[pl.name] = pl.Pos;

                    if (!canMove[pl.name] && HasMoved(pl, lastPositions[pl.name])) {
                        pl.HandleDeath(Block.Cobblestone, "You moved during Red Light!");
                        pl.Message("&cYou became a Referee!");
                        Command.Find("ref").Use(null, pl.name);
                    }

                    lastPositions[pl.name] = pl.Pos;
                }
            }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        } else {
            p.Message("&cAt least two players are required to play!");
        }
    }

    public static void StopGame(Player p) {
        if (!gameActive) {
            p.Message("&cNo game is currently running!");
            return;
        }

        gameActive = false;
        currentMap = "";
        Chat.MessageGlobal("&cThe Red Light Green Light game has ended!");

        if (gameTask != null) Server.MainScheduler.Cancel(gameTask);
        if (movementCheckTask != null) Server.MainScheduler.Cancel(movementCheckTask);

        lastPositions.Clear();
        canMove.Clear();
    }

    private static bool HasMoved(Player p, Position lastPos) {
        Position currentPos = p.Pos;
        return currentPos.X != lastPos.X || currentPos.Y != lastPos.Y || currentPos.Z != lastPos.Z;
    }

    private static void CheckForWater(Player p) {
        if (gameActive && p.level.name.CaselessEq(currentMap)) {
            ushort block = p.level.GetBlock((ushort)(p.Pos.BlockX), (ushort)(p.Pos.BlockY - 1), (ushort)(p.Pos.BlockZ));
            if (block == Block.Water) {
                Chat.MessageGlobal("&e" + p.truename + " &ahas won the Red Light Green Light game!");
                gameActive = false;

                if (gameTask != null) Server.MainScheduler.Cancel(gameTask);
                if (movementCheckTask != null) Server.MainScheduler.Cancel(movementCheckTask);

                lastPositions.Clear();
                canMove.Clear();
            }
        }
    }

    public override void Load(bool startup) {
        Command.Register(new GlrlCommand());
    }

    public override void Unload(bool shutdown) {
        Command.Unregister(Command.Find("glrl"));
    }
}

public class GlrlCommand : Command {
    public override string name { get { return "glrl"; } }
    public override string type { get { return "game"; } }
    public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

    public override void Use(Player p, string message) {
        if (message.StartsWith("start ")) {
            string mapName = message.Substring(6).Trim();
            RedLightGreenLight.StartGame(p, mapName);
        } else if (message == "stop") {
            RedLightGreenLight.StopGame(p);
        } else {
            p.Message("&cInvalid usage. Use &a/glrl start <mapname> &cor &a/glrl stop");
        }
    }

    public override void Help(Player p) {
        p.Message("&TStarts or stops a Red Light Green Light game.");
        p.Message("&H/glrl start <mapname>");
        p.Message("&H/glrl stop");
    }
}
