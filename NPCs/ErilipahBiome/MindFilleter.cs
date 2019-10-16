using Erilipah.Biomes.ErilipahBiome.Hazards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.ErilipahBiome
{
    internal class MindFilleter : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brain Filleter");
            Main.npcFrameCount[npc.type] = 8;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 130 : 60;
            npc.defense = Main.hardMode ? 22 : 15;
            npc.damage = Main.hardMode ? 36 : 26;
            npc.knockBackResist = 0.25f;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit19;

            npc.width = 42;
            npc.height = 58;

            npc.value = 150;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.5f);
            return false;
        }


        private Player Target => Main.player[npc.target];
        private Vector2 Eye => npc.position + new Vector2(20, 30).RotatedBy(npc.rotation, npc.Size / 2);

        private void HurtSound() => Main.PlaySound(Target.Male ? SoundID.PlayerHit : SoundID.FemaleHit, (int)npc.Center.X, (int)npc.Center.Y, 0, 1, Main.rand.NextFloat(-0.8f, -0.7f));
        private void DeathSound() => Main.PlaySound(SoundID.PlayerKilled, (int)npc.Center.X, (int)npc.Center.Y, 0, 1, Main.rand.NextFloat(-0.3f, -0.25f));

        private int spewTimer = 0;

        public override void AI()
        {
            npc.rotation = npc.Center.To(Target.Center).ToRotation() - MathHelper.PiOver2;

            if (Target.immuneTime == 3)
            {
                HurtSound();
            }

            if (npc.collideX)
                npc.velocity.X = npc.velocity.X / -1.5f;
            if (npc.collideY)
                npc.velocity.Y = npc.velocity.Y / -1.5f;

            bool canSee = Collision.CanHitLine(Target.position, Target.width, Target.height, npc.position, npc.width, npc.height);

            if (canSee)
            {
                npc.noTileCollide = false;
            }

            npc.aiStyle = -1;

            npc.velocity.X -= Math.Sign(npc.Center.X - Target.Center.X) * 0.02f;
            npc.velocity.Y -= Math.Sign(npc.Center.Y - Target.Center.Y) * 0.02f;
            npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -4, Vector2.One * 4);

            if (canSee)
                spewTimer++;

            if (spewTimer >= 150)
            {
                npc.netUpdate = true;
                spewTimer = 0;

                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(Eye, Eye.To(Target.Center, 6) - new Vector2(0, 1.5f), ProjectileType<Vomit>(), 1, 0);
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
                    Dust.NewDustPerfect(Eye, DustType<FlowerDust>(), rot.ToRotationVector2() * 10, Scale: 1.5f).noGravity = true;
                }

                for (int i = 0; i <= 4; i++)
                {
                    Gore.NewGore(npc.Center, new Vector2(hitDirection, -2), mod.GetGoreSlot("Gores/ERBiome/MindFilleter" + i));
                }
            }
            else
            {
                HurtSound();

                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDustPerfect(Eye, DustType<FlowerDust>(), new Vector2(hitDirection * 3, -3)).noGravity = true;
                }
            }
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemType<Items.ErilipahBiome.PutridFlesh>(), 1, 1, 18);
            Loot.DropItem(npc, ItemID.Heart, 1, 1, 50);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InErilipah() && spawnInfo.spawnTileY < Main.rockLayer ? 0.065f : 0;
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
            projectile.damage = 1;
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
            projectile.velocity.Y += 0.055f;
            Dust.NewDustPerfect(projectile.Center, DustType<VoidLiquid>(), Main.rand.NextVector2CircularEdge(0.1f, 0.1f), Scale: 0.55f);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<VoidLiquid>(), Main.rand.NextVector2CircularEdge(1.5f, 1.5f));
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            projectile.Kill();
        }
    }
}
