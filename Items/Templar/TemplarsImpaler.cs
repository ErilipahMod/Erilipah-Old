using Microsoft.Xna.Framework;
using System;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Templar
{
    public class TemplarsImpaler : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Spear;
        protected override int[] Dimensions => new int[] { 46, 46 };
        protected override int Rarity => 2;

        protected override int Damage => 12;
        protected override int UseSpeed => 30;
        protected override float Knockback => 1;
        protected override float ShootSpeed => 3;

        protected override string DisplayName => "Templar's Impaler";
        protected override string Tooltip => "Right click to charge a powerful healing bolt\n" +
            "Uses 7, 15, or 23 vitality depending on charge\n" +
            "The more charge, the more allies' life is healed";

        private const int vitality = 10;

        private int Proj2 => ProjectileType<TemplarsImpalerProjProj>();

        public override bool AltFunctionUse(Player player) => player.GetModPlayer<Vitality>().CurrentVitality >= vitality;
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                type = Proj2;
            }
            return true;
        }
    }
    public class TemplarsImpalerProj : SpearProjectile
    {
        protected override float MoveBackTimePercent => 0.4f;
        protected override float MoveSpeed => 1.6f;

        protected override int[] Dimensions => new int[] { 16, 16 };
        protected override int DustType => 0;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.None;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage) player.GetModPlayer<Vitality>().AddVitality(damage / 7);
        }
    }
    public class TemplarsImpalerProjProj : ChargeProjectile
    {
        private int Frame
        {
            get
            {
                double charge = Math.Floor(Charge / (MaxCharge / FrameCount));
                int checkedCharge = Math.Min((int)charge, 2);
                return checkedCharge;
            }
        }

        protected override int[] Dimensions
        {
            get
            {
                if (Frame == 0)
                    return new int[] { 10, 12 };

                if (Frame == 1)
                    return new int[] { 10, 16 };

                return new int[] { 14, 22 };
            }
        }
        protected override int DustType => 74;
        protected override int FrameCount => 3;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;

        protected override DamageTypes DamageType => DamageTypes.Melee;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;

#pragma warning disable IDE1006 // Naming Styles
        private int heal => (Frame + 1) * 10;
#pragma warning restore IDE1006 // Naming Styles

        private Player CheckCollision()
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player healPlayer = Main.player[i];
                if (healPlayer.active && !healPlayer.dead && healPlayer.statLife < healPlayer.statLifeMax2 && healPlayer != Main.player[projectile.owner] &&
                    projectile.Colliding(projectile.Hitbox, healPlayer.Hitbox))
                    return healPlayer;
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
            player.GetModPlayer<Vitality>().charge = (int)(Charge / (MaxCharge / 4));
            projectile.frame = Frame;
            projectile.width = Dimensions[0];
            projectile.height = Dimensions[1];
            CheckHeal();
        }
        protected override bool[] DamageTeam => new bool[] { false, false };

        protected override void OnCancelCharge()
        {
            ResumeVelocity(10);
            player.GetModPlayer<Vitality>().SubVitality(Vitality);
        }
        protected override void OnFinishCharge()
        {
            ResumeVelocity(10);
            player.GetModPlayer<Vitality>().SubVitality(Vitality);
        }

        private int Vitality => (int)(heal * 0.75f);
        protected override bool Cancel => !Main.mouseRight || player.GetModPlayer<Vitality>().CurrentVitality <= Vitality;
        protected override float MaxCharge => 180;
        protected override float MoveDistance => 50;
    }
}
