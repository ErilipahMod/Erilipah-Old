using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
namespace Erilipah.Projectiles
{
    public abstract class ChargeProjectile : NewModProjectile
    {
        protected bool done = false;

        private bool AtMaxCharge => Charge == MaxCharge;
        protected bool Charging => !done && Charge < MaxCharge;
        protected virtual bool Dusts => true;

        protected float Charge
        {
            get { return projectile.localAI[0]; }
            set { projectile.localAI[0] = value; }
        }
        protected abstract bool Cancel { get; }
        protected abstract float MaxCharge { get; }
        protected abstract float MoveDistance { get; }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.alpha = 255;
            projectile.friendly = projectile.hostile = false;
        }

        protected void ResumeVelocity(float speed = 10, LegacySoundStyle playSound = null)
        {
            if (playSound != null)
                Main.PlaySound(playSound, projectile.Center);

            projectile.friendly = DamageTeam[0];
            projectile.hostile = DamageTeam[1];
            projectile.alpha = 0;

            Vector2 velocity = Main.MouseWorld - player.Center;
            velocity.Normalize();
            velocity *= speed;

            projectile.velocity = velocity;
        }
        protected virtual void OnCancelCharge() { projectile.Kill(); }
        protected virtual void WhileCharging() { }
        protected virtual void OnFinishCharge() { ResumeVelocity(ItemSource != null ? ItemSource.shootSpeed : 10); }
        protected virtual void PostCharge() { }

        public override void AI()
        {
            base.AI();
            #region Set projectile position
            // Multiplayer support here, only run this code if the client running it is the owner of the projectile
            if (projectile.owner == Main.myPlayer && !AtMaxCharge && !done)
            {
                Vector2 diff = Main.MouseWorld - player.Center;
                diff.Normalize();
                projectile.velocity = diff;
                projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                projectile.netUpdate = true;
            }
            if (!AtMaxCharge && !done)
            {
                projectile.position = player.Center + projectile.velocity * MoveDistance;
                projectile.timeLeft = 300;
                int dir = projectile.direction;
                player.ChangeDir(dir);
                player.heldProj = projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.itemRotation = (float)Math.Atan2(projectile.velocity.Y * dir, projectile.velocity.X * dir);
            }
            #endregion
            #region Charging process
            // Kill the projectile if the player stops channeling
            if (Cancel && !AtMaxCharge && !done)
            {
                OnCancelCharge();
                done = true;
            }
            else if (!AtMaxCharge && !done)
            {
                if (Charge < MaxCharge)
                {
                    Charge++;
                }
                WhileCharging();

                if (!Dusts)
                    goto skip;

                Vector2 offset = projectile.velocity;
                offset *= MoveDistance - 20;
                Vector2 pos = player.Center + offset - new Vector2(10, 10);
                int chargeFact = (int)(Charge / 30f);
                Vector2 dustVelocity = Vector2.UnitX * 18f;
                dustVelocity = dustVelocity.RotatedBy(projectile.rotation - 1.57f, default);
                Vector2 spawnPos = projectile.Center + dustVelocity;
                for (int k = 0; k < chargeFact + 1; k++)
                {
                    Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - chargeFact * 2);
                    Dust dust = Main.dust[Dust.NewDust(pos, 17, 17, DustType, projectile.velocity.X / 2f,
                        projectile.velocity.Y / 2f, 0, default, 1f)];
                    dust.velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f - chargeFact * 2f) / 10f;
                    dust.noGravity = true;
                    dust.scale = Main.rand.Next(10, 20) * 0.05f;
                }
            skip:;
            }
            #endregion
            else if (AtMaxCharge && !done && projectile.owner == Main.myPlayer)
            {
                done = true;
                OnFinishCharge();
            }
            else if (done)
            {
                PostCharge();
            }
        }
    }
}