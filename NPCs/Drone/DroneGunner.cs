using Erilipah.NPCs.Dracocide;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Drone
{
    public class DroneGunner : NewModNPC
    {
        protected override string Title => "Combat Drone";

        protected override int MaxLife => 330;
        protected override int Damage => 10;
        protected override int Defense => 4;
        protected override float KnockbackResist => 0.5f;
        protected override float ScaleExpertDmg => 1.5f;

        protected override LegacySoundStyle HitSound => SoundID.NPCHit4;
        protected override LegacySoundStyle DeathSound => SoundID.NPCDeath14;

        protected override int[] ImmuneToDebuff => new int[] { BuffID.OnFire, BuffID.Frostburn, BuffID.CursedInferno, BuffID.ShadowFlame };
        protected override int[] Dimensions => new int[] { 38, 38 };
        protected override int NPCFrameCount => 1;

        protected override Vector2 GoreVelocity => Main.rand.NextVector2Unit() * 6f;
        protected override string GorePath => "Gores/Gunner";
        protected override int[] Gores => new int[] { 2, 2, 1 };
        protected override bool NoGravity => true;

        private bool Overdrive
        {
            get { return npc.ai[1] == 1; }
            set { npc.ai[1] = value ? 1 : 0; }
        }

        private bool PlayerTarget
        {
            get
            {
                int h = Helper.FindClosestNPC(
                    npc,
                    2000,
                    mod.NPCType<Observer>(),
                    mod.NPCType<ArcCaster>(),
                    mod.NPCType<Swarmer>(),
                    mod.NPCType<MiniSwarmer>(),
                    mod.NPCType<AssaultDrone>());
                if (h > -1)
                {
                    npc.target = h;
                    return false;
                }
                return true;
            }
        }

        private new Entity Target
        {
            get
            {
                if (PlayerTarget)
                    return Main.player[npc.target];

                return Main.npc[npc.target];
            }
        }

        private bool TargetInSight => Collision.CanHit(npc.Center, 0, 0, Target.Center, 0, 0);

        private Vector2 ToPlayer => (Target.Center - npc.Center).SafeNormalize(Vector2.Zero) * -1;

        private float DesiredRotation
        {
            get
            {
                return npc.life <= npc.lifeMax * 0.1 ?
                    npc.velocity.ToRotation() + MathHelper.ToDegrees(180) :
                    ToPlayer.ToRotation();
            }
        }

        private bool lockedOnTarget = false;
        private bool startup = false;
        private bool directionRight = false;
        private int timer = 0;
        private Vector2 shakePos;

        private void Setup()
        {
            if (Main.rand.NextBool(25))
            {
                Overdrive = true;
            }
            npc.netUpdate = true;
        }
        public override void AI()
        {
            base.AI();
            if (!startup)
            {
                startup = true;
                Setup();
            }
            timer++;
            // Every 270 ticks, set the shaking position at the NPC center.
            if (timer % 270 == 0)
            {
                shakePos = npc.Center;
            }
            // Then, if timer / 330 remainder is less than 50 [AKA for an extra 50 ticks],
            // set the position to around the previously set position + a tiny random shake.
            if (timer % 350 <= 50)
            {
                if (Main.rand.NextBool(2))
                    Dust.NewDustPerfect(npc.Center, 6, Main.rand.NextVector2Unit() * 4, Scale: 0.8f);
                // I don't add multiplayer sync for the randomized movement because it's so minor and affects nothing.
                npc.Center = shakePos + Main.rand.NextVector2Unit() * Main.rand.NextFloat(10);
                npc.velocity *= 0.8f; // Stop struggling, whore
                return; // Run no more code; don't fire, rotate, or move.
            }
            // For another 30 ticks,
            else if (timer % 350 <= 80)
            {
                npc.Center = shakePos; // Stop shaking.
                return; // Run no more code.
            }

            npc.dontTakeDamageFromHostiles = false;
            npc.spriteDirection = npc.direction = npc.Center.X < Target.Center.X ? 1 : 1;

            if (Main.rand.NextBool(10))
            {
                Dust.NewDustPerfect(npc.Center, 6, Main.rand.NextVector2Unit() * 4, Scale: 0.8f);
            }
            if (Main.rand.NextBool(40))
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Smoke);
            }

            if (Overdrive)
            {
                if (timer > 300 && npc.life > 1)
                {
                    CombatText combatText = null;
                    timer = 0;
                    combatText = Main.combatText[CombatText.NewText(npc.getRect(),
                        CombatText.DamagedHostileCrit, "CS" + Main.rand.Next(1000, 10000))];
                    combatText.lifeTime = 90;
                    combatText.scale = 1f;
                }
                if (Main.rand.NextBool(4))
                    Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, DustID.Fire, 0, 0, 0, default(Color), 1.3f)].noGravity = true;
            }
            #region Falling out of sky
            if (dead != -1)
            {
                dead++;
                npc.noTileCollide = false;
                npc.rotation += Helper.RadiansPerTick(2);

                npc.buffImmune[BuffID.OnFire] = false;
                npc.onFire = true;

                if (dead == 80 || npc.collideY || npc.collideX || (npc.velocity.Y == 0 && dead > 20))
                {
                    Kill(0, 1);
                }
                return;
            }
            #endregion
            #region Movement
            float mSpeed = Overdrive ? 0.4f : 0.15f;

            Vector2 toPlayerUp = TCen - new Vector2(0, 100) - npc.Center;
            toPlayerUp.Normalize();

            if (TDist < 170 && TargetInSight || Main.dayTime)
            {
                npc.velocity += ToPlayer * mSpeed;
                if (Main.dayTime)
                    npc.noTileCollide = true;
            }
            else if (TDist > 240 || (!TargetInSight && TDist > 140))
            {
                if (Target is Player)
                    npc.velocity += toPlayerUp * mSpeed;
                else
                    npc.velocity += npc.Center.To(ToPlayer + new Vector2(50, 50), mSpeed);
            }
            else
            {
                npc.velocity *= 0.8f;
            }
            #endregion

            if (!lockedOnTarget)
            {
                npc.ai[0] = 0;
                float RotationSpeed = Helper.RadiansPerTick(0.4f);
                float Leverage = 0.1f;
                bool OverRotation =
                    (npc.rotation < DesiredRotation && npc.rotation + RotationSpeed > DesiredRotation) ||
                    (npc.rotation > DesiredRotation && npc.rotation - RotationSpeed < DesiredRotation);

                if (Math.Abs(DesiredRotation - npc.rotation) < Leverage || OverRotation)
                {
                    npc.rotation = DesiredRotation;
                    lockedOnTarget = true;
                }

                bool compareInRadians = false;
                if (Math.Abs(DesiredRotation - MathHelper.ToRadians(270)) < MathHelper.ToRadians(5))
                    compareInRadians = true;

                if (!compareInRadians)
                {
                    if (MathHelper.ToDegrees(npc.rotation) < MathHelper.ToDegrees(DesiredRotation))
                    {
                        npc.rotation += RotationSpeed;
                    }
                    if (MathHelper.ToDegrees(npc.rotation) > MathHelper.ToDegrees(DesiredRotation))
                    {
                        npc.rotation -= RotationSpeed;
                    }
                }
                else
                {
                    if (npc.rotation < DesiredRotation)
                    {
                        npc.rotation += RotationSpeed;
                    }
                    if (npc.rotation > DesiredRotation)
                    {
                        npc.rotation -= RotationSpeed;
                    }
                }
            }
            else if (TargetInSight)
            {
                float speed = Overdrive ? 18 : 11;
                if (npc.ai[0] == 0)
                {
                    Main.PlaySound(SoundID.NPCDeath14, npc.Center);
                    npc.velocity -= ToPlayer * (Overdrive ? -1.5f : -2.25f);

                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.Center + ToPlayer * -30, ToPlayer * -speed,
                            mod.ProjectileType<DroneGunnerProj>(), Damage, 0, Main.myPlayer);

                    int type = mod.DustType("DeepFlames");
                    for (int i = 0; i < 3; i++)
                    {
                        Dust.NewDust(npc.Center, 0, 0, type, -ToPlayer.X * 6, -ToPlayer.Y * 6);
                    }
                    npc.netUpdate = true;
                }
                ++npc.ai[0];
                if (npc.ai[0] > (Overdrive ? 20 : 45))
                {
                    lockedOnTarget = false;
                    npc.ai[0] = 0;
                    npc.netUpdate = true;
                }
            }
            else if (Target.Center.Y - 40 > npc.Center.Y)
            {
                float diffX = Target.Center.X - npc.Center.X;
                if (diffX > 100)
                {
                    directionRight = true;
                }
                else if (directionRight && diffX < -100)
                {
                    directionRight = false;
                }
                int direction = directionRight ? 1 : -1;
                npc.velocity.X = direction * 10 * Helper.MPH;
                npc.rotation = !directionRight ? 0 : -MathHelper.Pi;
            }
        }

        private int dead = -1;
        private bool gores = false;
        protected override void OnKill(int hitDirection, double damage)
        {
            if (dead == -1)
            {
                npc.life = 1;
                npc.dontTakeDamage = true;
                dead = 0;

                npc.velocity = new Vector2(hitDirection * 5, -3);
                npc.noGravity = false;
                npc.netUpdate = true;

                CombatText combatText = Main.combatText[CombatText.NewText(npc.getRect(),
                    CombatText.DamagedHostileCrit, "FATAL ERROR: CS" + Main.rand.Next(1000, 10000))];
                combatText.lifeTime = 90;
                combatText.scale = 1.25f;
            }
            else if (!gores)
            {
                SpawnGore();
                gores = true;
            }
        }

        private bool droppedLoot = false;
        public override void NPCLoot()
        {
            if (dead != -1 && !droppedLoot)
            {
                npc.netUpdate = true;
                droppedLoot = true;
                Loot.DropItem(npc, mod.ItemType("PowerCoupling"), 1, 1, 50);
                if (Overdrive)
                    Loot.DropItem(npc, mod.ItemType("DetachedDroneBlaster"), 1, 1, 35);
                else
                    Loot.DropItem(npc, mod.ItemType("DetachedDroneBlaster"), 1, 1, 2);
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Terraria.NPC.downedBoss1 ? SpawnCondition.OverworldNightMonster.Chance * 0.06f : 0;
        }
    }
    public class DroneGunnerProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 12, 22 };
        protected override int DustType => DustID.AmberBolt;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override bool NoDustLight => true;
        protected override float TrailScale => 0.8f;
        protected override DamageTypes DamageType => DamageTypes.Hostile;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;
        protected override bool[] DamageTeam => new bool[] { true, true };

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.owner = Main.myPlayer;
            projectile.hostile = true;
            projectile.friendly = true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return target.type == mod.NPCType<DroneGunner>() ? false : true;
        }
    }
}