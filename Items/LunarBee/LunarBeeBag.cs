using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class LunarBeeBag : ModItem
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
            item.width = 40;
            item.height = 40;
            item.rare = 1;
            item.expert = true;
        }
        public override int BossBagNPC => mod.NPCType<NPCs.LunarBee.LunarBee>();

        public override bool CanRightClick()
        {
            return true;
        }

        public override void OpenBossBag(Player player)
        {
            player.QuickSpawnItem(mod.ItemType("SynthesizedLunaesia"), Main.rand.Next(20, 29));
            player.QuickSpawnItem(mod.ItemType("LunarFlask"), 1);
            if (Main.rand.NextFloat() < Loot.MaskChance)
                player.QuickSpawnItem(mod.ItemType("LunarBeeMask"));
        }
    }
}