using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    internal class BioluminescentSinew : ModItem
    {
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 28;
            item.height = 22;

            item.value = 100;
            item.rare = ItemRarityID.Orange;
        }
    }
}
