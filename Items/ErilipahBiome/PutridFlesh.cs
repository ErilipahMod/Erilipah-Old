using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    internal class PutridFlesh : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("'Looks...tasty?'");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 28;
            item.height = 22;

            item.value = 20;
            item.rare = ItemRarityID.White;
        }
    }
}
