using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineMallet : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 34;

            item.damage = 8;
            item.knockBack = 0.5f;
            item.melee = true;

            item.useTime = 20;
            item.useAnimation = 20;
            item.useStyle = 1;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.autoReuse = true;

            item.rare = 2;
            item.hammer = 55;
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
