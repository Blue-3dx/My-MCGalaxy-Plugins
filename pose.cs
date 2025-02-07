using System;
using MCGalaxy;
using MCGalaxy.Commands;

namespace MCGalaxy
{
    public class PosePlugin : Plugin
    {
        public override string creator { get { return "Blue3dx"; } }
        public override string name { get { return "Pose"; } }

        public override void Load(bool startup)
        {
            Command.Register(new CmdPose());
        }

        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("pose"));
        }

        // Create the command for /pose
        public class CmdPose : Command2
        {
            public override string name { get { return "pose"; } }
            public override string type { get { return "fun"; } }

            // List of available poses, including the reset pose
            private string[] poses = { "Sit", "RUUUUNNN!", "Reset", "Chibi", "Head", "Giant", "Hello!", "Over There!", "Good Stuff!", "Gun Point", "Float", "Balloon" };

            public override void Use(Player p, string message, CommandData data)
            {
                if (message == "")
                {
                    // Display the list of available poses to the player
                    p.Message("Available poses:");
                    for (int i = 0; i < poses.Length; i++)
                    {
                        p.Message((i + 1) + ". " + poses[i]);
                    }
                    return;
                }

                int poseNumber;
                if (int.TryParse(message, out poseNumber))
                {
                    // Validate the pose number
                    if (poseNumber >= 1 && poseNumber <= poses.Length)
                    {
                        string selectedPose = poses[poseNumber - 1];

                        // Handle the different poses
                        if (selectedPose == "Sit")
                        {
                            // Set the player's model to "sit"
                            Command.Find("model").Use(p, "sit");
                        }
                        else if (selectedPose == "RUUUUNNN!")
                        {
                            // Set the player's model to "swim"
                            Command.Find("model").Use(p, "swim");
                        }
                        else if (selectedPose == "Reset")
                        {
                            // Reset the player's model to "humanoid"
                            Command.Find("model").Use(p, "humanoid");
                        }
                        else if (selectedPose == "Chibi")
                        {
                            // Set the player's model to "chibi"
                            Command.Find("model").Use(p, "chibi");
                        }
                        else if (selectedPose == "Head")
                        {
                            // Set the player's model to "head"
                            Command.Find("model").Use(p, "head");
                        }
                        else if (selectedPose == "Giant")
                        {
                            // Set the player's model to "humanoid:2"
                            Command.Find("model").Use(p, "humanoid:2");
                        }
                        else if (selectedPose == "Hello!")
                        {
                            // Set the player's model to "hello"
                            Command.Find("model").Use(p, "hello");
                        }
                        else if (selectedPose == "Over There!")
                        {
                            // Set the player's model to "overthere"
                            Command.Find("model").Use(p, "overthere");
                        }
                        else if (selectedPose == "Good Stuff!")
                        {
                            // Set the player's model to "goodstuff"
                            Command.Find("model").Use(p, "goodstuff");
                        }
                        else if (selectedPose == "Gun Point")
                        {
                            // Set the player's model to "gunpoint"
                            Command.Find("model").Use(p, "gunpoint");
                        }
                        else if (selectedPose == "Float")
                        {
                            // Set the player's model to "float"
                            Command.Find("model").Use(p, "float");
                        }
                        else if (selectedPose == "Balloon")
                        {
                            // Set the player's model to "balloon"
                            Command.Find("model").Use(p, "balloon");
                        }

                        p.Message("You have selected the pose: " + selectedPose);
                    }
                    else
                    {
                        p.Message("Invalid pose number. Please choose a valid pose number.");
                    }
                }
                else
                {
                    p.Message("Invalid input. Use /pose to see the list of poses.");
                }
            }

            public override void Help(Player p)
            {
                p.Message("%T/pose [number] - %HChanges your pose to the selected one. %cNOTE: SOME POSES WON'T LOOK GOOD SOMETIMES!");
                p.Message("%TAvailable poses: 1. Sit, 2. RUUUNNN!, 3. Reset, 4. Chibi, 5. Head, 6. Giant, 7. Hello!, 8. Over There!, 9. Good Stuff!, 10. Gun Point, 11. Float, 12. Balloon");
            }
        }
    }
}
