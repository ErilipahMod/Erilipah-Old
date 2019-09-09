﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonPistol : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phlogiston Longbow");
            Tooltip.SetDefault("Fires an extra arrow\nCoats wooden arrows in phlogiston");
        }
        public override void SetDefaults()
        {
            item.width = 36;
            item.height = 66;

            item.damage = 18;
            item.ranged = true;
            item.knockBack = 1.7f;

            item.useTime = 17;
            item.useAnimation = 17;
            item.useStyle = 5;
            item.UseSound = SoundID.Item5;
            item.autoReuse = true;

            item.rare = 3;
            item.shootSpeed = 9f;
            item.shoot = 3;
            item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
                type = mod.ProjectileType<PhlogistonArrowProj>();

            int closest = player.FindClosestNPC(1000);
            if (closest != -1)
            {
                Vector2 npcPos = Main.npc[closest].Center;
                Vector2 velocity = position.To(npcPos, item.shootSpeed * 1.25f);
                Projectile.NewProjectile(position, velocity, type, damage, knockBack, player.whoAmI);
            }
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "StablePhlogiston", 6);
            r.AddIngredient(ItemID.Hellstone, 3);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
