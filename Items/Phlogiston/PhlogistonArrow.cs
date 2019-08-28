﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonArrow : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Arrow;
        protected override int[] Dimensions => new int[] { 14, 40 };
        protected override int Rarity => 3;
        protected override int Damage => 7;
        protected override string Tooltip => "Splits into smaller bolts on impact";

        protected override float ShootSpeed => 9;
        protected override int[,] CraftingIngredients => new int[,]
        {
            { ItemID.WoodenArrow, 333 },
            { mod.ItemType<StablePhlogiston>(), 1 }
        };
        protected override int CraftingTile => TileID.Anvils;
    }
    public class PhlogistonArrowProj : NewModProjectile
    {
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Width = 20;
            hitbox.Height = 30;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 20;
            height = 30;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override int AIStyle
        {
            get
            {
                aiType = ProjectileID.WoodenArrowFriendly;
                return 1;
            }
        }

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;

        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 300);
            Helper.FireInCircle(projectile.Center, 4, mod.ProjectileType<PhlogistonArrowProjProj>(),
                projectile.damage - 1, 7, projectile.knockBack, owner: projectile.owner);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Helper.FireInCircle(projectile.Center, 8, mod.ProjectileType<PhlogistonArrowProjProj>(),
               projectile.damage - 1, 7, projectile.knockBack, owner: projectile.owner);
            return true;
        }
    }
    public class PhlogistonArrowProjProj : NewModProjectile
    {
        protected override TextureTypes TextureType => TextureTypes.Invisible;
        protected override int[] Dimensions => new int[] { 12, 16 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        public override void AI()
        {
            base.AI();
            if (projectile.timeLeft > 150)
                projectile.friendly = false;
            else
                projectile.friendly = true;
        }

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees180;
    }
}
