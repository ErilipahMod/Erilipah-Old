using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline
{
    public class CrystallinePickaxe : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 32;

            item.damage = 6;
            item.melee = true;
            item.useTime = 12;
            item.useAnimation = 12;
            item.useStyle = 1;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.autoReuse = true;

            item.rare = 2;
            item.value = Terraria.Item.sellPrice(0, 1, 00);
            item.pick = 60;
        }

        public override void SetStaticDefaults() => Tooltip.SetDefault("Able to mine Sanguine Ore");

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "InfectionModule", 2);
            r.AddTile(mod, "ShadaineCompressorTile");
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
