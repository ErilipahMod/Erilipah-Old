using Microsoft.Xna.Framework;
using Terraria;
namespace Erilipah.Projectiles
{
    public abstract class SpearProjectile : NewModProjectile
    {
        protected override int Pierce => -2;
        protected override bool TileCollide => false;
        protected override float Gravity => 0;
        protected override int Bounce => -2;
        protected override float? Rotation => null;
        protected override DamageTypes DamageType => DamageTypes.Melee;

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.aiStyle = 19;
            projectile.ownerHitCheck = true;
            projectile.hide = true;
        }

        protected abstract float MoveBackTimePercent { get; }
        protected abstract float MoveSpeed { get; }
        protected virtual float MoveBackSpeed => MoveSpeed;
        private float MovementFactor
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }
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
                    MovementFactor = MoveSpeed; // Make sure the spear moves forward when initially thrown out
                    projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (plr.itemAnimation < plr.itemAnimationMax * MoveBackTimePercent) // Somewhere along the item animation, make sure the spear moves back
                {
                    MovementFactor -= MoveBackSpeed;
                }
                else // Otherwise, increase the movement factor
                {
                    MovementFactor += MoveSpeed;
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
                projectile.rotation -= MathHelper.ToRadians(90);
            }
        }
    }
}