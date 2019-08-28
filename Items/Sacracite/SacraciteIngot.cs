using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteIngot : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 20;
            item.maxStack = 999;
            item.value = Item.buyPrice(0, 0, 5, 0);
            item.rare = 2;
        }

        public override bool CanUseItem(Player player)
        { return false; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sacracite Alloy");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SacraciteTileItem"), 2);
            recipe.AddRecipeGroup("IronBar");
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
