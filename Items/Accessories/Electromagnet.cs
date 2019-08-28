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

            c(r, "IronBar", 4);
            c(r, "Erilipah:SilverBars", 4);
            a(r, ItemID.MeteoriteBar, 8);

            b(r, TileID.Anvils);
            c(r, Erilipah.Copper, 8);
            d(r);
        }

        private void a(ModRecipe r, int i, int s) => r.AddIngredient(i, s);
        private void b(ModRecipe r, int t) => r.AddTile(t);
        private void c(ModRecipe r, string n, int s) => r.AddRecipeGroup(n, s);

        private void d(ModRecipe r)
        {
            r.SetResult(this);
            r.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ErilipahPlayer>(mod).extraItemReach += 4 * 16;
        }
    }
}
