using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Accessories
{
    [AutoloadEquip(EquipType.Waist)]
    public class PowerCellCluster : NewModItem
    {
        protected override string Tooltip => "Increases life and mana regeneration" +
            "\nIncreases movement speed";
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 28, 32 };
        protected override int Rarity => 1;
        protected override int? Value => 10000;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            a(r, ItemType<Sacracite.SacraciteIngot>(), 6);
            a(r, ItemType<Sacracite.SacraciteCore>(), 4);
            a(r, ItemID.FallenStar, 4);
            b(r, TileID.Anvils);
            c(r, "Erilipah:SilverBars", 6);
            c(r, "Erilipah:CopperBars", 12);
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
            player.lifeRegen += 4;
            player.manaRegen += 4;
            player.moveSpeed += 0.08f;
        }
    }
}
