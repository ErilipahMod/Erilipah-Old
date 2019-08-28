using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineAxe : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 34;

            item.damage = 12;
            item.melee = true;
            item.useTime = 14;
            item.useAnimation = 14;
            item.autoReuse = true;
            item.useStyle = 1;

            item.rare = 2;
            item.axe = 75 / 5;
        }

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
