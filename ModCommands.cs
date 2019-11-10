#if DEBUG
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah
{
    public class Stats : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "stat";

        public override string Usage
            => "/stat <life|mana|infection|item> [value]";

        public override string Description
            => "Change your permanent stats";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            try
            {
                if (args[0] == "item")
                {
                    caller.Player.HeldItem.SetDefaults();
                    Main.NewText("[c/00ffff:" + caller.Player.HeldItem.Name + "] has called SetDefaults");
                    return;
                }

                float value = float.Parse(args[1]);
                Color color = Color.Lerp(new Color(255, 0, 0), new Color(0, 255, 0), int.Parse(args[1]) / 500f);

                if (args[0] == "mana")
                {
                    caller.Player.statManaMax = (int)value;
                }
                if (args[0] == "life")
                {
                    caller.Player.statLifeMax = (int)value;
                }
                if (args[0] == "infection")
                {
                    caller.Player.I().Infection = value;
                    color = Color.Lerp(new Color(0, 255, 0), Color.MediumVioletRed, value / caller.Player.I().infectionMax);
                }

                Main.NewText("[c/00ffff:" + args[0] + "] has been set to [c/" + color.Hex3() + ":" + value + "]");
            }
            catch
            {
                Main.NewText("Failure in usage. " + Usage);
            }
        }
    }
    public class Modes : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "expert";

        public override string Usage
            => "/expert <true|false>";

        public override string Description
            => "Change the game mode";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                string c = Main.expertMode ? "[c/00ff00:" : "[c/ff0000:";
                Main.NewText("[c/00ffff:expert mode] is set to " + c + Main.expertMode + "]");
                return;
            }
            bool yes = args[0] == "true";
            Main.expertMode = yes;

            string color = yes ? "[c/00ff00:" : "[c/ff0000:";
            Main.NewText("[c/00ffff:expert mode] has been set to " + color + yes + "]");
        }
    }
    public class DownedBosses : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "downed";

        public override string Usage => "/downed <eye|goblins|boss2|skeletron|wall|allMech|plant|golem|cultist|moonlord|all> <true|false>";

        public override string Description => "Change whether a certain boss has been downed";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            bool yes = args[1] == "true";
            if (args[0] == "eye") NPC.downedBoss1 = yes;
            else if (args[0] == "goblins") NPC.downedGoblins = yes;
            else if (args[0] == "boss2") NPC.downedBoss2 = yes;
            else if (args[0] == "skeletron") NPC.downedBoss3 = yes;
            else if (args[0] == "taranys") ErilipahWorld.downedTaintedSkull = yes;
            else if (args[0] == "wall") Main.hardMode = yes;
            else if (args[0] == "allMech") { NPC.downedMechBossAny = yes; NPC.downedMechBoss1 = yes; NPC.downedMechBoss2 = yes; NPC.downedMechBoss3 = yes; }
            else if (args[0] == "plant") NPC.downedPlantBoss = yes;
            else if (args[0] == "golem") NPC.downedGolemBoss = yes;
            else if (args[0] == "cultist") NPC.downedAncientCultist = yes;
            else if (args[0] == "moonlord") NPC.downedMoonlord = yes;
            else if (args[0] == "all")
            {
                NPC.downedBoss1 =
                NPC.downedBoss2 =
                NPC.downedBoss3 =
                Main.hardMode =
                NPC.downedMechBossAny =
                NPC.downedMechBoss1 =
                NPC.downedMechBoss2 =
                NPC.downedMechBoss3 =
                NPC.downedPlantBoss =
                NPC.downedGolemBoss =
                NPC.downedAncientCultist =
                NPC.downedMoonlord =

                NPC.downedHalloweenKing =
                NPC.downedHalloweenTree =
                NPC.downedChristmasIceQueen =
                NPC.downedChristmasSantank =
                NPC.downedChristmasTree =
                NPC.downedFishron =
                NPC.downedGoblins = yes;
            }
            else
            {
                Main.NewText("[c/ff0000:" + args[0] + " is not a valid name nerd]");
                return;
            }

            string color = yes ? "[c/00ff00:" : "[c/ff0000:";
            Main.NewText("[c/00ffff:" + args[0] + " boss downed] has been set to " + color + yes + "]");
        }
    }
    public class UnlockNote : ModCommand
    {
        public override string Command => "unlocknote";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != Main.myPlayer) // Only for local player
                return;

            try
            {
                GetInstance<NoteHandler>().CollectNote(int.Parse(args[0]));
            }
            catch (ArgumentOutOfRangeException)
            {
                Main.NewText("Not a valid note. 0-" + (UI.Notes.NoteUIPages.pageCount - 1));
            }
        }
    }
    public class Set : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "ai";

        public override string Usage
            => "/ai <name|index> <{X}|local{X}> <value>";

        public override string Description
            => "Change an NPC's AI fields";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // Prevent some stupid errors
            if (args.Length < 2)
            {
                Main.NewText("Not enough args.");
                return;
            }

            // Searching for index
            int index = -1;
            if (int.TryParse(args[0], out int _i))
                index = _i;
            else if (Main.npc.FirstOrDefault(npc => NPC.GetFirstNPCNameOrNull(npc.type) == args[0]) != null)
                index = Main.npc.First(npc => NPC.GetFirstNPCNameOrNull(npc.type) == args[0]).whoAmI;
            if (index == -1)
            {
                Main.NewText("[c/ff7000:NPC with the name/index '" + args[0] + "' not found.]");
                return;
            }

            // Display just the value, nothing more
            if (args.Length == 2)
            {
                if (int.TryParse(args[1], out int ai))
                {
                    Main.NewText(
                        string.Format("[c/00ffff:npc.ai][{0}] is set to {1}.", ai, Main.npc[index].ai[ai])
                        );
                }
                else if (int.TryParse(args[1].Remove(5), out ai))
                {
                    Main.NewText(
                        string.Format("[c/00ffff:npc.localAI][{0}] is set to {1}.", ai, Main.npc[index].localAI[ai])
                    );
                }
                return;
            }

            // If all args are provided
            if (!float.TryParse(args[2], out float value))
            {
                Main.NewText("[c/ff7000:" + args[3] + " is not a valid value.]");
                return;
            }
            if (int.TryParse(args[1], out int aiIndex))
            {
                float _oldVal = Main.npc[index].ai[aiIndex];
                Main.npc[index].ai[aiIndex] = value;

                Main.NewText(string.Format(
                    "[c/00ffff:npc.ai][{0}] = {1}; was {2}", aiIndex, value, _oldVal)
                    );
            }
            else if (int.TryParse(args[1].Remove(5), out aiIndex))
            {
                float _oldVal = Main.npc[index].localAI[aiIndex];
                Main.npc[index].localAI[aiIndex] = value;

                Main.NewText(string.Format(
                    "[c/00ffff:npc.localAI][{0}] = {1}; was {2}", aiIndex, value, _oldVal)
                    );
            }
        }
    }
}
#endif