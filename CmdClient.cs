using System;
using System.Collections.Generic;
using System.Text;

namespace MCGalaxy.Commands.Info 
{
    public sealed class CmdPClients : Command2 
    {
        public override string name { get { return "PClient"; } }
        public override string shortcut { get { return "Client"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool UseableWhenFrozen { get { return true; } }

        static readonly string Key = "FakeClientName"; // Key for storing fake names
        
        public override void Use(Player p, string message, CommandData data) {
            Dictionary<string, List<Player>> clients = new Dictionary<string, List<Player>>();
            Player[] online = PlayerInfo.Online.Items;
            
            foreach (Player pl in online) 
            {
                if (!p.CanSee(pl, data.Rank)) continue;

                // Use fake client name if it exists
                string appName = pl.Extras.Contains(Key) ? (string)pl.Extras[Key] : pl.Session.ClientName();

                List<Player> usingClient;
                if (!clients.TryGetValue(appName, out usingClient)) {
                    usingClient = new List<Player>();
                    clients[appName] = usingClient;
                }
                usingClient.Add(pl);
            }
            
            List<string> lines = new List<string>();
            lines.Add("Players using:");
            foreach (var kvp in clients) 
            {
                StringBuilder builder = new StringBuilder();
                List<Player> players  = kvp.Value;
                
                for (int i = 0; i < players.Count; i++) 
                {
                    string nick = Colors.StripUsed(p.FormatNick(players[i]));
                    builder.Append(nick);
                    if (i < players.Count - 1) builder.Append(", ");
                }
                lines.Add(string.Format("  {0}: &f{1}", kvp.Key, builder.ToString()));
            }
            p.MessageLines(lines);
        }

        public override void Help(Player p) {
            p.Message("&T/PClient");
            p.Message("&HLists the clients players are using, and who uses which client.");
        }
    }
}
