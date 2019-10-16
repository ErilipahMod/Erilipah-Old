using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Accessories
{
    public class TheFuckIsThis : NewModItem
    {
        protected override string DisplayName => "Bizarre Contraption";
        protected override string Tooltip => "Increased life and mana regen, increased move speed" +
            "\nGives light at the cost of higher detection" +
            "\nIncreases pickup range of all items" +
            "\n'Okay. But why?'";
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 82, 60 };
        protected override int Rarity => 2;
        protected override int? Value => 20000;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            a(r, ItemType<Electromagnet>());
            a(r, ItemType<Lightbulb>());
            a(r, ItemType<PowerCellCluster>());
            a(r, ItemType<DuctTape>());

            b(r, TileID.TinkerersWorkbench);
            c(r, "Erilipah:CopperBars", 8);
            d(r);
        }

        private void a(ModRecipe r, int i, int s = 1) => r.AddIngredient(i, s);
        private void b(ModRecipe r, int t) => r.AddTile(t);
        private void c(ModRecipe r, string n, int s) => r.AddRecipeGroup(n, s);

        private void d(ModRecipe r)
        {
            r.SetResult(this);
            r.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.lifeRegen += 5;
            player.manaRegen += 5;
            player.moveSpeed += 0.08f;

            Lighting.AddLight(player.Center, 1.2f, 1.25f, 1f);
            player.aggro += 2;

            player.GetModPlayer<ErilipahPlayer>().extraItemReach += 4 * 16;
        }
    }
    public class ExtItemReach : GlobalItem
    {
        public override void GrabRange(Item item, Player player, ref int grabRange) => grabRange += player.GetModPlayer<ErilipahPlayer>().extraItemReach;
    }
}
