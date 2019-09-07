using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class SoulRubble : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("'An abhorrent and ectoplasmic lump of matter'");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 38;
            item.height = 28;

            item.value = 1250;
            item.rare = ItemRarityID.LightRed;
        }
    }
}
