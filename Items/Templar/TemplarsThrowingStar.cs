using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Templar
{
    public class TemplarsThrowingStar : NewModItem
    {
        protected override string DisplayName => "Templar's Throwing Star";
        protected override string Tooltip => "Right click to fire a small healing shuriken" +
            "\nUses " + vitality + " vitality";
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 28, 28 };
        protected override int Rarity => 2;

        protected override int Damage => 9;
        protected override int UseSpeed => 15;
        protected override float Knockback => 1.25f;
        protected override float ShootSpeed => 9;

        private const int vitality = 5;

        private int Proj2 => mod.ProjectileType<ThrowingStarMini>();

        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) =>
            player.altFunctionUse != 2 || (player.GetModPlayer<Vitality>().CurrentVitality >= vitality && player.altFunctionUse == 2);
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                type = Proj2;
                Vitality modPlayer = player.GetModPlayer<Vitality>();
                modPlayer.SubVitality(vitality);
            }
            return true;
        }
    }
    public class TemplarsThrowingStarProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => DustID.MarblePot;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0.1f;
        protected override int FlightTime => 15;
        protected override float? Rotation => Rotate(2);

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage) player.GetModPlayer<Vitality>().AddVitality(damage / 6);
        }
    }
    public class ThrowingStarMini : NewModProjectile
    {
        protected override TextureTypes TextureType => TextureTypes.Default;
        protected override int[] Dimensions => new int[] { 12, 12 };
        protected override int DustType => 74;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0.1f;
        protected override float? Rotation => Rotate(2);

        protected override bool[] DamageTeam => new bool[] { false, false };
        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;

        private const int heal = 5;

        private Player CheckCollision()
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead && Main.player[i] != Main.player[projectile.owner] &&
                    projectile.Colliding(projectile.Hitbox, Main.player[i].Hitbox))
                    return Main.player[i];
            }
            return null;
        }

        private void CheckHeal()
        {
            Player player = CheckCollision();
            if (player != null)
            {
                player.Heal(heal);
                projectile.Kill();
            }
        }
        public override void AI()
        {
            base.AI();
            CheckHeal();
        }
    }
}
