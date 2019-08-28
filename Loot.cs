using Erilipah.Items.Sanguine;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Erilipah
{
    public class Loot : GlobalNPC
    {
        public const float MaskChance = 14.29f;

        public override void NPCLoot(NPC npc)
        {
            if (npc.type == NPCID.EyeofCthulhu && !ErilipahWorld.sanguineOreSpawned)
            {
                ErilipahWorld.sanguineOreSpawned = true;
                string text = "Sanguine ore disperses throughout the caverns' depths.";
                if (Main.netMode == 0)
                    Main.NewText(text, new Color(255, 20, 00));
                else if (Main.netMode == 2)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(255, 20, 00));

                mod.GetModWorld<ErilipahWorld>().SanguineOre();
            }

            Player player = Main.player.First(p => p.active && p.whoAmI == Main.myPlayer);

            if (player.ZoneUndergroundDesert || player.ZoneDesert)
            {
                DropItem(npc, mod.ItemType<Items.Sacracite.SacraciteTileItem>(), 1, 2, Main.hardMode ? 0.1f : 2.5f, 2);
            }
            if (Main.bloodMoon && (player.ZoneDirtLayerHeight || player.ZoneOverworldHeight))
            {
                DropItem(npc, mod.ItemType<SanguineTileItem>(), 1, 2, Main.hardMode ? 2.5f : 0.1f, 2);
            }
        }

        public static void DropItem(NPC npc, int item, int minValue = 1, int maxValue = 1, float percent = 100, float expertMult = 1)
        {
            DropItem(npc.getRect(), item, minValue, maxValue, percent, expertMult);
        }
        public static void DropItem(Rectangle rect, int item, int minValue = 1, int maxValue = 1, float percent = 100, float expertMult = 1)
        {
            int i = 0;
            float o = Main.expertMode ? expertMult : 1;
            int e = Main.rand.Next((int)(o * minValue), (int)(o * (maxValue + 1)));
            float a = percent * (Main.expertMode ? 2 : 1);
            if (Main.rand.NextFloat() < (percent * a / 100f))
            {
                i = Item.NewItem(rect, item, e);
            }
            if (Main.netMode == 1)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i, o);
            }
        }

        public override bool InstancePerEntity => true;
    }
}
