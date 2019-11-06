using Erilipah.NPCs.Drone;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonCaster : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 120 : 60;
            npc.defense = 12;
            npc.damage = 30;
            npc.knockBackResist = 0.5f;

            npc.lavaImmune = true;
            npc.aiStyle = 0;
            npc.noGravity = false;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.LiquidsWaterLava;

            npc.width = 32;
            npc.height = 48;

            npc.value = Item.buyPrice(0, 0, 4, 80);

            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor.R = Math.Max(drawColor.R, (byte)50);
            drawColor.G = Math.Max(drawColor.G, (byte)50);
            drawColor.B = Math.Max(drawColor.B, (byte)50);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemType<Items.Phlogiston.StablePhlogiston>(), 2, 3, 100, 1.5f);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.Underworld.Chance * 0.07f;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
                for (int i = 0; i < 4; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), 1.15f);
                }

            Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), Math.Min(1.2f, (float)damage / npc.lifeMax));
        }

        private void Teleport()
        {
            Player player = Main.player[npc.target];

            Vector2 newPosition = Vector2.Zero;
            for (int attempt = 0; attempt < 150; attempt++)
            {
                Vector2 worldPos = new Vector2(
                    Main.rand.NextFloat(player.Center.X - 650, player.Center.X + 650),
                    Main.rand.NextFloat(player.Center.Y - 250, player.Center.Y + 250));
                Point tilePos = worldPos.ToTileCoordinates();

                bool canSeePlayer = Collision.CanHitLine(worldPos, 1, 1, player.Center, 1, 1);
                bool empty = !Collision.SolidTiles(tilePos.X - 1, tilePos.X + 1, tilePos.Y - 1, tilePos.Y + 2);
                bool floor = Collision.SolidTiles(tilePos.X - 1, tilePos.X + 1, tilePos.Y + 3, tilePos.Y + 3);
                if (canSeePlayer && empty && floor && Main.tile[tilePos.X, tilePos.Y].liquid < 10 && Vector2.Distance(worldPos, player.Center) > 80)
                {
                    newPosition = tilePos.ToWorldCoordinates() + new Vector2(0, 16);
                    break;
                }
            }
            if (newPosition == Vector2.Zero)
            {
                return;
            }

            Main.PlaySound(SoundID.Item8, npc.Center);
            Main.PlaySound(SoundID.NPCDeath3, newPosition);

            for (int i = 0; i < 22; i++)
            {
                Vector2 spawnPosition = npc.Center + Main.rand.NextVector2CircularEdge(20, 20);
                Vector2 direction = (i / 22f * MathHelper.TwoPi).ToRotationVector2();
                Dust.NewDustPerfect(spawnPosition, DustType<PhlogistonDust>(), direction * 2.5f);

                Dust.NewDustPerfect(newPosition + Main.rand.NextVector2CircularEdge(100, 100), DustType<PhlogistonDust>(), new Vector2(2.5f)).customData = newPosition;
            }

            npc.Center = newPosition;
        }

        private void DustFx(int currentTime, int cycleTime)
        {
            float percent = (float)currentTime / cycleTime;

            // Spawns 3 dusts that spiral inward in a triangle manner
            for (int i = 0; i < 3; i++)
            {
                float rotation = percent * MathHelper.TwoPi;
                rotation += i / 3f * MathHelper.TwoPi;
                float distance = MathHelper.SmoothStep(200, 20, Math.Min(1, percent * 3f)); // At 33%, the dusts will circling already

                Dust.NewDustPerfect(DustBallPosition + new Vector2(distance, 0).RotatedBy(rotation), DustType<DeepFlames>(), Vector2.Zero).noGravity = true;
            }

            // Spawns dusts on outer ring that come inward
            if (percent > 0.33f)
            {
                if (Main.rand.NextFloat(percent) > 0.2f)
                    Dust.NewDustPerfect(DustBallPosition + Main.rand.NextVector2CircularEdge(15, 15), DustType<PhlogistonDust>(), Vector2.One).customData = DustBallPosition;
            }

            // Spawns dust that goes to where the fireball will go
            if (percent > 0.33f)
            {
                const int totalSpan = 250;
                const int spacing = 10;
                float dustSpanNormally = MathHelper.SmoothStep(0, totalSpan, (percent - 0.33f) * 1.5f);
                float distanceTarget = Vector2.Distance(HeldPosition, DustBallPosition);
                int currentSpan = (int)Math.Min(dustSpanNormally, distanceTarget);

                Vector2 direction = (HeldPosition - DustBallPosition).SafeNormalize(Vector2.Zero);
                for (int i = 0; i < currentSpan; i += spacing)
                {
                    Dust.NewDustPerfect(DustBallPosition + direction * 20 + direction * i, DustType<DeepFlames>(), Vector2.Zero, Scale: 0.8f).noGravity = true;
                }
            }
        }

        private Vector2 DustBallPosition => npc.Center - Vector2.UnitY * 50;
        private Vector2 HeldPosition => new Vector2(npc.ai[1], npc.ai[2]);
        public override void AI()
        {
            // ai0 = timer
            // ai1 = to X
            // ai2 = to Y

            npc.ai[0]++;
            npc.FaceTarget();

            if (npc.wet && !npc.lavaWet)
            {
                npc.life = 0;
                HitEffect(0, 1);
                Main.PlaySound(npc.DeathSound, npc.Center);
                npc.active = false;
            }

            const int restTime = 120;
            if (npc.ai[0] < restTime)
            {
                // Do nothing! This is the player's break period
                return;
            }

            const int cycleTime = 360;
            if (npc.ai[0] < cycleTime)
            {
                DustFx((int)npc.ai[0] - restTime, cycleTime - restTime);

                int leeway = (Main.expertMode ? cycleTime - 12 : cycleTime - 18);
                if (npc.ai[0] < leeway)
                {
                    npc.ai[1] = Main.player[npc.target].Center.X;
                    npc.ai[2] = Main.player[npc.target].Center.Y;
                }
            }
            else if (npc.ai[0] == cycleTime)
            {
                Main.PlaySound(SoundID.Item45, npc.Center);
                if (Main.netMode != 1)
                {
                    Vector2 to = DustBallPosition.To(HeldPosition);
                    int damage = Main.expertMode ? 40 : 50;
                    if (Main.hardMode)
                        damage = (int)(damage * 1.5);

                    Projectile.NewProjectile(DustBallPosition, to * 6.5f, ProjectileType<Fireball>(), damage, 1, 255, HeldPosition.X, HeldPosition.Y);
                }
                npc.netUpdate = true;
            }
            else if (npc.ai[0] > cycleTime + 80)
            {
                Teleport();
                npc.ai[0] = 0;
                npc.netUpdate = true;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (damage > npc.lifeMax / 2)
            {
                Teleport();
                npc.ai[0] = 0;
                npc.netUpdate = true;
            }
            target.AddBuff(BuffID.OnFire, 180);
        }
    }

    public class Fireball : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Name");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.extraUpdates = 3;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (target.statDefense < 100)
            {
                float reduction = Math.Max(200 - target.statDefense, 0) / 200f;
                reduction *= target.noKnockback ? 0.7f : 1;

                target.velocity -= target.DirectionTo(projectile.Center) * reduction * 8f;
                if (!target.noKnockback)
                    target.velocity.Y -= 1.5f;
            }
            target.AddBuff(BuffID.OnFire, 180);
            projectile.Kill();
        }

        public override void AI()
        {
            projectile.netUpdate = true;

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(15, 15), DustType<PhlogistonDust>(), Vector2.Zero).noGravity = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            const int count = 60;
            for (int i = 0; i < count; i++)
            {
                bool alt = i % 2 == 0;
                int type = alt ? DustType<PhlogistonDust>() : DustType<Drone.DeepFlames>();
                float speed = alt ? 3f : 5f;
                float angle = i / (float)count * MathHelper.TwoPi;

                Dust.NewDustPerfect(projectile.Center, type, angle.ToRotationVector2() * speed, 0);
            }
        }
    }
}
