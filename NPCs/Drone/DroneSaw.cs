using Erilipah.NPCs.Dracocide;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Drone
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class DroneSaw : NewModNPC
#pragma warning restore CS0618 // Type or member is obsolete
    {
        protected override string Title => "Rogue Buzzsaw Drone";
        protected override int[] Dimensions => new int[] { 40, 40 };
        protected override int MaxLife => 160;
        protected override int Damage => 30;
        protected override int Defense => 4;
        protected override float KnockbackResist => 0.15f;

        protected override LegacySoundStyle HitSound => SoundID.NPCHit4;
        protected override LegacySoundStyle DeathSound => SoundID.NPCDeath14;

        protected override int[] ImmuneToDebuff => new int[] { BuffID.OnFire };
        protected override Vector2 GoreVelocity => Main.rand.NextVector2Unit() * 6;
        protected override string GorePath => "Gores/Saw";
        protected override int[] Gores => new int[] { 1, 2, 2 };

        protected override int NPCFrameCount => 2;
        protected override int FrameDelay => 3;
        protected override int MaxMotionBlurLength => 5;

        private bool PlayerTarget
        {
            get
            {
                int h = npc.FindClosestNPC(1000, mod.NPCType<Observer>(), mod.NPCType<ArcCaster>());
                if (h > -1)
                {
                    npc.target = h;
                    return false;
                }
                return true;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            MotionBlurActive = true;
        }

        protected override int Animate(int frameHeight) => (int)AnimationTypes.AutoCycle;

        public override void AI()
        {
            base.AI();
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

            MotionBlurActive = Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) >= 9;
            MotionBlurLength = 4;

            if (Terraria.Main.dayTime)
            {
                npc.velocity.Y -= 0.3f;
                npc.noTileCollide = true;
                return;
            }
            if (Terraria.Main.time % 120 == 0)
                npc.netUpdate = true;

            float fuckoffX = Math.Abs(npc.Center.X - TCen.X);
            float fuckoffY = Math.Abs(npc.Center.Y - TCen.Y);

            if (fuckoffX < 40 && fuckoffY > 150)
                npc.GoTo(TCen, 0.5f);
            else
                npc.GoTo(TCen - new Vector2(0, 200), 0.35f);
            if (npc.velocity.Y > 2 && npc.collideY)
            {
                for (int i = 0; i < 7; i++)
                {
                    Main.dust[Dust.NewDust(npc.BottomLeft, npc.width, 1, 0, 2.5f)].velocity.Y = -5;
                }
            }

            foreach (var enemy in Main.npc)
            {
                if (Collision.CheckAABBvAABBCollision(npc.position,
                    new Vector2(npc.width, npc.height),
                    enemy.position,
                    new Vector2(enemy.width, enemy.height)))
                {
                    enemy.StrikeNPC(npc.damage, 1, 0);
                }
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

                npc.velocity = new Vector2(hitDirection * 7.5f, -3);
                npc.noGravity = false;
                npc.netUpdate = true;
            }
            else if (!gores)
            {
                SpawnGore();
                gores = true;
            }
            MotionBlurActive = false;
        }

        private bool droppedLoot = false;
        public override void NPCLoot()
        {
            if (dead != -1 && !droppedLoot)
            {
                droppedLoot = true;
                Loot.DropItem(npc, mod.ItemType("PowerCoupling"), 1, 1, 65, 1f);
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Terraria.NPC.downedBoss1 ? SpawnCondition.OverworldNightMonster.Chance * 0.08f : 0;
        }
    }
}