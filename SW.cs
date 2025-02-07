using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;

namespace Plugins {
    public class SkyWarsPlugin : Plugin {
        public override string name { get { return "SkyWars"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.4"; } }
        
        public static SkyWarsPlugin Instance;
        static Dictionary<string, string> playerTeams = new Dictionary<string, string>();
        static Dictionary<string, int> deathCount = new Dictionary<string, int>();  // Track player deaths

        public override void Load(bool startup) {
            Instance = this;
            Command.Register(new CmdSkyWars());
            // Register the player disconnect event
            OnPlayerDisconnectEvent.Register(new OnPlayerDisconnect(OnPlayerLeave), Priority.High, true);
        }

        public override void Unload(bool shutdown) {
            Instance = null;
            Command.Unregister(Command.Find("SkyWars"));
            OnPlayerDisconnectEvent.Unregister(new OnPlayerDisconnect(OnPlayerLeave));
        }

        static void OnPlayerLeave(Player p, string reason) {
            if (playerTeams.ContainsKey(p.name)) {
                playerTeams.Remove(p.name);
            }
            if (deathCount.ContainsKey(p.name)) {
                deathCount.Remove(p.name);  // Clean up death tracking on disconnect
            }
        }

        // This method handles a death caused by a player (for example, when a player kills another)
        // You indicated the following logic:
        //   Die(victim, 4);
        //   string deathMessage = p.color + p.name + " %ekilled " + victim.color + victim.name + "%e.";
        //   foreach( Player pl in PlayerInfo.Online.Items) { ... pl.Message(deathMessage); }
        // Here, p is the killer and victim is the one who died.
        public static void HandleDeath(Player killer, Player victim) {
            // Record the victim's death for tracking:
            TrackDeath(victim);
            
            // Kill the victim:
            Die(victim, 4);
            
            // Construct the death message:
            string deathMessage = killer.color + killer.name + " %ekilled " + victim.color + victim.name + "%e.";
            
            // Broadcast the death message to all players on the same level as either the killer or the victim:
            foreach (Player pl in PlayerInfo.Online.Items) {
                if (killer.level == pl.level || victim.level == pl.level) {
                    pl.Message(deathMessage);
                }
            }
            
            // Announce the kill globally
            string announceMessage = string.Format("/announce global %3{0} %cKilled %4{1}", killer.name, victim.name);
            Command.Find("announce global").Use(killer, announceMessage);
        }

        // Record a player's death in the death count dictionary.
        public static void TrackDeath(Player p) {
            if (!deathCount.ContainsKey(p.name)) {
                deathCount[p.name] = 0;
            }
            deathCount[p.name]++;
            // (Optionally, log the death count to a file or database)
        }

        // Example Die method – you may already have one in your survival plugin.
        // This example simply calls p.HandleDeath() and resets the player's state.
        public static void Die(Player p, ushort reason = 4) {
            p.HandleDeath(reason, immediate: true);
            // (Optionally, reinitialize the player's state after death)
            // For example: SurvivalPlugin.InitPlayer(p);
        }

        public static void AssignTeam(Player p, string team) {
            if (playerTeams.ContainsKey(p.name)) {
                p.Message("&cYou are already in a team!");
                return;
            }
            playerTeams[p.name] = team;
            p.Message("&aYou have joined the {0} team!", team);
            TeleportToTeamSpawn(p, team);
            Command.Find("color").Use(p, "-own " + GetTeamColor(team));
        }

        public static void QuitTeam(Player p) {
            if (!playerTeams.ContainsKey(p.name)) {
                p.Message("&cYou are not in a team!");
                return;
            }
            playerTeams.Remove(p.name);
            p.Message("&cYou have left your team.");
            Command.Find("color").Use(p, "-own white");
        }

        public static void TeleportToTeamSpawn(Player p, string team) {
            int x = 250, y = 101, z = 250;
            if (team == "red") { x = 50;  y = 101; z = 50; }
            else if (team == "blue") { x = 150; y = 101; z = 50; }
            else if (team == "yellow") { x = 50;  y = 101; z = 150; }
            else if (team == "green") { x = 150; y = 101; z = 150; }
            var pos = p.Pos;
            pos.X = (ushort)(x * 32);
            pos.Y = (ushort)(y * 32);
            pos.Z = (ushort)(z * 32);
            p.Pos = pos;
            p.SendPos(Byte.MaxValue, p.Pos, p.Rot);
        }

        public static string GetTeamColor(string team) {
            switch (team) {
                case "red": return "red";
                case "blue": return "blue";
                case "yellow": return "yellow";
                case "green": return "green";
                default: return "white";
            }
        }
    }

    public class CmdSkyWars : Command {
        public override string name { get { return "SkyWars"; } }
        public override string type { get { return "game"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public override bool museumUsable { get { return true; } }

        public override void Use(Player p, string message) {
            if (message == "") {
                Help(p);
                return;
            }
            string cmd = message.ToLower();
            if (cmd == "start") {
                Command.Find("PVP").Use(p, "add " + p.level.name);
                Command.Find("announce").Use(p, "global &aSkyWars Has Started!");
                Command.Find("map").Use(p, "motd -hax -push +thirdperson +hold");
                Command.Find("map").Use(p, "deletable");
                p.Message("&aSkyWars has started! Fight to the last player!");
                return;
            }
            if (cmd == "stop") {
                Command.Find("PVP").Use(p, "del " + p.level.name);
                Command.Find("RESTORE").Use(p, "17 " + p.level.name);
                Command.Find("announce").Use(p, "global &cSkyWars Has Ended!");
                Command.Find("map").Use(p, "deletable");
                Command.Find("map").Use(p, "motd -hax -push +thirdperson +hold -inventory");  

                p.Message("&cSkyWars has ended.");
                return;
            }
            if (cmd == "spectate") {
                p.Message("&7You are now a spectator.");
                var pos = p.Pos;
                pos.X = (ushort)(110 * 32);
                pos.Y = (ushort)(70 * 32);
                pos.Z = (ushort)(110 * 32);
                p.Pos = pos;
                p.SendPos(Byte.MaxValue, p.Pos, p.Rot);
                return;
            }
            if (cmd == "quit") {
                SkyWarsPlugin.QuitTeam(p);
                return;
            }
            if (cmd == "red" || cmd == "blue" || cmd == "yellow" || cmd == "green") {
                SkyWarsPlugin.AssignTeam(p, cmd);
                return;
            }
            p.Message("&cInvalid command. Use &a/SkyWars [start|stop|red|blue|yellow|green|spectate|quit]");
        }

        public override void Help(Player p) {
            p.Message("&T/SkyWars [start|stop|red|blue|yellow|green|spectate|quit]");
            p.Message("&HStarts/stops SkyWars, joins a team, or quits a team. Help For Map Makers: red spawn 50 101 50, blue spawn 150 101 50, yellow spawn 50 101 150, green spawn 150 101 150, default spawn 250 101 250 on 300 300 300 map");
        }
    }
}
