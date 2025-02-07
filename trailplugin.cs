//pluginref goodlyeffects.dll
using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Tasks;

namespace TrailPlugin {
    public sealed class TrailPlugin : Plugin {
        public override string name { get { return "TrailPlugin"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.5"; } }
        public override string creator { get { return "Blue 3dx"; } }

        private static Dictionary<Player, string> activeTrails = new Dictionary<Player, string>();
        private static SchedulerTask trailTask;

        public override void Load(bool startup) {
            Command.Register(new CmdTrail());
            trailTask = Server.MainScheduler.QueueRepeat(SpawnTrailEffects, null, TimeSpan.FromMilliseconds(100));
        }

        public override void Unload(bool shutdown) {
            Command.Unregister(Command.Find("Trail"));
            Server.MainScheduler.Cancel(trailTask);
            activeTrails.Clear();
        }

        private static void SpawnTrailEffects(SchedulerTask task) {
            foreach (var kvp in activeTrails) {
                Player player = kvp.Key;
                string effectName = kvp.Value;

                if (player.Session == null) {
                    activeTrails.Remove(player);
                    continue;
                }

                float x = player.Pos.X / 32f;
                float y = (player.Pos.Y - Entities.CharacterHeight) / 32f;
                float z = player.Pos.Z / 32f;

                // Make the effect visible to all players in the same level
                foreach (Player other in PlayerInfo.Online.Items) {
                    if (other.Level == player.Level) {
                        GoodlyEffects.SpawnEffectFor(other, effectName, x, y, z, 0, 0, 0);
                    }
                }
            }
        }

        private class CmdTrail : Command {
            public override string name { get { return "Trail"; } }
            public override string type { get { return "Fun"; } }

            public override void Use(Player p, string message) {
                if (string.IsNullOrEmpty(message)) {
                    p.Message("&cUsage: /trail <effect>");
                    return;
                }

                string effectName = message.ToLower();

                // Validate the effect name using GoodlyEffects
                if (!GoodlyEffects.effectAtEffectName.ContainsKey(effectName)) {
                    p.Message("&cEffect \"{0}\" does not exist. Use /effect list to see available effects.", effectName);
                    return;
                }

                if (activeTrails.ContainsKey(p)) {
                    activeTrails.Remove(p);
                    p.Message("&eTrail disabled.");
                } else {
                    activeTrails[p] = effectName;
                    p.Message("&aTrail enabled with effect: {0}", effectName);
                }
            }

            public override void Help(Player p) {
                p.Message("&T/Trail <effect>");
                p.Message("&HEnables a trail effect that follows you.");
                p.Message("&HUse /effect list to see available effects.");
            }
        }
    }
}
