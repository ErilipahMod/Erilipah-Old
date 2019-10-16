using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteRelic : ModItem
    {
        private const int Damage = 0;
        private const int Crit = 0;
        private const int UseSpeed = 14;
        private const float Knockback = 0;
        private const float ShootSpeed = 0;
        private static readonly int[] Dimensions = new int[] { 36, 30 };
        private new string DisplayName = null;
        private new string Tooltip = "Summons a sandstorm for 12 in-game hours" +
            "\nOnly usable in the desert when no sandstorms are active" +
            "\n'Awaken the storm!'";
        private const bool Staff = false;
        private const bool Shoots = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.HoldingUp;
            item.maxStack = 999;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            if (Shoots)
#pragma warning disable CS0162 // Unreachable code detected
                item.shootSpeed = ShootSpeed;
#pragma warning restore CS0162 // Unreachable code detected

            // occasionally changed booleans
            item.noMelee = false;
            //item.melee = true;
            item.noUseGraphic = false;
            item.consumable = true;
            item.autoReuse = false;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item8;
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
        public override bool UseItem(Player player)
        {
            Sandstorm.Happening = true;
            Sandstorm.TimeLeft = 720 * 60;
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            return !Sandstorm.Happening && player.ZoneDesert;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(ItemType<SacraciteCore>(), 1);
            r.AddIngredient(ItemID.SandBlock, 50);
            r.AddIngredient(ItemID.AntlionMandible, 3);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }
}
