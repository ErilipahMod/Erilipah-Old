﻿using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenWaraxe : ModItem
    {
        private const int Damage = 20;
        private const int UseSpeed = 34; // change useTime
        private const float Knockback = 7;
        private const int Crit = 6;

        private int[] Ingredient => new int[2] { mod.ItemType("SoilComposite"), 4 };

        private int[] Dimensions => new int[2] { 34, 34 };

        private new string DisplayName = null;
        private new string Tooltip = null;
        private const float ShootSpeed = -1;
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
            item.useTime = UseSpeed / 3;
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
            item.axe = 70 / 5;
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