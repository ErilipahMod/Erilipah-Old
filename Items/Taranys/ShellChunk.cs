using Erilipah.Biomes.ErilipahBiome.Tiles;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Taranys
{
    internal class ShellChunk : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 22;
            item.rare = 4;
            item.maxStack = 999;
            item.value = 0;
        }

        public override void AddRecipes()
        {
            void ItemRecipe<T>(int chunks) where T : ModItem => SimpleRecipe<T>(TileType<Altar>(), mod.ItemType<ShellChunk>(), chunks);

            ItemRecipe<TyrantEye>(6);
            ItemRecipe<VoidSpike>(7);
            ItemRecipe<ScepterOfEternalAbyss>(7);
            ItemRecipe<LEECH>(6);
        }
    }
}
