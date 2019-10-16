using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories
{
    public class Electromagnet : NewModItem
    {
        protected override string Tooltip => "Increases pickup range of all items";
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 30, 30 };
        protected override int Rarity => 1;
        protected override int? Value => 15000;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddRecipeGroup("IronBar", 4);
            r.AddRecipeGroup("Erilipah:SilverBars", 4);
            r.AddIngredient(ItemID.MeteoriteBar, 8);

            r.AddTile(TileID.Anvils);
            r.AddRecipeGroup(Erilipah.Copper, 8);

            r.SetResult(this);
            r.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ErilipahPlayer>().extraItemReach += 4 * 16;
        }
    }
}
