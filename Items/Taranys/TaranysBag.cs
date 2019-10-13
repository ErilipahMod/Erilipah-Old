using Erilipah.Items.ErilipahBiome;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class TaranysBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Treasure Bag");
            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
        }

        public override void SetDefaults()
        {
            item.maxStack = 999;
            item.consumable = true;
            item.width = 36;
            item.height = 34;
            item.rare = 1;
            item.expert = true;
        }

        public override int BossBagNPC => mod.NPCType<NPCs.Taranys.Taranys>();

        public override bool CanRightClick()
        {
            return true;
        }

        public override void OpenBossBag(Player player)
        {
            player.QuickSpawnItem(mod.ItemType<VoidFlower>(), 10);

            player.QuickSpawnItem(mod.ItemType<ShellChunk>(), Main.rand.Next(6, 15));
            player.QuickSpawnItem(mod.ItemType<MadnessFocus>(), Main.rand.Next(15, 23));

            List<int> types = new List<int>() {
                mod.ItemType<TyrantEye>(),
                mod.ItemType<VoidSpike>(),
                mod.ItemType<TorchOfSoul>(),
                mod.ItemType<ScepterOfEternalAbyss>(),
                mod.ItemType<LEECH>()
            };
            for (int i = 0; i < 2; i++)
            {
                int chosen = Main.rand.Next(types);
                player.QuickSpawnItem(chosen);
                types.Remove(chosen);
            }

            //if (Main.rand.NextFloat() < Loot.MaskChance)
            //    player.QuickSpawnItem(mod.ItemType("LunarBeeMask"));
        }
    }
}