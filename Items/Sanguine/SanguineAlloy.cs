using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguineAlloy : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.maxStack = 999;
            item.value = Item.buyPrice(0, 0, 5, 0);
            item.rare = 3;
        }

        public override bool CanUseItem(Player player)
        { return false; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Alloy");
            Tooltip.SetDefault("'The refined form of this material is flexible and sturdy'" +
                "\n'Its red shine hungers for life'");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SanguineTileItem"), 5);
            recipe.AddIngredient(ItemID.Bone, 5);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
