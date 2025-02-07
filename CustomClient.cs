using System;
using MCGalaxy;
using MCGalaxy.Commands;

public class ClientNameChanger : Plugin {
    public override string name { get { return "ClientNameChanger"; } }
    public override string MCGalaxy_Version { get { return "1.9.3.9"; } }

    static readonly string Key = "FakeClientName";

    public override void Load(bool startup) {
        Command.Register(new CmdClientName());
    }

    public override void Unload(bool shutdown) {
        Command.Unregister(Command.Find("ClientName"));
    }

    class CmdClientName : Command2 {
        public override string name { get { return "ClientName"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0) {
                p.Message("%cUsage: /clientname edit [name] or /clientname del");
                return;
            }

            string[] args = message.Split(' ');
            if (args.Length < 1) { p.Message("%cInvalid arguments!"); return; }

            if (args[0].CaselessEq("edit")) {
                if (args.Length < 2) { p.Message("%cYou must specify a name!"); return; }
                string newClient = message.Substring(5).Trim();

                if (newClient.Length > 20) {
                    p.Message("%cClient name is too long! Max 20 characters.");
                    return;
                }

                p.Extras[Key] = newClient;
                p.Message("%aYour client name has been changed to: %f" + newClient);

            } else if (args[0].CaselessEq("del")) {
                if (p.Extras.Contains(Key)) {
                    p.Extras.Remove(Key);
                    p.Message("%aYour client name has been reset.");
                } else {
                    p.Message("%cYou don’t have a custom client name set!");
                }
            } else {
                p.Message("%cInvalid subcommand! Use /clientname edit [name] or /clientname del");
            }
        }

        public override void Help(Player p) {
            p.Message("%T/ClientName edit [name]");
            p.Message("%H - Changes how your client name appears in /clients and /pclients.");
            p.Message("%T/ClientName del");
            p.Message("%H - Restores your default client name.");
        }
    }
}
