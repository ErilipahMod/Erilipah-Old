using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonPistol : NewModItem
    {
        protected override int Damage => 48;
        protected override int[] UseSpeedArray => new int[] { 22, 22 };
        protected override float Knockback => 4;

        protected override bool AutoReuse => false;
        protected override int[] Dimensions => new int[] { 56, 40 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Gun;
        protected override bool FiresProjectile => true;
        protected override string Tooltip => "Hold for 2.5 seconds to fire a large, deadly salvo guaranteed to ignite foes\n" +
            "Cancelling the charge will release a smaller, less powerful bolt\n" +
            "'Who called this monstrosity a PISTOL?'\n";

        protected override int ShootType => mod.ProjectileType<PhlogistonPistolProjProj>();
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type != ProjectileID.Bullet)
            {
                Main.PlaySound(SoundID.Item11, player.Center);
                return true;
            }
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), ShootType, damage, knockBack, player.whoAmI);
            return false;
        }
        protected override float ShootSpeed => 12;
        protected override bool Channel => true;
        public override void SetDefaults()
        {
            base.SetDefaults();
            item.UseSound = default;
        }
        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 8 }, { Terraria.ID.ItemID.HellstoneBar, 7 } };
        protected override int CraftingTile => TileID.Anvils;

        protected override Vector2? HoldoutOffSet => new Vector2(-8, 2);
        protected override Vector2 ShootPosOffset => new Vector2(0, -10);
        protected override float ShootDistanceOffset => 2;
    }
    public class PhlogistonPistolProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 10, 14 };
        protected override int DustType => mod.DustType("DeepFlames");
        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override int TimeLeft => 20; //vel 6

        protected override float Gravity => 0;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 22;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
    public class PhlogistonPistolProjProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 10, 20 };

        protected override int DustType => mod.DustType("DeepFlames");
        protected override int Pierce => 2;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override bool[] DamageTeam => new bool[] { false, false };

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => done ? DustTrailTypes.PerfectNoGravity : DustTrailTypes.None;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (4));
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.alpha = 255;
            projectile.timeLeft = 30;
        }
        #region Charge
        // The maximum charge value
        private const float MaxChargeValue = 60 * 2.5f;
        //The distance charge particle from the player center
        private const float MoveDistance = 80;
        private Vector2 offSetY = new Vector2(-10, -20);

        // The actual distance is stored in the ai0 field
        // By making a property to handle this it makes our life easier, and the accessibility more readable
        private float Distance
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }

        // The actual charge value is stored in the localAI0 field
        private float Charge
        {
            get { return projectile.localAI[0]; }
            set { projectile.localAI[0] = value; }
        }

        private bool done = false;

        private bool Charging => Main.player[projectile.owner].channel && !done;

        // Are we at max charge? With c#6 you can simply use => which indicates this is a get only property
        public bool AtMaxCharge { get { return Charge == MaxChargeValue; } }

        private Vector2 MousePos => Main.MouseWorld;

        protected override float? Rotation => null;

        private void ReachMaxCharge()
        {
            Main.PlaySound(SoundID.Item11, player.Center);
            projectile.alpha = 0;
            projectile.damage = (int)(300 * player.rangedDamage);
            projectile.friendly = true;

            Vector2 velocity = MousePos - player.Center;
            velocity.Normalize();
            velocity *= 18;

            projectile.velocity = velocity;
            player.channel = false;

            if (!Collision.CanHit(projectile.Center, 1, 1, player.Center, 1, 1))
            {
                projectile.Kill();
                return;
            }

            const int degreeSpread = 6;
            int damage = (int)(48 * player.rangedDamage);
            float numberProjectiles = 5; // 3, 4, or 5 shots
            float rotation = MathHelper.ToRadians(degreeSpread * numberProjectiles);
            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 perturbedSpeed = projectile.velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))); // Watch out for dividing by 0 if there is only 1 projectile.
                perturbedSpeed.Normalize();
                perturbedSpeed *= 9f;
                if (perturbedSpeed.ToRotation() != velocity.ToRotation())
                {
                    Projectile.NewProjectile(projectile.position, perturbedSpeed, mod.ProjectileType<PhlogistonPistolProj>(), damage, 1, player.whoAmI);
                }
            }
        }

        private void AfterCharged()
        {

        }

        private void CancelCharge()
        {
            Main.PlaySound(SoundID.Item11, player.Center);
            projectile.friendly = true;

            Vector2 velocity = MousePos - player.Center;
            velocity.Normalize();
            velocity *= 12;

            player.itemAnimation = 22;
            player.itemTime = 22;

            player.channel = false;

            if (!Collision.CanHit(projectile.Center, 1, 1, player.Center, 1, 1))
            {
                projectile.Kill();
                return;
            }

            Projectile proj = Projectile.NewProjectileDirect(projectile.position + new Vector2(0, 3), velocity, mod.ProjectileType<PhlogistonPistolProj>(),
                24, 1, projectile.owner);
            proj.damage += (int)Charge / 2;
            proj.timeLeft = 300;
            projectile.Kill();
        }
        public override void AI()
        {
            base.AI();
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(270);
            Lighting.AddLight(projectile.Center, 0.2f, 0, 0);

            Player player = Main.player[projectile.owner];

            if (projectile.ai[1] == 0)
            {
                #region Set projectile position
                // Multiplayer support here, only run this code if the client running it is the owner of the projectile
                if (projectile.owner == Main.myPlayer && !AtMaxCharge)
                {
                    Vector2 diff = MousePos - player.Center;
                    diff.Normalize();
                    projectile.velocity = diff;
                    projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                    projectile.netUpdate = true;
                }
                if (!AtMaxCharge)
                {
                    projectile.position = player.Center + projectile.velocity * MoveDistance + Vector2.UnitY * offSetY.Y;
                    projectile.timeLeft = TimeLeft;
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
                if (!player.channel && !AtMaxCharge)
                {
                    CancelCharge();
                }
                else if (!AtMaxCharge)
                {
                    Vector2 offset = projectile.velocity;
                    offset *= MoveDistance - 20;
                    Vector2 pos = player.Center + offset + offSetY;
                    if (Charge < MaxChargeValue)
                    {
                        Charge++;
                    }
                    int chargeFact = (int)(Charge / 30f);
                    Vector2 dustVelocity = Vector2.UnitX * 18f;
                    dustVelocity = dustVelocity.RotatedBy(projectile.rotation - MathHelper.PiOver2, default(Vector2));
                    Vector2 spawnPos = projectile.Center + dustVelocity;
                    for (int k = 0; k < chargeFact + 1; k++)
                    {
                        Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - (chargeFact * 2));
                        Dust dust = Main.dust[Dust.NewDust(pos, 20, 20, DustType, projectile.velocity.X / 2f,
                            projectile.velocity.Y / 2f, 0, default(Color), 1f)];
                        dust.velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f - chargeFact * 2f) / 10f;
                        dust.noGravity = true;
                        dust.scale = Main.rand.Next(10, 20) * 0.05f;
                    }
                }
                #endregion
                if (AtMaxCharge && !done && projectile.owner == Main.myPlayer)
                {
                    done = true;
                    ReachMaxCharge();
                }
                if (done)
                {
                    AfterCharged();
                }
            }
            else
            {
                AfterCharged();
            }
            GetFrame(true);
        }

        private int GetFrame(bool inc = false)
        {
            if (inc)
                projectile.frameCounter++;

            const int chargeLengthSecs = (int)(MaxChargeValue / 60);
            const int frameCount = 5;
            const int frameLength = 60 / (frameCount / chargeLengthSecs);

            if (projectile.frameCounter < frameLength * 1)
                return 0;
            else if (projectile.frameCounter < frameLength * 2)
                return 1;
            else if (projectile.frameCounter < frameLength * 3)
                return 2;
            else if (projectile.frameCounter < frameLength * 4)
                return 3;
            else
                return 4;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Charging)
            {
                lightColor = Sanguine.SanguineDominatorProj.BarColor;
                Texture2D texture = ModContent.GetTexture(mod.Name + "/Items/Phlogiston/PhlogistonUI");

                Vector2 position = (Main.player[projectile.owner].Center - Vector2.UnitY * Sanguine.SanguineDominatorProj.BarHeight);
                Rectangle rect = texture.Frame(1, 5, 0, GetFrame());

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / 5) * 0.5f);
                Vector2 drawPos = position - Main.screenPosition;

                spriteBatch.Draw(texture, drawPos, rect, lightColor, 0f, drawOrigin, 1f, SpriteEffects.None, 0f);
            }
        }
        #endregion
    }
}
