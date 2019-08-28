using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class Dracocell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("An intact, liquid-filled cell from within a dracocide drone\n" +
                "Seems volatile");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 38;
            item.height = 28;

            item.value = Item.sellPrice(0, 0, 10, 0);
            item.rare = ItemRarityID.Orange;
        }
    }
}
