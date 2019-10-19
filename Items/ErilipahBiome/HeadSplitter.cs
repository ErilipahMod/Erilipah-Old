using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.ErilipahBiome
{
    public class HeadSplitter : NewModItem
    {
        protected override string Tooltip => "Throws extremely fast headsplitters";
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 32, 34 };
        protected override int Rarity => ItemRarityID.Pink;

        protected override int Damage => 45;
        protected override int[] UseSpeedArray => new int[] { 15, 15 };
        protected override float Knockback => 4.5f;
        protected override int Crit => 20;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 10;
    }
    public class HeadSplitterProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => DustType<Crystalline.CrystallineDust>();
        protected override int MaxMotionBlurLength => 4;

        protected override int Pierce => InfinitePierceAndBounce;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.ItemCopy;
        protected override DustTrailTypes DustTrailType => IsDead || IsStickingToTarget ? DustTrailTypes.None : DustTrailTypes.PerfectNoGravity;
        protected override int TrailThickness => 7;

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.extraUpdates = 1;
            MotionBlurActive = true;
            MotionBlurLength = 4;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = (int)(width / 1.25);
            height = (int)(width / 1.25);
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (IsStickingToTarget)
                return false;

            IsDead = true;

            projectile.extraUpdates = 0;

            if (projectile.velocity.X != oldVelocity.X)
                projectile.velocity.X = -oldVelocity.X;
            if (projectile.velocity.Y != oldVelocity.Y)
                projectile.velocity.Y = -oldVelocity.Y;

            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 4;

            projectile.netUpdate = true;

            return false;
        }

        protected override float? Rotation
        {
            get
            {
                if (!IsStickingToTarget)
                    return projectile.rotation + Helper.RadiansPerTick(2.2f) * RotateDirection + Main.rand.NextFloat(-0.09f, 0.09f);
                return null;
            }
        }

        public override void Kill(int timeLeft)
        {
            if (projectile.alpha < 250)
            {
                Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDustPerfect(projectile.Center, DustType, Main.rand.NextVector2Circular(16, 16)).noGravity = true;
                }
            }
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            if (IsStickingToTarget)
            {
                projectile.tileCollide = false;
                int npcIndex = (int)projectile.ai[1];
                if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
                {
                    if (Main.npc[npcIndex].behindTiles)
                        drawCacheProjsBehindNPCsAndTiles.Add(index);
                    else
                        drawCacheProjsBehindNPCs.Add(index);
                    return;
                }
            }
            drawCacheProjsBehindProjectiles.Add(index);
        }

        private bool IsDead
        {
            get => projectile.ai[0] == 2;
            set => projectile.ai[0] = value ? 2f : 0f;
        }

        private bool IsStickingToTarget
        {
            get => projectile.ai[0] == 1f && !IsDead;
            set => projectile.ai[0] = value ? 1f : 0f;
        }

        private float TargetWhoAmI
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit,
            ref int hitDirection)
        {
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
            projectile.velocity = target.Center - projectile.Center;
            IsStickingToTarget = true;
            TargetWhoAmI = target.whoAmI;
            projectile.netUpdate = true;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return !IsStickingToTarget && !target.townNPC;
        }

        public override void AI()
        {
            base.AI();
            DropItem = false;

            if (IsDead)
            {
                MotionBlurActive = false;
                projectile.damage = 0;
                projectile.friendly = projectile.hostile = false;
                projectile.velocity.Y += 0.04f;

                if (projectile.alpha < 120)
                    projectile.alpha = 120;
                projectile.alpha += 4;

                if (projectile.alpha >= 255)
                    projectile.Kill();
            }
            else if (IsStickingToTarget)
            {
                MotionBlurActive = false;
                projectile.ignoreWater = true;
                projectile.tileCollide = false;
                projectile.localAI[0]++;

                int projTargetIndex = (int)TargetWhoAmI;
                bool killProj = false;

                if ((projTargetIndex < 0 || projTargetIndex >= 200))
                {
                    killProj = true;
                }
                if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage)
                {
                    projectile.Center = Main.npc[projTargetIndex].Center - projectile.velocity * 2f;
                    projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
                    if (projectile.localAI[0] > 60 * 0.7)
                    {
                        Main.npc[projTargetIndex].StrikeNPC(projectile.damage + 10, projectile.knockBack, 0);
                        projectile.Kill();
                    }
                }
                else
                {
                    killProj = true;
                }

                if (killProj)
                {
                    projectile.Kill();
                }
            }
        }
    }
}
