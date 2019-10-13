using Erilipah.Items.Dracocide;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Dracocide
{
    public class Swarmer : DracocideDrone
    {
        public override void AI()
        {
            if (!Base())
                return;

            npc.velocity = npc.Center.To(Target.Center - new Vector2(0, 125), 4f);

            if (Target.Distance(npc.Center) < 300)
                ++npc.ai[0];

            if (npc.ai[0] >= 240 && Main.netMode != 1) // When on the last frame
            {
                // Kill 'em.
                DeathCounter = 80;
                for (int i = 0; i < (Main.expertMode ? 15 : 11); i++)
                {
                    NPC mini = Main.npc[
                        NPC.NewNPC(
                            (int)npc.Center.X,
                            (int)npc.Center.Y,
                            mod.NPCType<MiniSwarmer>(),
                            ai1: (int)npc.ai[1],
                            ai2: PlayerTarget ? 1 : 0)];

                    mini.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat();
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (npc.ai[0] > 200)
                npc.frame.Y = (int)(npc.ai[0] - 200) / 5 * frameHeight;
            else
                npc.Animate(frameHeight, 5, 4);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 12;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 150;
            npc.defense = 15;
            npc.damage = 20;
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;

            npc.width = 32;
            npc.height = 60;

            npc.value = Item.buyPrice(0, 0, 40, 0);

            npc.MakeBuffImmune(BuffID.OnFire, BuffID.ShadowFlame, BuffID.CursedInferno, BuffID.Frostburn, BuffID.Chilled);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<Dracocell>(), 1, 1, 55);
            Loot.DropItem(npc, ItemID.SilverCoin, 30, 50, 100, 2);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    }

    public class MiniSwarmer : DracocideDrone
    {
        private bool loot = true;

        public override void AI()
        {
            Lighting.AddLight(npc.Center, 0.4f, 0.3f, 0.1f);
            npc.spriteDirection = -1;
            npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
            // Run
            if (fuckingRun || TargetIndex == -1 || Target == null || !Target.active || (Target is Player player && player.dead))
            {
                TargetIndex = -1;
                int motherIndex = npc.FindClosestNPC(1500, mod.NPCType<Observer>());
                if (motherIndex == -1)
                {
                    npc.netUpdate = true;
                    npc.velocity.Y -= 0.1f;
                    return;
                }
                Vector2 atMother = Main.npc[motherIndex].Center;

                if (Vector2.Distance(atMother, npc.Center) > 500)
                {
                    npc.velocity = npc.GoTo(atMother, 0.25f);
                    npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -5, Vector2.One * 5);
                }

                return;
            }

            var likeMe = Main.npc.Where(x => x.active && x.type == npc.type && x.ai[1] == npc.ai[1] && x.ai[2] == npc.ai[2])
                .OrderBy(x => x.whoAmI).ToList(); // All NPCs that are of this type & targetting the same thing.

            Vector2 futurePos = npc.position + npc.velocity.SafeNormalize(Vector2.Zero) * 48;
            bool canSeeTarget = Collision.CanHit(npc.position, npc.width, npc.height, Target.position, Target.width, Target.height);
            bool wontHitWall = Collision.CanHit(npc.position, npc.width, npc.height, futurePos, npc.width, npc.height);

            const float speed = 0.4f;
            bool ready = likeMe.Count <= 1 ||
                (likeMe.First() == npc && Target.velocity.X < 0) ||
                (likeMe.Last() == npc && Target.velocity.X > 0) ||
                (likeMe.ElementAt(likeMe.Count / 2) == npc && Target.velocity.X == 0);

            if (ready)
                npc.velocity = npc.GoTo(Target.Center + new Vector2(0, -5), speed);

            else if (!canSeeTarget || !wontHitWall) // Ensure it won't just run into a wall and fucking kill itself
                npc.velocity *= -0.5f;

            else
            {
                float degrees = MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, (float)likeMe.IndexOf(npc) / likeMe.Count);
                Vector2 spot = new Vector2(0, 300).RotatedBy(degrees);

                npc.velocity = npc.GoTo(Target.Center - spot, 0.25f);
                npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -3, Vector2.One * 3);
            }

            if (npc.collideX || npc.collideY)
            {
                loot = false;
                npc.StrikeNPC(30 + Main.rand.Next(-6, 6), 1, 1);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            npc.Animate(frameHeight, 5, 4);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Swarmer");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 1;
            npc.damage = 21;
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;

            npc.width = 20;
            npc.height = 20;

            npc.value = 0;

            npc.MakeBuffImmune(BuffID.OnFire, BuffID.ShadowFlame, BuffID.CursedInferno, BuffID.Frostburn, BuffID.Chilled);
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return projectile.type != mod.ProjectileType<HostileDracocideExplosion>();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Main.myPlayer] = 0;
            SpawnProj();
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.immune = false;
            SpawnProj();
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override bool PreNPCLoot() => loot;

        public override void NPCLoot() => Loot.DropItem(npc, mod.ItemType<Dracocell>(), 1, 1, 3);

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

        public override void HitEffect(int hitDirection, double damage) => SpawnProj();

        private void SpawnProj()
        {
            npc.life = 0;
            Projectile.NewProjectile(npc.Center, Vector2.Zero, mod.ProjectileType<HostileDracocideExplosion>(), npc.damage, 3f, Main.myPlayer);
        }
    }

    public class HostileDracocideExplosion : ModProjectile
    {
        public override string Texture => "Erilipah/Items/Dracocide/DracocideExplosion";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dracocide Explosion");
            Main.projFrames[projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;

            projectile.penetrate = -1;
            projectile.hostile = projectile.friendly = true;
        }
        public override void AI()
        {
            float intensity = MathHelper.Lerp(1, 0, projectile.frame / 6f);
            Lighting.AddLight(projectile.Center, intensity, intensity * 0.5f, 0);

            if (++projectile.frameCounter % 3 == 0)
                projectile.frame++;

            if (projectile.frameCounter == 1)
            {
                for (int i = 0; i < 9; i++)
                {
                    Vector2 randomVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 4f);
                    Dust.NewDustPerfect(projectile.Center, DustID.Fire, randomVelocity);
                }
            }
            if (projectile.frame == 6)
            {
                projectile.frame = 5;
                projectile.Kill();
            }
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 450);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 450);
        }
    }
}