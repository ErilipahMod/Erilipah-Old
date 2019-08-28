using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories
{
    public class Lightbulb : NewModItem
    {
        protected override string Tooltip => "Gives bright light at the cost of higher detection";
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 24, 42 };
        protected override int Rarity => 1;
        protected override int? Value => 5000;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            c(r, "IronBar", 4);
            a(r, ItemID.Glass, 8);
            b(r, TileID.WorkBenches);
            c(r, Erilipah.Copper, 5);
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
            Lighting.AddLight(player.Center, 1f, 1f, 0.8f);
            player.aggro += 2;
        }
    }
}
