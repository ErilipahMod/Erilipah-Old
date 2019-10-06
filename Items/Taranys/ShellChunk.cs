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
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(this, 7);
            r.AddTile(mod.TileType<Altar>());
            r.SetResult(mod.ItemType<TorchOfSoul>());
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(this, 6);
            r.AddTile(mod.TileType<Altar>());
            r.SetResult(mod.ItemType<TyrantEye>());
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(this, 5);
            r.AddTile(mod.TileType<Altar>());
            r.SetResult(mod.ItemType<VoidSpike>());
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(this, 7);
            r.AddTile(mod.TileType<Altar>());
            r.SetResult(mod.ItemType<ScepterOfEternalAbyss>());
            r.AddRecipe();
        }
    }
}
