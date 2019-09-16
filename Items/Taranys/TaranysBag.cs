using Terraria;
using Terraria.ModLoader;
using Erilipah.Items.Taranys;
using Erilipah.Items.ErilipahBiome;

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
            player.QuickSpawnItem(mod.ItemType<PureFlower>(), 10);

            player.QuickSpawnItem(mod.ItemType<ShellChunk>(), Main.rand.Next(6, 15));
            player.QuickSpawnItem(mod.ItemType<MadnessFocus>(), Main.rand.Next(15, 23));

            for (int i = 0; i < 2; i++)
            {
                switch (Main.rand.Next(4))
                {
                    default: player.QuickSpawnItem(mod.ItemType<TyrantEye>()); break;
                    case 1: player.QuickSpawnItem(mod.ItemType<VoidSpike>()); break;
                    case 2: player.QuickSpawnItem(mod.ItemType<TorchOfSoul>()); break;
                    case 3: player.QuickSpawnItem(mod.ItemType<ScepterOfEternalAbyss>()); break;
                }
            }

            // TODO: Boss mask
            //if (Main.rand.NextFloat() < Loot.MaskChance)
            //    player.QuickSpawnItem(mod.ItemType("LunarBeeMask"));
        }
    }
}