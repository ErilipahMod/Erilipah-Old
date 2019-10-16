using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Templar
{
    public class TemplarsGrandPearlStaff : NewModItem
    {
        protected override UseTypes UseType => UseTypes.MagicStaff;
        protected override int[] Dimensions => new int[] { 48 };
        protected override int Rarity => 2;

        protected override int Damage => 28;
        protected override int UseSpeed => 30;
        protected override float Knockback => 4;
        protected override int Mana => 16;

        protected override float ShootSpeed => 8;
        protected override int ShootType => ProjectileType<OrbitCenter>();
        protected override bool Channel => true;

        protected override string DisplayName => "Templar's Grand Pearl Staff";
        protected override string Tooltip => "Fires a golden orb\n" +
            "Right click to charge a powerful healing orb\n" +
            "Uses 16 vitality and 8 more for each charge\n" +
            "The more charge, the more allies' life is healed";

        private const int vitality = 16;

        private int Proj2 => ProjectileType<HealingOrbitCenter>();

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

    public class OrbitCenter : ChargeProjectile
    {
        private int charge => (int)Math.Floor(Charge / (MaxCharge / 4));

        protected override bool Dusts => false;
        protected override bool Cancel => !player.channel;
        protected override float MaxCharge => 120;
        protected override float MoveDistance => 100;
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.alpha = 0;
        }

        protected override void OnCancelCharge()
        {
            OnFinishCharge();
            player.itemTime = 33;
        }
        protected override void WhileCharging()
        {
            if (Charge % (MaxCharge / 4) == 0 && player.whoAmI == projectile.owner)
            {
                Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileType<Orbit>(),
                    16, 2, player.whoAmI, projectile.whoAmI, Charge / (MaxCharge / 4));
            }
        }

        public override void AI()
        {
            base.AI();
            if (Charging)
                player.GetModPlayer<Vitality>().charge = charge;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage) player.GetModPlayer<Vitality>().AddVitality(damage / 6);
        }

        protected override int[] Dimensions => new int[] { 14 };
        protected override int DustType => DustID.MarblePot;

        protected override int Pierce => 2;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => Rotate(1);

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
    }
    public class Orbit : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 18, 12 };
        protected override int DustType => DustID.MarblePot;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => projectile.velocity.ToRotation();

        public override void AI()
        {
            base.AI();
            projectile.tileCollide = false;
            projectile.timeLeft = 2;
            if (!Main.projectile[(int)projectile.ai[0]].active)
            {
                projectile.Kill();
                return;
            }
            Projectile parent = Main.projectile[(int)projectile.ai[0]];

            if (parent.friendly)
                projectile.friendly = true;
            else
                projectile.friendly = false;

            OrbitParent(parent.Center + parent.velocity / 3);
        }

        private void OrbitParent(Vector2 parent)
        {
            const int dist = 20;

            float newDeg = projectile.localAI[0] + (projectile.ai[1] * 90);
            float rad = MathHelper.ToRadians(newDeg);

            projectile.position.X = parent.X - (int)(Math.Cos(rad) * dist) - projectile.width / 2;
            projectile.position.Y = parent.Y - (int)(Math.Sin(rad) * dist) - projectile.height / 2;
            projectile.rotation = rad - MathHelper.ToRadians(90);

            projectile.velocity = Vector2.Zero;

            projectile.localAI[0] += 6;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage) player.GetModPlayer<Vitality>().AddVitality(damage / 6);
        }

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
    }

    public class HealingOrbitCenter : ChargeProjectile
    {
        private int charge => (int)Math.Floor(Charge / (MaxCharge / 4));

        private int vitality => charge * 4 + 16;

        protected override bool Dusts => false;
        protected override bool Cancel => !Main.mouseRight || player.GetModPlayer<Vitality>().CurrentVitality <= vitality;
        protected override float MaxCharge => 120;
        protected override float MoveDistance => 100;
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.alpha = 0;
        }

        protected override void OnCancelCharge()
        {
            ResumeVelocity(10);
            player.GetModPlayer<Vitality>().SubVitality(vitality);
            player.itemTime = 33;
        }
        protected override void OnFinishCharge()
        {
            ResumeVelocity(10);
            player.GetModPlayer<Vitality>().SubVitality(vitality);
        }
        protected override void WhileCharging()
        {
            if (Charge % (MaxCharge / 4) == 0 && player.whoAmI == projectile.owner)
            {
                Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileType<HealingOrbit>(),
                    16, 2, player.whoAmI, projectile.whoAmI, Charge / (MaxCharge / 4));
            }
        }

        private int heal => charge > 1 ? 24 : 16;

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
            if (Charging)
                player.GetModPlayer<Vitality>().charge = charge;
            projectile.friendly = false;
            CheckHeal();
        }

        protected override int[] Dimensions => new int[] { 14 };
        protected override int DustType => 74;

        protected override int Pierce => 2;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
    }
    public class HealingOrbit : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 18, 12 };
        protected override int DustType => 74;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => projectile.velocity.ToRotation();

        private int heal = 0;

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
            if (heal > 0)
                CheckHeal();
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.timeLeft = 2;
            if (!Main.projectile[(int)projectile.ai[0]].active)
            {
                projectile.Kill();
                return;
            }
            Projectile parent = Main.projectile[(int)projectile.ai[0]];
            if (parent.localAI[0] >= 120)
                heal = 8;

            OrbitParent(parent.Center + parent.velocity / 3);
        }

        private void OrbitParent(Vector2 parent)
        {
            const int dist = 20;

            float newDeg = projectile.localAI[0] + (projectile.ai[1] * 90);
            float rad = MathHelper.ToRadians(newDeg);

            projectile.position.X = parent.X - (int)(Math.Cos(rad) * dist) - projectile.width / 2;
            projectile.position.Y = parent.Y - (int)(Math.Sin(rad) * dist) - projectile.height / 2;
            projectile.rotation = rad - MathHelper.ToRadians(90);

            projectile.velocity = Vector2.Zero;

            projectile.localAI[0] += 6;
        }

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
    }
}
