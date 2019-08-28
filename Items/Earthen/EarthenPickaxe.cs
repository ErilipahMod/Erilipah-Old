using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenPickaxe : ModItem
    {
        private const int Damage = 5;
        private const int UseSpeed = 17;
        private const float Knockback = 3;
        private const int Crit = 6;

        private int[] Ingredient => new int[2] { mod.ItemType("SoilComposite"), 5 };

        private int[] Dimensions => new int[2] { 28, 28 };

        private new string DisplayName = null;
        private const bool Staff = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.SwingThrow;
            item.maxStack = 1;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime = UseSpeed;
            item.useAnimation = UseSpeed;

            // occasionally changed booleans
            item.noMelee = false;
            item.melee = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;
            item.useTurn = true;
            item.channel = false;

            // occasionally changed integers
            item.pick = 60;
            item.rare = 0;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = Terraria.ID.AmmoID.None;
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            base.Tooltip.SetDefault("Can mine Meteorite");

            Terraria.Item.staff[item.type] = Staff;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(Ingredient[0], Ingredient[1]);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }
}
