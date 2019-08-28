using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class DunePiercer : ModItem
    {
        private const int Damage = 10;
        private const int Crit = 5;
        private const int UseSpeed = 18;
        private const float Knockback = 5;
        private const float ShootSpeed = 9.5f;
        private static readonly int[] Dimensions = new int[] { 36, 36 };
        private new string DisplayName = null;
        private new string Tooltip = null;
        private const bool Staff = false;
        private const bool Shoots = false;

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
            item.useTime =
                item.useAnimation = UseSpeed;

            // occasionally changed booleans
            item.noMelee = false;
            item.melee = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.pick = 70;
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = Terraria.ID.AmmoID.None;
            if (Shoots)
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

            r.AddIngredient(mod.ItemType<SacraciteIngot>(), 3);
            r.AddIngredient(mod.ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }
}
