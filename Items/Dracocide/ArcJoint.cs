using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class ArcJoint : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arc Coil");
            Tooltip.SetDefault("A clean coil used to connect two arcs\n");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 14;
            item.height = 28;

            item.value = Item.sellPrice(0, 0, 15, 0);
            item.rare = ItemRarityID.Orange;
        }
    }
}
