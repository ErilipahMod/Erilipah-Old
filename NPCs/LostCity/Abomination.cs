using Erilipah.Items.Crystalline;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.LostCity
{
    public class Abomination : ModNPC
    {
        private enum AttacksEnum : byte
        {
            None = 0,
            Dash = 1,
            Grab = 2,
            Spew = 4,
            Spit = 8,
        }

        private bool justSpawned = true;
        private bool blink = false;

        private Vector2 oldPos;
        private float rotateSpeed = 1;
        private int blurLength = 0;
        private int AttackTimer { get => (int)npc.ai[0]; set => npc.ai[0] = value; }
        private int AttackTypes { get => (int)npc.ai[1]; set => npc.ai[1] = value; }
        private int CurAttack { get => (int)npc.ai[2]; set => npc.ai[2] = value; }
        private float FrameY { get => npc.ai[3]; set => npc.ai[3] = value; }

        private Player Target { get => Main.player[npc.target]; set => npc.target = value.whoAmI; }
        private Vector2 Eye
        {
            get
            {
                int _rotDir = (npc.Center.X < Target.Center.X).ToDirectionInt();
                float _rot = npc.rotation + (_rotDir == -1 ? MathHelper.Pi : 0);
                return (npc.position + new Vector2(43, 26) * npc.scale).RotatedBy(_rot, npc.Center);
            }
        }

        public override void AI()
        {
            const int blinkLength = 30;
            const float accApproach = 0.275f;
            const float maxApproach = 3f;
            const float dashSpeed = 15;

            int _rotDir = (Eye.X < Target.Center.X).ToDirectionInt();
            npc.direction = npc.spriteDirection = _rotDir;
            AttackTimer++;

            // Set all the spawning shit, so that it "wakes up"
            if (justSpawned && Main.netMode != 1)
            {
                Setup();
            }
            if (blink)
            {
                Blink();
            }
            if (AttackTimer < 0)
            {
                WakeUp();
                return;
            }

            if (Target.dead && !Main.player.Any(p => p.active && !p.dead && p.Distance(npc.Center) < 2000))
            {
                npc.velocity.Y += 0.2f;
                return;
            }

            if (CurAttack <= 0) NewAttack();
            if (CurAttack == (int)AttacksEnum.Dash)
            {
                Dash(blinkLength, accApproach, maxApproach, dashSpeed);
            }
            if (CurAttack == (int)AttacksEnum.Grab)
            {
                Grab(accApproach, maxApproach);
            }
            if (CurAttack == (int)AttacksEnum.Spew)
            {
                Spew(accApproach, maxApproach);
            }
            if (CurAttack == (int)AttacksEnum.Spit)
            {
                Spit(accApproach, maxApproach);
            }
        }

        private void Grab(float accApproach, float maxApproach)
        {
            if (AttackTimer % 110 == 0)
            {
                blink = true;
            }

            Vector2 headPos = new Vector2(Target.Center.X, Target.Center.Y - 20);
            float distHead = Vector2.Distance(npc.Center, headPos);

            if (AttackTimer < 200)
            {
                rotateSpeed = npc.life;
                RotateToTarget();
                npc.velocity = npc.GoTo(headPos, accApproach + 0.2f, maxApproach * 1.35f);
                Dust.NewDustPerfect(Eye, DustType<VoidParticle>(), Vector2.Zero)
                    .customData = 0.1f;

                if (distHead < 30) // Grabbed him! jump over New Attack
                    AttackTimer = 201;
            }

            if (AttackTimer == 200)
            {
                NewAttack();
                return;
            }

            // If not grabbed, return
            if (AttackTimer <= 200)
                return;

            //npc.noTileCollide = false;
            if (npc.life < rotateSpeed * 0.60)
            {
                NewAttack();
            }

            if (AttackTimer == 201)
            {
                npc.velocity.Y -= 2f;
            }
            else if (AttackTimer < 300)
            {
                npc.velocity = npc.GoTo(npc.Center - new Vector2(0, 50), accApproach / 4f, maxApproach * 1.6f);
                npc.spriteDirection = rotateSpeed > npc.lifeMax * 0.5 ? 1 : -1;

                if (npc.rotation > MathHelper.Pi)
                    npc.rotation = MathHelper.Lerp(0, npc.rotation, 0.025f);
                else
                    npc.rotation = MathHelper.Lerp(npc.rotation, 0, 0.025f);

                Target.Center = npc.Center + new Vector2(0, Target.height / 2 + npc.height / 2 - 6);
                Target.velocity = Vector2.Zero;
            }
            else if (AttackTimer == 300)
            {
                Target.velocity = new Vector2(npc.scale * (rotateSpeed > npc.lifeMax * 0.5 ? 15 : -15), npc.scale * -8f);
                rotateSpeed = 0;
                RotateToTarget();
            }
            else if (Target.velocity.Y != 0 && Target.velocity.X != 0)
            {
                RotateToTarget();
                npc.velocity = npc.GoTo(headPos, accApproach - 0.1f, maxApproach * 0.8f);
                Target.GetModPlayer<ErilipahPlayer>().canJump = false;
                rotateSpeed += 1 / 4f;
            }
            else
            {
                Target.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage + (int)rotateSpeed, 0);
                AttackTimer = 0;
                npc.netUpdate = true;
                if (Main.rand.Chance(0.4f))
                {
                    NewAttack();
                }
            }
        }

        private void Spit(float accApproach, float maxApproach)
        {
            RotateToTarget();
            npc.velocity = npc.GoTo(
                Target.Center + new Vector2(0, 150).RotatedBy(AttackTimer / 100f), accApproach, maxApproach
                );

            if (AttackTimer % 70 == 0)
            {
                blink = true;
            }
            if (AttackTimer % 120 == 0 && Main.netMode != 1)
            {
                npc.netUpdate = true;
                //if (AttackTimer % 6 == 0)
                {
                    Main.PlaySound(npc.HitSound, npc.Center);
                    Vector2 vel = npc.Center.To(Target.Center, 5f);
                    //float range = Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8);
                    Projectile.NewProjectile(Eye, vel/*.RotatedBy(range)*/, ProjectileType<SpitBall>(), npc.damage / 2, 1);
                }
            }

            if (AttackTimer > 300)
            {
                AttackTimer = 0;
                npc.netUpdate = true;
                if (Main.rand.Chance(0.4f))
                {
                    NewAttack();
                }
            }
        }

        private void Spew(float accApproach, float maxApproach)
        {
            const float spewDuration = 500;
            const float spewRadius = 250;
            float size = MathHelper.SmoothStep(0, spewRadius, AttackTimer / 300f);
            size = Math.Min(spewRadius, size);

            RotateToTarget();
            if (npc.Distance(Target.Center) > spewRadius - 50)
                npc.velocity = npc.GoTo(Target.Center, accApproach, maxApproach / 3);
            else if (npc.Distance(Target.Center) < 100)
                npc.velocity = -npc.GoTo(Target.Center, accApproach, maxApproach / 3);
            else
                npc.velocity *= 0.95f;

            // hurt the player if they get close
            for (int i = 0; i < 255; i++)
            {
                Player player = Main.player[i];
                if (player.Distance(Eye) < size)
                    player.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage + Main.rand.Next(-10, 10), (player.Center.X > npc.Center.X).ToDirectionInt());
            }
            // spawn dusts
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Eye, DustType<CrystallineDust>(), Main.rand.NextVector2Unit() * 13);
                dust.customData = new Vector3(Eye, size + 10);
            }

            if (AttackTimer % 130 == 0) blink = true;
            if (AttackTimer > spewDuration)
            {
                AttackTimer = 0;
                npc.netUpdate = true;
                if (Main.rand.Chance(0.4f))
                {
                    NewAttack();
                }
            }
        }

        private void Dash(int blinkLength, float accApproach, float maxApproach, float dashSpeed)
        {
            // Rotate towards player & move towards them
            if (AttackTimer < 60)
            {
                bool isToLeft = npc.Center.X < Target.Center.X;
                RotateToTarget();
                npc.velocity = npc.GoTo(Target.Center + new Vector2(isToLeft ? -300 : 300, 0), accApproach, maxApproach);
                if (npc.Distance(Target.Center) > 400)
                    AttackTimer = 0;
            }
            // Start blinking
            else if (AttackTimer == 60)
            {
                Main.PlaySound(29, (int)npc.Center.X, (int)npc.Center.Y, 102, 0.7f, -0.825f);
                blink = true;
            }
            // Lock onto player
            else if (AttackTimer < 60 + blinkLength)
            {
                RotateToTarget();
                npc.velocity *= 0.97f;
                oldPos = npc.Center.To(Target.Center, dashSpeed);
            }
            // Dash
            else if (AttackTimer <= 90 + blinkLength)
            {
                npc.velocity = oldPos;
                blurLength = 4;
            }
            // Look at player
            else if (npc.Distance(Target.Center) > 300)
            {
                RotateToTarget();
                npc.velocity *= 0.9f;
                blurLength = 0;
            }

            if (npc.velocity.Length() < 3)
                blurLength = 0;

            // Ater 2/3 sec, restart
            if (AttackTimer > 160 + blinkLength)
            {
                blurLength = 0;
                npc.netUpdate = true;
                AttackTimer = 0;
                if (Main.rand.Chance(0.3f))
                {
                    NewAttack();
                }
            }
        }

        private void NewAttack()
        {
            npc.netUpdate = true;
            AttackTimer = 0;
            rotateSpeed = 0;
            do
            {
                CurAttack <<= 1;
                CurAttack %= 16;
                if (CurAttack <= 0) CurAttack = 1;
            }
            while ((CurAttack & AttackTypes) == 0);
        }

        private void RotateToTarget()
        {
            int __rotDir = (npc.Center.X > Target.Center.X).ToInt();
            float _rotTo = npc.Center.To(Target.Center).ToRotation() + MathHelper.Pi * __rotDir;
            npc.rotation = _rotTo;
        }

        private void WakeUp()
        {
            npc.velocity *= 0.95f;
            if (AttackTimer == -80) blink = true;
            if (AttackTimer >= -40)
            {
                RotateToTarget();
            }
            if (AttackTimer == -1) Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 2, 1, -0.12f * npc.scale);
        }

        private void Setup()
        {
            List<byte> _attackTypes = new List<byte> { 1, 2, 4, 8, 16 };
            AttackTypes = Main.rand.Next(_attackTypes); _attackTypes.Remove((byte)AttackTypes);
            AttackTypes |= Main.rand.Next(_attackTypes);
            if (Main.rand.Chance(0.33f))
            {
                _attackTypes.Remove((byte)AttackTypes);
                AttackTypes |= Main.rand.Next(_attackTypes);
            }

            npc.GivenName = Main.rand.Next(new string[]
                { "Abomination", "Monster", "Thing", "Amalgam", "Horror" }
            );

            float scale = Main.rand.NextFloat(0.9f, 1.085f);
            npc.scale = scale;
            npc.damage = (int)(npc.damage * scale);
            npc.defense = (int)(npc.defense * scale);
            npc.life = npc.lifeMax = (int)(npc.lifeMax * scale);
            npc.knockBackResist = 0.1f - (scale - 1);

            NewAttack();
            npc.velocity.Y -= 2f;
            blink = true;
            FrameY = 3;
            AttackTimer = -120;
            justSpawned = false;
        }

        private void Blink()
        {
            npc.netUpdate = true;
            npc.frameCounter++;
            npc.frameCounter %= 20;
            FrameY = (float)npc.frameCounter / 4;

            if (npc.frameCounter == 0 && Main.rand.Chance(0.75f))
                blink = false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return CurAttack != (int)AttacksEnum.Grab;
        }

        public override bool CheckDead()
        {
            if (npc.lastInteraction > -1)
            {
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<WarmedSoul>(), 0, 0, npc.lastInteraction);
            }
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawTrail(spriteBatch, blurLength, drawColor);
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.75f);
            return false;
        }
        public override void FindFrame(int frameHeight)
        {
            npc.frame.Y = (int)FrameY % 5 * frameHeight;
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleCrystalShard, Scale: npc.scale);
            }
            if (npc.life <= 0)
            {
                Target.fullRotation = default;

                string gore = "Gores/ERBiome/AbominationGore";
                for (int i = 0; i < 5; i++)
                {
                    Gore.NewGore(npc.Center, new Vector2(hitDirection * Main.rand.NextFloat() * 3, Main.rand.NextFloat() * -3), mod.GetGoreSlot(gore + i), npc.scale);
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleCrystalShard, Scale: npc.scale);
                }
                Gore.NewGore(npc.Center, new Vector2(hitDirection * Main.rand.NextFloat() * 3, Main.rand.NextFloat() * -3), mod.GetGoreSlot(gore + "3"), npc.scale);
                for (int i = 0; i < 2; i++)
                    Gore.NewGore(npc.Center, new Vector2(hitDirection * Main.rand.NextFloat() * 3, Main.rand.NextFloat() * -3), mod.GetGoreSlot(gore + "5"), npc.scale);
            }
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 306;
            npc.defense = 70;
            npc.damage = 53;
            npc.SetInfecting(2.65f);
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit19.WithPitchVariance(-0.4f);
            npc.DeathSound = SoundID.NPCDeath22.WithPitchVariance(-0.6f);

            npc.width = 50;
            npc.height = 48;

            npc.value = 0;

            // npc.MakeBuffImmune(BuffID.OnFire);
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 5;
            NPCID.Sets.TrailCacheLength[npc.type] = 4;
            NPCID.Sets.TrailingMode[npc.type] = 0;
        }
    }

    public class SpitBall : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Void Spew");
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.SetInfecting(2.0f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }
        public override void AI()
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, DustType<CrystallineDust>());
                dust.velocity = Main.rand.NextVector2Unit() * projectile.velocity.Length();
                dust.noGravity = true;

                dust = Dust.NewDustPerfect(projectile.Center, DustType<VoidParticle>(), Scale: 1.15f);
                dust.velocity = Main.rand.NextVector2Unit() * projectile.velocity.Length();
                dust.customData = new Vector3(projectile.Center, 60);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Blackout, Main.expertMode ? 300 : 100);
        }
    }
}
