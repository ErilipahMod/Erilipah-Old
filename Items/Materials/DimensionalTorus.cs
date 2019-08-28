using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Materials
{
    public class DimensionalTorus : ModItem
    {
        public override string Texture => "Erilipah/TempSprite";
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Can be used to modify the geometric properties of objects\n" +
                "'Don't think about it for too long'");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 38;
            item.height = 28;

            item.value = Item.buyPrice(0, 33, 00, 0);
            item.rare = ItemRarityID.Orange;
        }
    }
}
