using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Weapons
{
    public class HeadSplitter : NewModItem
    {
        protected override string Tooltip => "Throws extremely fast headsplitters";
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 40, 40 };
        protected override int Rarity => ItemRarityID.Orange;

        protected override int Damage => 30;
        protected override int[] UseSpeedArray => new int[] { 15, 15 };
        protected override float Knockback => 3;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9;
    }
    public class HeadSplitterProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => mod.DustType<Crystalline.CrystallineDust>();
        protected override int MaxMotionBlurLength => 4;

        protected override int Pierce => InfinitePierceAndBounce;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.ItemCopy;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override int TrailThickness => 8;

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
        #region Sticking in enemies
        protected override float? Rotation
        {
            get
            {
                if (!IsStickingToTarget)
                    return projectile.rotation + Helper.RadiansPerTick(2.5f) * RotateDirection;
                return null;
            }
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            if (IsStickingToTarget)
            {
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

        public bool IsStickingToTarget
        {
            get { return projectile.ai[0] == 1f; }
            set { projectile.ai[0] = value ? 1f : 0f; }
        }

        public float targetWhoAmI
        {
            get { return projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit,
            ref int hitDirection)
        {
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
            projectile.velocity =
                (target.Center - projectile.Center);
            IsStickingToTarget = true;
            targetWhoAmI = target.whoAmI;
            projectile.netUpdate = true; // netUpdate this javelin
        }
        public override bool? CanHitNPC(NPC target)
        {
            return !IsStickingToTarget && !target.townNPC;
        }

        public override void AI()
        {
            base.AI();
            DropItem = false;
            if (IsStickingToTarget)
            {
                projectile.ignoreWater = true;
                projectile.tileCollide = false;
                projectile.localAI[0]++;

                int projTargetIndex = (int)targetWhoAmI;
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
        #endregion
    }
}
