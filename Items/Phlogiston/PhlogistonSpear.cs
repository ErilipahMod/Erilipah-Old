using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonSpear : NewModItem
    {
        protected override int Damage => 42;
        protected override int[] UseSpeedArray => new int[] { 30, 25 };
        protected override float Knockback => 5;

        protected override float ShootSpeed => 3;
        protected override bool FiresProjectile => true;
        protected override int[] Dimensions => new int[] { 38, 38 };
        protected override int Rarity => 3;
        protected override string Tooltip => "Fires a projectile on use";

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 9 }, { Terraria.ID.ItemID.HellstoneBar, 8 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;

        protected override UseTypes UseType => UseTypes.Spear;
        public override bool CanUseItem(Player player)
        {
            // Ensures no more than one spear can be thrown out, use this when using autoReuse
            base.CanUseItem(player);
            return player.ownedProjectileCounts[item.shoot] < 1;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            Projectile.NewProjectile(position.X, position.Y, speedX * 3, speedY * 3,
                mod.ProjectileType<PhlogistonSpearProjProj>(), 25, 2, player.whoAmI);
            return true;
        }
    }
    public class PhlogistonSpearProj : ModProjectile
    {
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 30;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.aiStyle = 19;
            projectile.penetrate = -1;
            projectile.scale = 1f;
            projectile.alpha = 0;

            projectile.hide = true;
            projectile.ownerHitCheck = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.friendly = true;
        }

        // In here the AI uses this example, to make the code more organized and readable
        // Also showcased in ExampleJavelinProjectile.cs
        private float MovementFactor // Change this value to alter how fast the spear moves
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }

        // It appears that for this AI, only the ai0 field is used!
        public override void AI()
        {
            // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            // Sadly, Projectile/ModProjectile does not have its own
            Player plr = Main.player[projectile.owner];
            // Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
            Vector2 ownerMountedCenter = plr.RotatedRelativePoint(plr.MountedCenter, true);
            projectile.direction = plr.direction;
            plr.heldProj = projectile.whoAmI;
            plr.itemTime = plr.itemAnimation;
            projectile.position.X = ownerMountedCenter.X - projectile.width / 2;
            projectile.position.Y = ownerMountedCenter.Y - projectile.height / 2;
            // As long as the player isn't frozen, the spear can move
            if (!plr.frozen)
            {
                if (MovementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    MovementFactor = 3f; // Make sure the spear moves forward when initially thrown out
                    projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (plr.itemAnimation < plr.itemAnimationMax / 2) // Somewhere along the item animation, make sure the spear moves back
                {
                    MovementFactor -= 2.4f;
                }
                else // Otherwise, increase the movement factor
                {
                    MovementFactor += 2.1f;
                }
            }
            // Change the spear position based off of the velocity and the movementFactor
            projectile.position += projectile.velocity * MovementFactor;
            // When we reach the end of the animation, we can kill the spear projectile
            if (plr.itemAnimation == 0)
            {
                projectile.Kill();
            }
            // Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
            // MathHelper.ToRadians(xx degrees here)
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(135);
            // Offset by 90 degrees here
            if (projectile.spriteDirection == -1)
            {
                projectile.rotation -= MathHelper.ToRadians(90); //utb 90
            }
        }
    }
    public class PhlogistonSpearProjProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 10, 16 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override DamageTypes DamageType => DamageTypes.Melee;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 30;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}
