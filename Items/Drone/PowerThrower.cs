using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Drone
{
    public class PowerThrower : NewModItem
    {
        protected override string Tooltip => "Sticks to enemies and explodes after a short time";
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 30, 30 };
        protected override int Rarity => 2;

        protected override int Damage => 17;
        protected override int[] UseSpeedArray => new int[] { 12, 12 };
        protected override float Knockback => 4.5f;
        protected override int Crit => 10;
        protected override bool? Consumable => true;
        protected override bool AutoReuse => false;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9.5f;

        protected override int[,] CraftingIngredients => new int[,] { { ItemType<PowerCoupling>(), 1 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
        protected override int CraftingResultAmount => 75;
    }
    public class PowerThrowerProj : NewModProjectile
    {
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Height = hitbox.Width = 18;
        }

        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => DustID.Blood;

        protected override int Pierce => 3;
        protected override int Bounce => 0;
        protected override float Gravity => 0.15f;
        protected override int FlightTime => 30;

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override int TrailThickness => 16;
        protected override float? Rotation
        {
            get
            {
                if (!isStickingToTarget)
                    return projectile.rotation + Helper.RadiansPerTick(2.2f) * RotateDirection;
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
            if (isStickingToTarget)
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

        public bool isStickingToTarget
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
            isStickingToTarget = true;
            targetWhoAmI = target.whoAmI;
            projectile.netUpdate = true; // netUpdate this javelin
        }
        public override bool? CanHitNPC(NPC target)
        {
            return !isStickingToTarget && !target.townNPC;
        }

        public override void AI()
        {
            base.AI();
            DropItem = false;
            if (isStickingToTarget)
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
    }
}