using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class SoilComposite : ModItem
    {
        public override void SetDefaults()
        {
            item.width = item.height = 24;
            item.rare = 0;
            item.maxStack = 999;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.DirtBlock, 10);
            r.AddIngredient(mod.ItemType("EarthenClump"), 2);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
