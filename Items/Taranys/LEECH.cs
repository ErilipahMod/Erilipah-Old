using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Erilipah.NPCs.ErilipahBiome;
using Erilipah.Items.ErilipahBiome;

namespace Erilipah.Items.Taranys
{
    class LEECH : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("L.E.E.C.H.");
            Tooltip.SetDefault(
                "Barrages your enemies with volleys of plague and death\n" +
                "Sustaining attacks strengthens these plagues\n" +
                "'You get a cookie if you can guess what it stands for'"
                );

            //Item.staff[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 28;

            item.damage = 30;
            item.knockBack = 2.5f;
            item.crit = 6;

            item.ranged = true;
            item.noMelee = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 11f;

            item.useTime =
            item.useAnimation = 10;

            item.useStyle = ItemUseStyleID.HoldingOut;
            item.autoReuse = true;
            item.useTurn = false;

            item.maxStack = 1;
            item.value = 1000;
            item.rare = ItemRarityID.LightRed;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 vel = new Vector2(speedX, speedY);
            Vector2 offset = vel.SafeNormalize(Vector2.Zero) * 40 - (Vector2)HoldoutOffset();
            if (!Collision.CanHitLine(position, 0, 0, position + offset, 0, 0))
                return false;

            position += offset;

            // Standard gun
            if (type != ProjectileID.Bullet)
            {
                vel = vel.RotateRandom(0.1);
                speedX = vel.X;
                speedY = vel.Y;

                Main.PlaySound(SoundID.Item11, position);
                return true;
            }

            // Spechull
            if (Main.rand.NextBool(8))
            {
                type = mod.ProjectileType<CrystalBolt>();
                vel /= 2;
                damage += 10;
                knockBack *= 1.5f;

                Main.PlaySound(SoundID.Item101, position);
            }
            else
            {
                type = mod.ProjectileType<VoidBolt>();
                vel = vel.RotateRandom(0.15);
                vel /= 2;

                Main.PlaySound(SoundID.Item111, position);
            }

            speedX = vel.X;
            speedY = vel.Y;

            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(8, 0);
        }
    }

    public class VoidBolt : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("LEECH");
        }
        public override void SetDefaults()
        {
            projectile.width = 6;
            projectile.height = 6;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 3;

            projectile.ranged = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType<Wither>(), 80);
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.04f;
            Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(3, 3), mod.DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Vector2.Zero)
                .noGravity = true;
            Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(3, 3), mod.DustType<VoidParticle>(), Vector2.Zero, Scale: 0.7f)
                .noGravity = true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath9, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
                Dust.NewDustPerfect(projectile.Center, mod.DustType<VoidParticle>(), 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi), Scale: 0.7f)
                    .noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6, 6), mod.DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Vector2.Zero)
                    .noGravity = true;
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6, 6), mod.DustType<VoidParticle>(), Vector2.Zero, Scale: 0.7f)
                    .noGravity = true;
            }
        }
    }

    public class CrystalInfection : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "Erilipah/Debuff";
            return true;
        }

        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
            npc.lifeRegen -= npc.GetGlobalNPC<ErilipahNPC>().CrystalInfectionDamage;

            if (Main.rand.NextBool())
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, mod.DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Scale: 1.5f);
                d.velocity = new Vector2(0, -5);
                d.noGravity = true;
            }

            int other = npc.FindClosestNPC(100);
            if (other != -1)
            {
                Main.npc[other].AddBuff(Type, npc.buffTime[buffIndex]);
            }
        }
    }

    public class CrystalBolt : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("LEECH");
        }
        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 12;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 2;

            projectile.ranged = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType<CrystalInfection>(), 500);
            projectile.Kill();
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.04f;

            for (int i = 0; i < 2; i++)
            {
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(6, 6), mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero)
                    .noGravity = true;
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(6, 6), mod.DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Vector2.Zero, Scale: 1.25f)
                    .noGravity = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item27, projectile.Center);
            for (int i = 0; i < 18; i++)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<Crystalline.CrystallineDust>(), 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi));
            }
            for (int i = 0; i < 18; i++)
            {
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6, 6), mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero)
                    .noGravity = true;
            }

            // Small AoE
            projectile.width += 20;
            projectile.height += 20;
            projectile.Center -= Vector2.One * 10;
        }
    }
}
