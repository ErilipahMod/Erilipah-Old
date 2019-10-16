using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Accessories
{
    public class TrackingConsole : NewModItem
    {
        protected override string DisplayName => "Life Seeker";
        protected override string Tooltip => "Applies slight homing to all thrown projectiles";
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 40, 40 };
        protected override int Rarity => 2;
        protected override int? Value => 18000;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            a(r, ItemType<Crystalline.InfectionModule>(), 3);
            a(r, ItemType<Drone.PowerCoupling>(), 2);
            a(r, ItemID.MeteoriteBar, 3);

            b(r, TileType<Materials.MaterialFabricatorTile>());
            d(r);
        }

        private void a(ModRecipe r, int i, int s = 1) => r.AddIngredient(i, s);
        private void b(ModRecipe r, int t) => r.AddTile(t);

        //void c(ModRecipe r, string n, int s) => r.AddRecipeGroup(n, s);
        private void d(ModRecipe r)
        {
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
