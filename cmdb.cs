using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;

public sealed class CmdPermissionBypasser : Plugin
{
    public override string name { get { return "CmdPermissionBypasser"; } }
    public override string MCGalaxy_Version { get { return "1.9.4.9"; } }
    
    private static Dictionary<string, string> commandPasswords = new Dictionary<string, string>();
    private static Random random = new Random();

    public override void Load(bool startup)
    {
        Command.Register(new CmdCMDB());
    }
    
    public override void Unload(bool shutdown)
    {
        Command.Unregister(Command.Find("CMDB"));
    }
    
    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] buffer = new char[5];
        for (int i = 0; i < 5; i++)
        {
            buffer[i] = chars[random.Next(chars.Length)];
        }
        return new string(buffer);
    }
    
    private class CmdCMDB : Command
    {
        public override string name { get { return "CMDB"; } }
        public override string type { get { return "mod"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message)
        {
            string[] args = message.Split(' ');
            if (args.Length < 2)
            {
                p.Message("&cUsage: /cmdb [genpass/delpass] [command] OR /cmdb [password] [command]");
                return;
            }

            if (args[0].ToLower() == "genpass")
            {
                string cmdName = args[1].ToLower();
                if (Command.Find(cmdName) == null)
                {
                    p.Message("&cThat command does not exist!");
                    return;
                }
                string password = GeneratePassword();
                commandPasswords[cmdName] = password;
                p.Message("&aGenerated password for /{0}: &b{1}", cmdName, password);
                return;
            }
            
            if (args[0].ToLower() == "delpass")
            {
                string cmdName = args[1].ToLower();
                if (commandPasswords.ContainsKey(cmdName))
                {
                    commandPasswords.Remove(cmdName);
                    p.Message("&aRemoved password protection for /{0}", cmdName);
                }
                else
                {
                    p.Message("&cNo password found for /{0}", cmdName);
                }
                return;
            }
            
            string passwordAttempt = args[0];
            string commandAttempt = args[1].ToLower();
            
            string correctPassword;
            if (commandPasswords.TryGetValue(commandAttempt, out correctPassword))
            {
                if (passwordAttempt == correctPassword)
                {
                    string newMessage = message.Substring(passwordAttempt.Length + 1);
                    Command.Find(commandAttempt).Use(p, newMessage);
                }
                else
                {
                    p.Message("&cIncorrect password for /{0}", commandAttempt);
                }
            }
            else
            {
                p.Message("&cNo password protection exists for /{0}", commandAttempt);
            }
        }
        
        public override void Help(Player p)
        {
            p.Message("&T/cmdb genpass [command] - Generates a password to bypass a command's permission.");
            p.Message("&T/cmdb delpass [command] - Removes the password for a command.");
            p.Message("&T/cmdb [password] [command] - Executes a command using the generated password.");
        }
    }
}
