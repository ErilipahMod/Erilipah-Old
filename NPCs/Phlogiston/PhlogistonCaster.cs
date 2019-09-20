using Erilipah.NPCs.Drone;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonCaster : ModNPC
    {
        // TODO: check source code for caster AI
        // TODO: telegraphs with orange line, fires a fast, large fire proj (dust)
        // TODO: inferno ball explodes on contact, MASSIVE damage and sends you flying, small ripple
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 40;
            npc.defense = 12;
            npc.damage = 16;
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noGravity = false;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.LiquidsWaterLava;

            npc.width = 32;
            npc.height = 52;

            npc.value = Item.buyPrice(0, 0, 4, 80);

            npc.MakeBuffImmune(BuffID.OnFire);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life < 0)
            for (int i = 0; i < 4; i++)
            {
                Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), 1.15f);
            }

            Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), Math.Min(1.2f, (float)damage / npc.lifeMax));
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            Teleport();
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            Teleport();
        }

        private void Teleport()
        {
            Player player = Main.player[npc.target];

            Vector2 newPosition = Vector2.Zero;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                Vector2 worldPos = new Vector2(
                    Main.rand.NextFloat(player.Center.X - 500, player.Center.X + 500), 
                    Main.rand.NextFloat(player.Center.Y - 500, player.Center.Y + 500));
                Point tilePos = worldPos.ToTileCoordinates();

                if (!Collision.SolidTiles(tilePos.X - 1, tilePos.Y - 1, tilePos.X + 1, tilePos.Y + 1))
                {
                    newPosition = worldPos;
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
                Vector2 spawnPosition = npc.Center + Main.rand.NextVector2CircularEdge(40, 40);
                Vector2 direction = spawnPosition.To(npc.Center);
                Dust.NewDustPerfect(spawnPosition, mod.DustType<PhlogistonDust>(), direction * 4);

                direction = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
                Dust.NewDustPerfect(newPosition, mod.DustType<PhlogistonDust>(), direction * 4);
            }

            npc.Center = newPosition;
        }

        private void DustFx(int currentTime, int cycleTime)
        {
            float percent = (float)currentTime / cycleTime;

            // Spawns 3 dusts that spiral inward in a triangle manner
            for (int i = 0; i < 3; i++)
            {
                float rotation  = percent * MathHelper.TwoPi;
                      rotation += i / 3 * MathHelper.TwoPi;
                float distance  = MathHelper.SmoothStep(600, 9, Math.Min(1, percent * 3f)); // At 33%, the dusts will circling already

                Dust.NewDustPerfect(DustBallPosition + new Vector2(distance, 0).RotatedBy(rotation), mod.DustType<DeepFlames>(), Vector2.Zero).noGravity = true;
            }

            // Spawns dusts on outer ring that come inward
            if (percent > 0.33f)
            {
                if (Main.rand.NextFloat(percent) > 0.2f)
                Dust.NewDustPerfect(DustBallPosition + Main.rand.NextVector2CircularEdge(9, 9), mod.DustType<PhlogistonDust>(), Vector2.One).customData = DustBallPosition;
            }

            // Spawns dust that goes to where the fireball will go
            if (percent > 0.5f)
            {
                float speed = 60 * (1 - percent) + 1; // + 1 to prevent DbZ exception
                Vector2 position = Vector2.Lerp(DustBallPosition, HeldPosition, currentTime / speed % 1);
                Dust.NewDustPerfect(position, mod.DustType<DeepFlames>(), Vector2.Zero).noGravity = true;
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

                if (npc.ai[0] < cycleTime - 60)
                {
                    npc.ai[1] = Main.player[npc.target].Center.X;
                    npc.ai[2] = Main.player[npc.target].Center.Y;
                }
            }
            else
            {
                if (Main.netMode != 1)
                {
                    Vector2 to = DustBallPosition.To(HeldPosition);
                    int damage = (int)(Main.expertMode ? npc.damage * 1.1 : npc.damage * 1.6);

                    Projectile.NewProjectile(DustBallPosition, to * 5.5f, mod.ProjectileType<Fireball>(), damage, 1, 255, HeldPosition.X, HeldPosition.Y);
                }

                Teleport();
                npc.netUpdate = true;
                npc.ai[0] = 0;
            }
        }
    }

    public class Fireball : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Name");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.extraUpdates = 2;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (target.statDefense < 100)
                target.velocity -= target.Center.To(projectile.Center, (100 - target.statDefense) / 100f * 7.5f);
            projectile.Kill();
        }

        public override void AI()
        {
            projectile.netUpdate = true;

            Vector2 targetLocation = new Vector2(projectile.ai[0], projectile.ai[1]);
            if (Vector2.Distance(projectile.Center, targetLocation) < 10)
            {
                projectile.timeLeft = 10;
            }

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(9, 9), mod.DustType<PhlogistonDust>(), Vector2.Zero).noGravity = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            const int count = 35;
            for (int i = 0; i < count; i++)
            {
                bool alt    = i % 2 == 0;
                int type    = alt ? mod.DustType<PhlogistonDust>() : mod.DustType<Drone.DeepFlames>();
                float speed = alt ? 3f : 5f;
                float angle = i / (float)count * MathHelper.PiOver2;

                Dust.NewDustPerfect(projectile.Center, type, angle.ToRotationVector2() * speed, 0);
            }
        }
    }
}
