using Erilipah.Biomes.ErilipahBiome.Tiles;
using Terraria.ModLoader;

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
            void ItemRecipe(int item, int chunks) => mod.SimpleRecipe(item, mod.ItemType<ScepterOfEternalAbyss>(), mod.ItemType<ShellChunk>(), chunks);

            ItemRecipe(mod.ItemType<TyrantEye>(), 6);
            ItemRecipe(mod.ItemType<VoidSpike>(), 7);
            ItemRecipe(mod.ItemType<ScepterOfEternalAbyss>(), 7);
            ItemRecipe(mod.ItemType<LEECH>(), 6);
        }
    }
}
