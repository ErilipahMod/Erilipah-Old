using Erilipah.Projectiles;
using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonSword : NewModItem
    {
        protected override int Damage => 38;
        protected override int[] UseSpeedArray => new int[] { 20, 20 };
        protected override float Knockback => 6;

        protected override float ShootSpeed => 8;
        protected override int ShootCool => 34;
        protected override bool FiresProjectile => true;

        protected override int[] Dimensions => new int[] { 64, 64 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.SwordSwing;

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 8 }, { Terraria.ID.ItemID.HellstoneBar, 7 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 20;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
    public class PhlogistonSwordProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 30, 14 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0.04f;

        protected override DamageTypes DamageType => DamageTypes.Melee;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override int TrailThickness => 2;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees180;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 20;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}
