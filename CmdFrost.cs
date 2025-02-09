/*
    Copyright 2011 MCForge
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;

namespace MCGalaxy.Commands.Misc {
    public sealed class CmdFrost : Command2 {
        public override string name { get { return "Frost"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            if (!Hacks.CanUseFly(p)) {
                p.Message("You cannot use &T/Frost &Son this map.");
                p.isFlying = false; return;
            }
            
            p.isFlying = !p.isFlying;
            if (!p.isFlying) return;
            
            p.Message("Frost Walker Enchant Activated");
            
            FlyState state = new FlyState();
            state.player = p;
            SchedulerTask task = new SchedulerTask(FlyCallback, state, TimeSpan.Zero, true);
            p.CriticalTasks.Add(task);
        }
        
        class FlyState {
            public Player player;
            public Position oldPos = default(Position);
            public List<Vec3U16> lastIce = new List<Vec3U16>();
            public List<Vec3U16> iceCoords = new List<Vec3U16>();
        }
        
        static void FlyCallback(SchedulerTask task) {
            FlyState state = (FlyState)task.State;
            Player p = state.player;
            if (state.player.isFlying) { DoFly(state); return; }
            
            foreach (Vec3U16 pos in state.lastIce) {
                p.SendBlockchange(pos.X, pos.Y, pos.Z, Block.Air);
            }            
            p.Message("Stopped flying");
            task.Repeating = false;
        }

        static void DoFly(FlyState state) {
            Player p = state.player;
            if (p.Pos == state.oldPos) return;

            int x = p.Pos.BlockX, z = p.Pos.BlockZ;
            int y = (p.Pos.Y - 60) / 32;

            for (int yy = y - 1; yy <= y; yy++)
                for (int zz = z - 2; zz <= z + 2; zz++)
                    for (int xx = x - 2; xx <= x + 2; xx++)
            {
                Vec3U16 pos;
                pos.X = (ushort)xx; pos.Y = (ushort)yy; pos.Z = (ushort)zz;
                if (p.level.IsAirAt(pos.X, pos.Y, pos.Z)) state.iceCoords.Add(pos);
            }
            
            foreach (Vec3U16 P in state.iceCoords) {
                if (state.lastIce.Contains(P)) continue;
                state.lastIce.Add(P);
                p.SendBlockchange(P.X, P.Y, P.Z, Block.Ice);
            }
            
            for (int i = 0; i < state.lastIce.Count; i++) {
                Vec3U16 P = state.lastIce[i];
                if (state.iceCoords.Contains(P)) continue;
                
                p.RevertBlock(P.X, P.Y, P.Z);
                state.lastIce.RemoveAt(i); i--;
            }
            
            state.iceCoords.Clear();
            state.oldPos = p.Pos;
        }
        
        public override void Help(Player p) {
            string name = Group.GetColoredName(LevelPermission.Operator);
            p.Message("&T/Fly");
            p.Message("&HCreates a ice platform underneath you that moves with you.");
            p.Message("&H  May not work if you have high latency.");
            p.Message("&H  Cannot be used on maps which have -hax in their motd. " +
                           "(unless you are {0}&H+ and the motd has +ophax)", name);
        }
    }
}
