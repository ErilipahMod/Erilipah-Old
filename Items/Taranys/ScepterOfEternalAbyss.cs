using Terraria.ID;
using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Erilipah.Items.Weapons;
using Erilipah.Items.ErilipahBiome;

namespace Erilipah.Items.Taranys
{
    public class ScepterOfEternalAbyss : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scepter of Ethereal Abyss");
            Tooltip.SetDefault("Fires powerful ethereal knives");
            Item.staff[item.type] = true;
        }
        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 46;

            item.damage = 33;
            item.knockBack = 4;
            item.crit = 8;
            item.magic = true;

            item.reuseDelay = 7;
            item.useTime = 12;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;

            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;

            item.UseSound = SoundID.Item43;

            item.maxStack = 1;
            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<ScepterProj1>();
            item.shootSpeed = 13f;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position.X += Main.rand.NextFloat(-75, -30) * player.direction;
            position.Y += Main.rand.NextFloat(-60, 60);

            Lighting.AddLight(position, 0.22f, 0.1f, 0.35f);

            Vector2 to = position.To(Main.MouseWorld) * item.shootSpeed;
            to = to.RotateRandom(0.15f);
            speedX = to.X;
            speedY = to.Y;

            if (Main.rand.NextBool())
                type = mod.ProjectileType<ScepterProj2>();

            for (int i = 0; i < 35; i++)
            {
                float rot = i / 35f * MathHelper.TwoPi;
                bool shouldIncVel  = rot > MathHelper.Pi + MathHelper.PiOver4 && rot < MathHelper.TwoPi - MathHelper.PiOver4;
                shouldIncVel |= rot > MathHelper.PiOver4 && rot < MathHelper.PiOver2 + MathHelper.PiOver4;

                float mult = shouldIncVel ? 5f : 3f;

                Dust.NewDustPerfect(position, mod.DustType<Crystalline.CrystallineDust>(), Vector2.UnitX.RotatedBy(rot) * mult, Scale: 0.75f)
                    .noGravity = true;
            }

            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    public class ScepterProj2 : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scepter");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.magic = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType<Wither>(), 90);
            projectile.Kill();
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (projectile.ai[0]++ > 120)
                projectile.tileCollide = true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, DustID.PurpleCrystalShard, 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
            }
        }
    }

    public class ScepterProj1 : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scepter");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.magic = true;
            projectile.maxPenetrate = 6;
            projectile.penetrate = 6;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void AI()
        {
            int dir = Math.Sign(projectile.velocity.X);
            projectile.spriteDirection = -dir;
            projectile.rotation += dir * 0.4f;

            if (projectile.ai[0]++ > 120)
                projectile.tileCollide = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.velocity *= 0.885f;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath3, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, DustID.PurpleCrystalShard, 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
            }
        }
    }
}
