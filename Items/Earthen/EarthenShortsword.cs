using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenShortsword : ModItem
    {
        private const int Damage = 7;
        private const int UseSpeed = 9;
        private const float Knockback = 1;
        private const int Crit = 14;

        private int[] Ingredient => new int[2] { mod.ItemType("SoilComposite"), 4 };

        private int[] Dimensions => new int[2] { 30, 30 };

        private new string DisplayName = null;
        private new string Tooltip = "'Surprisingly agile...'";
        private const float ShootSpeed = -1;
        private const bool Staff = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.Stabbing;
            item.maxStack = 1;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            if (ShootSpeed >= 0)
#pragma warning disable CS0162 // Unreachable code detected
                item.shootSpeed = ShootSpeed;
#pragma warning restore CS0162 // Unreachable code detected

            // occasionally changed booleans
            item.noMelee = false;
            item.melee = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;
            item.useTurn = true;
            item.channel = false;

            // occasionally changed integers
            item.rare = 0;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = Terraria.ID.AmmoID.None;
            if (ShootSpeed >= 0)
#pragma warning disable CS0162 // Unreachable code detected
                item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
#pragma warning restore CS0162 // Unreachable code detected
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);

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