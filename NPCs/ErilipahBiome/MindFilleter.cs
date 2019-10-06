﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Erilipah.Biomes.ErilipahBiome.Hazards;

namespace Erilipah.NPCs.ErilipahBiome
{
    class MindFilleter : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brain Filleter");
            Main.npcFrameCount[npc.type] = 8;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 150 : 65;
            npc.defense = 12;
            npc.damage = 26;
            npc.knockBackResist = 0.25f;

            npc.aiStyle = 0;
            npc.noGravity = false;
            npc.noTileCollide = false;

            npc.width = 42;
            npc.height = 58;

            npc.value = 150;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        private Player Target => Main.player[npc.target];
        private Vector2 Eye => npc.position + new Vector2(20, 30).RotatedBy(npc.rotation, npc.Size / 2);

        private void HurtSound() => Main.PlaySound(Target.Male ? SoundID.PlayerHit : SoundID.FemaleHit, (int)npc.Center.X, (int)npc.Center.Y, 0, 1, Main.rand.NextFloat(0.2f, 0.4f));
        private void DeathSound() => Main.PlaySound(SoundID.PlayerKilled, (int)npc.Center.X, (int)npc.Center.Y, 0, 1, Main.rand.NextFloat(0.12f, 0.22f));

        private int spewTimer = 0;

        public override void AI()
        {
            npc.rotation = npc.Center.To(Target.Center).ToRotation() - MathHelper.PiOver2;

            if (Target.immuneTime == 3)
            {
                HurtSound();
            }

            bool canSee = Collision.CanHitLine(Target.Center, 1, 1, npc.Center, 1, 1);

            if (npc.Distance(Target.Center) > 300)
            {
                npc.velocity = npc.GoTo(Target.Center, 0.265f, 2f);
            }
            else if (npc.Distance(Target.Center) < 50 || !canSee)
            {
                npc.velocity = -npc.GoTo(Target.Center, 0.1f, 2f);
            }
            else
            {
                npc.velocity *= 0.93f;
            }

            if (canSee)
                spewTimer++;

            if (spewTimer > 90 && spewTimer < 150)
            {
                Dust.NewDustPerfect(Eye, mod.DustType<FlowerDust>(), Vector2.Zero, Scale: 1.25f).noGravity = true;
            }

            if (spewTimer >= 150)
            {
                npc.netUpdate = true;
                spewTimer = 0;

                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(Eye, Eye.To(Target.Center, 6) - new Vector2(0, 1.5f), mod.ProjectileType<Vomit>(), 0, 0);
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(spewTimer);
        public override void ReceiveExtraAI(BinaryReader reader) => spewTimer = reader.ReadInt32();

        public override void FindFrame(int frameHeight)
        {
            npc.Animate(frameHeight, 5);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                DeathSound();

                for (int i = 0; i < 18; i++)
                {
                    float rot = i / 18f * MathHelper.TwoPi;
                    Dust.NewDustPerfect(Eye, mod.DustType<FlowerDust>(), rot.ToRotationVector2() * 6).noGravity = true;
                }
            }
            else
            {
                HurtSound();

                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDustPerfect(Eye, mod.DustType<FlowerDust>(), new Vector2(hitDirection * 3, -3)).noGravity = true;
                }
            }
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<Items.ErilipahBiome.PutridFlesh>(), 1, 1, 18);
            Loot.DropItem(npc, ItemID.Heart, 1, 1, 50);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InErilipah() && spawnInfo.spawnTileY < Main.rockLayer ? 0.06f : 0;
        }
    }

    public class Vomit : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        //public override string GlowTexture => Texture;

        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("tMod_Projectile1");
        }

        public override void SetDefaults()
        {
            projectile.damage = 0;
            projectile.width = 8;
            projectile.height = 8;

            projectile.aiStyle = 0;
            projectile.timeLeft = 300;
            projectile.tileCollide = true;

            projectile.maxPenetrate = 1;
            projectile.hostile = !(projectile.friendly = false);

            projectile.SetInfecting(2.5f);
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.025f;
            Dust.NewDustPerfect(projectile.Center, mod.DustType<VoidLiquid>(), Main.rand.NextVector2CircularEdge(0.1f, 0.1f));
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<VoidLiquid>(), Main.rand.NextVector2CircularEdge(2.5f, 2.5f));
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.immune = true;
            target.immuneTime = 45;
        }
    }
}