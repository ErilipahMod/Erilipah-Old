using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class MalleableShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Malleable Shard");
            Tooltip.SetDefault("A metal scrap from within an Assault Drone\n");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 24;
            item.height = 22;

            item.value = Item.sellPrice(0, 0, 15, 0);
            item.rare = ItemRarityID.Orange;
        }
    }
}
