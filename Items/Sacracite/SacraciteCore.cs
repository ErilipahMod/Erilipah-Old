using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteCore : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 14;
            item.height = 20;
            item.maxStack = 999;
            item.value = Item.buyPrice(0, 0, 10, 0);
            item.rare = 2;
        }

        public override bool CanUseItem(Player player)
        { return false; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sacracite Core");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SacraciteTileItem"), 3);
            recipe.AddRecipeGroup("Erilipah:AnyGem", 1);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
