using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class InfectionModule : ModItem
    {
        public override void SetDefaults()
        {
            item.width = item.height = 20;
            item.rare = ItemRarityID.Green;
            item.maxStack = 999;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystalline Infection Module");
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(ItemType<CrystallineTileItem>(), 6);
            r.AddTile(mod.TileType("ShadaineCompressorTile"));
            r.SetResult(this, 1);
            r.AddRecipe();
        }

    }
}
