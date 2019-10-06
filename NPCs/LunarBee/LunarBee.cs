using Erilipah.Items.LunarBee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.LunarBee
{
    [AutoloadBossHead]
    public class LunarBee : ModNPC
    {
        private const int overhead = 0;
        private const int smallchunks = 1;
        private const int summon = 2;
        private const int dashing = 3;
        private const int DIOknives = 4;

        private const int numOverhead = 5;
        private const int numSmallChunks = 8;
        private const int numSummon = 3;
        private const int numDash = 5;
        private const int numDIO = 4;

        private const int dmgOverhead = 16;
        private const int dmgSmallChunks = 12;
        private const int dmgSummon = 13;
        private const int dmgDash = 30;
        private const int dmgDIO = 14;

        private Player Target => Main.player[npc.target];
        private int Phase { get => (int)npc.ai[0]; set => npc.ai[0] = value; }
        private int PhaseTimer { get => (int)npc.ai[1]; set => npc.ai[1] = value; }
        private Vector2 DashEndPos
        {
            get => new Vector2(npc.ai[2], npc.ai[3]);
            set
            {
                npc.ai[2] = value.X;
                npc.ai[3] = value.Y;
            }
        }

        private readonly Vector2[] TrailPos = new Vector2[3];
        private Vector2 StingerPos
        {
            get
            {
                Vector2 offset = (npc.spriteDirection == 1 ? new Vector2(npc.width - 4, 102) : new Vector2(4, 102));
                return npc.position + offset.RotatedBy(npc.rotation, npc.Size / 2);
            }
        }
        private float TrailRot = 0;
        private int TrailAlpha = 0;

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (ShouldFlee())
                return;

            Texture2D texture = mod.GetTexture("NPCs/LunarBee/LunarBeeGlow");
            Vector2 drawPos = npc.Center + Vector2.UnitY * 4 - Main.screenPosition;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / 4) * 0.5f);
            Rectangle frame = new Rectangle(0, npc.frame.Y, texture.Width, texture.Height / 4);

            spriteBatch.Draw(
                texture,
                drawPos,
                frame,
                Color.White * 0.5f,
                npc.rotation,
                drawOrigin,
                1f,
                npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f);

            TrailAlpha -= 4;
            for (int i = 0; i < 3; i++)
            {
                spriteBatch.Draw(
                    texture,
                    TrailPos[i] - Main.screenPosition,
                    new Rectangle(0, i * 100, texture.Width, texture.Height / 4),
                    Color.White * ((TrailAlpha + i * 30) / 255f),
                    TrailRot,
                    drawOrigin,
                    1f,
                    TrailRot > MathHelper.Pi ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally,
                    0f);
            }
        }
        public override void AI()
        {
            // A local function for changing phases.
            int maxPhase = 3;
            void ChangePhases()
            {
                npc.netUpdate = true;

                if (Main.expertMode) // in expert mode, automatically be able to reach phase 3 (dash)
                    maxPhase++;
                if (npc.life < npc.lifeMax / 2) // normal below 50%: start charging; expert below 50%: start ZA WARUDOing
                    maxPhase++;

                Phase += 1;
                Phase %= maxPhase;
                PhaseTimer = 0;
            }

            // Some basic sets.
            PhaseTimer++;
            npc.spriteDirection = npc.direction = npc.Center.X < Target.Center.X ? 1 : -1;
            npc.rotation = npc.velocity.X / 20f;

            int dustType = mod.DustType<MoonFire>();

            if (ShouldFlee())
            {
                npc.velocity.Y -= 0.05f;
                npc.rotation += npc.localAI[0] += Helper.RadiansPerTick(0.25f);
                npc.dontTakeDamage = true;
                return;
            }
            npc.dontTakeDamage = false;

            if (Phase == overhead)
            {
                Vector2 goTo = Target.Center - new Vector2(0, 150);

                // Move away from the player quickly if the player gets close
                if (npc.Distance(goTo) < 40)
                {
                    npc.velocity = -npc.GoTo(goTo, 0.2f, 4f);
                }

                // And if too far away / can't see, get closer
                bool canSee = Collision.CanHit(npc.Center, 1, 1, Target.position, Target.width, Target.height);
                if (npc.Distance(goTo) > 100 || !canSee)
                {
                    npc.velocity = npc.GoTo(goTo, 0.15f, 6f);
                }

                // As life decreases, increase fire speed
                float expertMult = npc.lifeMax * (Main.expertMode ? 2f : 1f);
                int cycleLength = (int)MathHelper.Lerp(70, 120, npc.life / expertMult);
                int cycleTime = PhaseTimer % cycleLength;

                // Spawn some spiraling-inward dust as a channeling.
                Vector2 position = Target.Center - new Vector2(0, 280);
                if (cycleTime > 40 && cycleTime < cycleLength)
                {
                    // Offset gets closer as it comes inward
                    Vector2 offset = new Vector2(0, cycleLength - cycleTime);
                    offset = offset.RotatedBy(cycleTime / 5d); // Offset the offset by a rotation

                    Dust dust = Dust.NewDustPerfect(position - offset, dustType, Vector2.Zero, 100, Scale: 1.5f);
                    dust.noGravity = true;
                    dust = Dust.NewDustPerfect(position - offset.RotatedBy(Math.PI), dustType, Vector2.Zero, 100, Scale: 1.5f);
                    dust.noGravity = true;
                }

                if (cycleTime == 0)
                {
                    npc.netUpdate = true;
                    // Dust explosion!
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDustPerfect(position, dustType, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 4), 50);
                    }
                    Main.PlaySound(SoundID.Item17, position);

                    if (Main.netMode != 1) // Multiplayer!
                        Projectile.NewProjectile(position, new Vector2(0, 11), mod.ProjectileType<CrystalChunk>(), dmgOverhead, 1);
                }

                // Change phases after 5 firings
                if (cycleTime == 0 && PhaseTimer > cycleLength * numOverhead)
                {
                    ChangePhases();
                    return;
                }
            }

            else if (Phase == smallchunks)
            {
                Vector2 goTo = Target.Center - new Vector2((npc.life < npc.lifeMax / 2) ? -300 : 300, 200);

                // And if too far away / can't see, get closer
                bool canSee = Collision.CanHit(npc.Center, 1, 1, Target.position, Target.width, Target.height);
                if (npc.Distance(goTo) > 50 || !canSee)
                {
                    npc.velocity = npc.GoTo(goTo, 0.2f, 6f);
                }

                // As life decreases, increase fire speed
                float expertMult = npc.lifeMax * (Main.expertMode ? 2f : 1f);
                int cycleLength = (int)MathHelper.Lerp(70, 130, npc.life / expertMult);
                int cycleTime = PhaseTimer % cycleLength;

                // For 20 ticks,
                if (cycleTime >= 45 && cycleTime < 70)
                {
                    // For every 4th tick (totalling 5 callings), spawn a projectile if not a multiplayer client.
                    if (cycleTime % 5 == 0)
                    {
                        npc.netUpdate = true;
                        Vector2 spreaded = npc.Center.To(Target.Center).RotatedByRandom(0.3f);
                        Main.PlaySound(SoundID.Item17, StingerPos);

                        if (Main.netMode != 1)
                            Projectile.NewProjectile(StingerPos + spreaded * 30, spreaded * 8.5f, mod.ProjectileType<CrystalShard>(), dmgSmallChunks, 1);
                    }
                }

                // Change phases after 8 firings
                if (cycleTime == 0 && PhaseTimer > cycleLength * numSmallChunks)
                {
                    ChangePhases();
                    return;
                }
            }

            else if (Phase == summon)
            {
                // Max of X bees (X+1 in expert)
                bool canSpawnWasps = NPC.CountNPCS(mod.NPCType<Lunacrita>()) < numSummon + Main.expertMode.ToInt();
                if (PhaseTimer > 360) // If max bees and can't spawn crystals yet (or time has run out), switch
                {
                    ChangePhases();
                    return;
                }

                if (Main.netMode != 1 && PhaseTimer >= 60 && PhaseTimer <= 240)
                {
                    if (!canSpawnWasps)
                    {
                        if (PhaseTimer % 30 == 0)
                        {
                            Vector2 position = npc.Center + Main.rand.NextVector2CircularEdge(100, 100);
                            Vector2 awayBody = npc.Center.To(position, 6);
                            Projectile.NewProjectile(position, awayBody, mod.ProjectileType<LunaBubble>(), dmgSummon, 1);
                        }
                    }
                    else
                    {
                        if (PhaseTimer % 90 == 0)
                        {
                            Vector2 butt = npc.position + (npc.spriteDirection == 1 ? new Vector2(npc.width - 14, 70) : new Vector2(14, 70));
                            NPC wasp = Main.npc[NPC.NewNPC((int)butt.X, (int)butt.Y, mod.NPCType<Lunacrita>(), ai1: npc.whoAmI, Target: npc.target)];
                            wasp.damage = dmgSummon;
                            wasp.velocity = npc.Center.To(butt, 5);
                            wasp.alpha = 0;
                            wasp.dontTakeDamage = false;
                        }
                    }
                }

                npc.velocity *= 0.97f;
            }

            else if (Phase == dashing)
            {
                // As life decreases, increase fire speed
                float expertMult = npc.lifeMax * (Main.expertMode ? 2f : 1f);
                int cycleLength = (int)MathHelper.Lerp(120, 200, npc.life / expertMult);
                int cycleTime = PhaseTimer % cycleLength;

                Vector2 dirToPlayer = npc.Center.To(DashEndPos);
                Vector2 endPosition = DashEndPos + dirToPlayer * 350;

                if (cycleTime > 0 && cycleTime < 80)
                {
                    Vector2 aroundPlayer = new Vector2(0, 350).RotatedBy(PhaseTimer / 120f);
                    npc.velocity = npc.Center.To(Target.Center + aroundPlayer, 6f);
                }
                else if (cycleTime < 0.8f * cycleLength)
                {
                    Vector2 aroundPlayer = new Vector2(0, 350).RotatedBy(PhaseTimer / 120f);
                    npc.velocity = npc.Center.To(Target.Center + aroundPlayer, 2f);
                }
                else
                {
                    npc.velocity *= 0.8f;
                }

                float dustStart = cycleLength * 0.55f;
                if (cycleTime > dustStart)
                {
                    float dashLerp = (cycleTime - dustStart) / (cycleLength - dustStart); // how close to the end pos the dust is
                    Vector2 dustSpawn = Vector2.Lerp(npc.Center, endPosition, dashLerp * 4 % 1);

                    Dust dust = Dust.NewDustPerfect(dustSpawn, dustType, Vector2.Zero, 50, Scale: 1.75f);
                    dust.noGravity = true;

                    dustSpawn = Vector2.Lerp(npc.TopLeft, endPosition - npc.Size / 2, dashLerp * 4 % 1);
                    dust = Dust.NewDustPerfect(dustSpawn, dustType, Vector2.Zero, 50, Scale: 1.25f);
                    dust.noGravity = true;

                    dustSpawn = Vector2.Lerp(npc.BottomRight, endPosition + npc.Size / 2, dashLerp * 4 % 1);
                    dust = Dust.NewDustPerfect(dustSpawn, dustType, Vector2.Zero, 50, Scale: 1.25f);
                    dust.noGravity = true;
                }

                // If dash is ready
                if (cycleTime == 0)
                {
                    npc.netUpdate = true;

                    float a = 0;

                    // Hurt our Target if they're in the collision line
                    foreach (var player in Main.player)
                    {
                        bool hit = Collision.CheckAABBvLineCollision(
                            player.position,
                            player.Size,
                            npc.Center,
                            endPosition,
                            200,
                            ref a);

                        if (hit)
                        {
                            int hitDirection = npc.Center.X > player.Center.X ? -1 : 1;
                            player.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), dmgDash * (Main.expertMode.ToInt() + 1), hitDirection);
                            player.AddBuff(mod.BuffType<LunarBreakdown>(), 120);
                        }
                    }

                    // Set some trail positions to fade out
                    TrailPos[0] = Vector2.Lerp(npc.Center, endPosition, 0.25f);
                    TrailPos[1] = Vector2.Lerp(npc.Center, endPosition, 0.50f);
                    TrailPos[2] = Vector2.Lerp(npc.Center, endPosition, 0.75f);
                    TrailRot = npc.Center.To(endPosition).ToRotation();
                    TrailAlpha = 100;

                    // Teleport
                    npc.Center = endPosition;
                }

                if (cycleTime < 0.8f * cycleLength)
                {
                    DashEndPos = Target.Center;
                }

                // Change phases after X dashes
                if (cycleTime == 1 && PhaseTimer > cycleLength * numDash)
                {
                    ChangePhases();
                    return;
                }
            }

            else if (Phase == DIOknives)
            {
                // Similar movement to smallchunks
                Vector2 goTo = Target.Center - new Vector2((npc.life < npc.lifeMax / 4) ? -350 : 350, 200);

                bool canSee = Collision.CanHit(npc.Center, 1, 1, Target.position, Target.width, Target.height);
                if (npc.Distance(goTo) > 50 || !canSee)
                {
                    npc.velocity = npc.GoTo(goTo, 0.2f, 6f);
                }

                // As life decreases, increase fire speed
                float expertMult = npc.lifeMax * (Main.expertMode ? 2f : 1f);
                int cycleLength = (int)MathHelper.Lerp(65, 200, npc.life / expertMult);
                int cycleTime = PhaseTimer % cycleLength;

                // Pew pew
                if (Main.netMode != 1 && cycleTime == cycleLength - 1)
                {
                    npc.netUpdate = true;
                    int numCrystals = Main.rand.Next(12, 17) + Main.expertMode.ToInt() * 3;

                    Helper.FireInCircle(StingerPos, numCrystals, mod.ProjectileType<CrystalShard>(), dmgDIO, 5f, ai0: 1);
                }

                // Change phases after X knifethrows
                if (cycleTime == 0 && PhaseTimer > cycleLength * numDIO)
                {
                    ChangePhases();
                    return;
                }
            }

            else
            {
                Phase = 0;
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            ErilipahWorld.downedLunaemia = true;
            name = "Lunaemia";
            potionType = ItemID.LesserHealingPotion;
            if (Main.expertMode)
            {
                npc.DropBossBags();
            }
            else
            {
                Loot.DropItem(npc, mod.ItemType("SynthesizedLunaesia"), 16, 24, 100);
            }
        }

        private bool ShouldFlee() => Main.dayTime || Main.player.All(plr => !plr.active || plr.dead || plr.Distance(npc.Center) > 5000);

        public override void FindFrame(int frameHeight)
        {
            const int ticksPerFrame = 5;
            ++npc.frameCounter;
            if (npc.frameCounter % ticksPerFrame == 0)
            {
                npc.frame.Y += frameHeight;
                npc.frame.Y %= 4 * frameHeight;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunaemia");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 2800;
            npc.defense = 8;
            npc.damage = 16;
            npc.knockBackResist = 0f;

            npc.boss = true;
            npc.aiStyle = 0;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            npc.width = 86;
            npc.height = 104;

            npc.value = Item.buyPrice(0, 3, 0, 0);

            npc.MakeBuffImmune(mod.BuffType<LunarBreakdown>());

            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/LunarBee");
            bossBag = mod.ItemType<LunarBeeBag>();
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.6 * Math.Max(1, numPlayers * 0.8f));
            npc.defense = (int)(npc.defense * 1.15);
        }
    }

    public class Lunacrita : ModNPC
    {
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawNPC(spriteBatch, drawColor);
            npc.DrawGlowmask(spriteBatch, "NPCs/LunarBee/LunacritaGlow", Color.White * 0.5f);
            return false;
        }

        public override void SetDefaults()
        {
            npc.width = 42;
            npc.height = 36;

            npc.defense = 2;
            npc.lifeMax = 20;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 0;
            npc.damage = 15;
            npc.knockBackResist = 0.3f;
            npc.lavaImmune = true;
            npc.noGravity = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunacrita");
            Main.npcFrameCount[npc.type] = 4;
        }

        private bool HoverAroundMother => npc.life < npc.lifeMax * 0.65f && NPC.AnyNPCs(mod.NPCType<LunarBee>());
        public override void AI()
        {
            #region Rotation
            npc.rotation = npc.velocity.X / 11;
            if (npc.rotation > MathHelper.PiOver2 / 2) npc.rotation = MathHelper.PiOver2 / 2;
            if (npc.rotation < -MathHelper.PiOver2 / 2) npc.rotation = -MathHelper.PiOver2 / 2;
            npc.spriteDirection = npc.direction;
            npc.TargetClosest(true);
            #endregion

            NPC mom = Main.npc[(int)npc.ai[1]];
            Player target = Main.player[npc.target];
            Vector2 stinger = npc.position + (npc.spriteDirection == 1 ? new Vector2(npc.width - 3, 38) : new Vector2(3, 38));

            bool canhit = Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height);

            if (HoverAroundMother)
            {
                npc.noTileCollide = true;
                npc.velocity = npc.GoTo(mom.Center, 0.2f);
            }
            else if (canhit)
            {
                npc.noTileCollide = false;
                npc.velocity = npc.GoTo(Main.player[mom.target].Center - new Vector2(0, 100), 0.2f);
            }
            else
            {
                npc.noTileCollide = true;
                npc.velocity = npc.GoTo(Main.player[mom.target].Center - new Vector2(0, 200), 0.2f);
            }
            npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * (-18 * Helper.MPH), Vector2.One * (18 * Helper.MPH));

            if (++npc.ai[0] % 240 == 0 && canhit)
            {
                npc.netUpdate = true;
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(
                        stinger,
                        Helper.To(stinger, target.Center, 35 * Helper.MPH),
                        mod.ProjectileType("CrystalShardSmall"),
                        npc.damage / 2,
                        1,
                        ai0: 1);
                }
            }
        }
        public override void FindFrame(int frameHeight)
            => npc.Animate(frameHeight, 8, 4);

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemID.Heart);
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
            //ErilipahWorld.downedLunarBee && spawnInfo.player.GetModPlayer<ErilipahPlayer>().ZoneLunarBiome ?
            //SpawnCondition.Sky.Chance * 0.09f : 0f;
        }
    }

    public class CrystalChunk : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 74;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.penetrate = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 80;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 1;
            DisplayName.SetDefault("Lunar Crystal Chunk");
        }

        public override void AI()
        {
            projectile.alpha = (int)MathHelper.Lerp(0, 255, projectile.timeLeft / 60f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (projectile.timeLeft < 55)
            {
                projectile.tileCollide = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != 1)
                Helper.FireInCircle(projectile.Center, Main.rand.Next(9, 13), mod.ProjectileType("CrystalShardSmall"), projectile.damage / 2, 20 * Helper.MPH, kb: 0);
            Main.PlaySound(0, projectile.position);
            for (int a = 0; a < 16; a++)
                Dust.NewDust(projectile.Center, projectile.width, projectile.height, mod.DustType("MoonFire"), Scale: 1 + (a / 10.5f));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(mod.BuffType("LunarBreakdown"), 260);
        }
    }
    public class CrystalShard : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 40;
            projectile.friendly = false;
            projectile.penetrate = 2;
            projectile.hostile = true;
            projectile.ranged = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunar Crystal");
            Main.projFrames[projectile.type] = 1;
        }

        public override void AI()
        {
            if (projectile.ai[0] == 1)
            {
                if (projectile.timeLeft > 290) // Set rotation so that it just doesn't change
                    projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (projectile.timeLeft > 200)
                {
                    if (projectile.velocity.Length() > 0.001f) // make sure that it doesn't fucking stop entirely
                        projectile.velocity *= 0.945f;
                }
                if (projectile.timeLeft < 180)
                {
                    projectile.velocity *= 1.085f;
                }
            }
            else
            {
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int a = 0; a < 4; a++)
                Dust.NewDust(projectile.Center, projectile.width, projectile.height, mod.DustType("MoonFire"));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.expertMode)
                target.AddBuff(mod.BuffType<LunarBreakdown>(), 140);
        }
    }
    public class CrystalShardSmall : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 22;
            projectile.friendly = false;
            projectile.penetrate = 1;
            projectile.hostile = true;
            projectile.ranged = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunar Crystal");
            Main.projFrames[projectile.type] = 1;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Kill(int timeLeft)
        {
            for (int a = 0; a < 6; a++)
                Dust.NewDust(projectile.Center, projectile.width, projectile.height, mod.DustType("MoonFire"));
        }
    }
    public class LunaBubble : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.hostile = true;
            projectile.ranged = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;

            projectile.timeLeft = 300;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunaemia");
            Main.projFrames[projectile.type] = 14;
        }

        public override void AI()
        {
            projectile.velocity *= 0.8f;
            if (projectile.timeLeft > 60)
                projectile.Animate(60 / 13, 4);
            else
                projectile.Animate(60 / 13, 14);

            int time = Main.expertMode ? 15 : 30;
            if (Main.netMode != 1 && projectile.timeLeft < 200 && projectile.timeLeft % time == 0)
            {
                Projectile.NewProjectile(projectile.Center, Main.rand.NextVector2Unit() * 8, mod.ProjectileType<CrystalShardSmall>(), projectile.damage / 2, 1);
            }
        }
        public override void Kill(int timeLeft)
        {
            // Dust explosion!
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<MoonFire>(), Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 4), 50);
            }
        }
    }
}