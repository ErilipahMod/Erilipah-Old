using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguineDominator : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 46;
            item.height = 24;

            item.damage = 18;
            item.ranged = true;
            item.knockBack = 0.3f;
            item.channel = true;

            item.useTime = 18;
            item.useAnimation = 15;
            item.autoReuse = false;
            item.noMelee = true;
            item.useStyle = 5;
            item.UseSound = SoundID.Item11;
            item.rare = ItemRarityID.Orange;

            item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 11;
            item.useAmmo = AmmoID.Bullet;
        }

        public override void SetStaticDefaults()
            => Tooltip.SetDefault("Right click to fire normally" +
                "\nHold left click for 2 seconds to fire a spread of life-stealing bolts" +
                "\nThe bolts heal you for 33% of damage dealt and consume no ammunition");
        public override bool AltFunctionUse(Player player) => true;
        public override Vector2? HoldoutOffset() => new Vector2(-6, 1.5f);

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse != 2)
            {
                damage = (int)(damage * 0.9f);
                Vector2 velocity = new Vector2(speedX, speedY);
                Projectile.NewProjectile(position, velocity, mod.ProjectileType("SanguineDominatorProj"),
                    damage, knockBack, player.whoAmI);
                return false;
            }
            damage = (int)(damage * 0.4f);
            return true;
        }

        public override bool ConsumeAmmo(Player player)
        {
            return player.altFunctionUse == 2;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SanguineAlloy"), 8);
            recipe.AddIngredient(ItemID.Handgun);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class SanguineDominatorProj : ModProjectile
    {
        public override string Texture => mod.Name + "/Items/Sanguine/SanguineStaffProj";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Sanguine Dominator");
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 12;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.tileCollide = false;

            projectile.alpha = 255;
        }

        // The maximum charge value
        private const float MaxChargeValue = 120;
        //The distance charge particle from the player center
        private const float MoveDistance = 70f;

        // The actual charge value is stored in the localAI0 field
        private float Charge
        {
            get { return projectile.localAI[0]; }
            set { projectile.localAI[0] = value; }
        }

        private bool done = false;

        // Are we at max charge? With c#6 you can simply use => which indicates this is a get only property
        private bool AtMaxCharge => Charge == MaxChargeValue;
        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(180);
            Lighting.AddLight(projectile.Center, 0.2f, 0, 0);

            Vector2 mousePos = Main.MouseWorld;
            Player player = Main.player[projectile.owner];

            if (projectile.ai[1] == 0)
            {
                #region Set projectile position
                // Multiplayer support here, only run this code if the client running it is the owner of the projectile
                if (projectile.owner == Main.myPlayer && !AtMaxCharge)
                {
                    Vector2 diff = mousePos - player.Center;
                    diff.Normalize();
                    projectile.velocity = diff;
                    projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                    projectile.netUpdate = true;
                }
                if (!AtMaxCharge)
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
                if (!player.channel && !AtMaxCharge)
                {
                    projectile.Kill();
                }
                else if (!AtMaxCharge)
                {
                    Vector2 offset = projectile.velocity;
                    offset *= MoveDistance - 20;
                    Vector2 pos = player.Center + offset - new Vector2(10, 10);
                    if (Charge < MaxChargeValue)
                    {
                        Charge++;
                    }
                    int chargeFact = (int)(Charge / 30f);
                    Vector2 dustVelocity = Vector2.UnitX * 18f;
                    dustVelocity = dustVelocity.RotatedBy(projectile.rotation - 1.57f, default(Vector2));
                    Vector2 spawnPos = projectile.Center + dustVelocity;
                    for (int k = 0; k < chargeFact + 1; k++)
                    {
                        Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - (chargeFact * 2));
                        Dust dust = Main.dust[Dust.NewDust(pos, 20, 20, mod.DustType("Sanguine"), projectile.velocity.X / 2f,
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
                    Main.PlaySound(SoundID.Item11, player.Center);
                    projectile.alpha = 0;
                    projectile.friendly = true;

                    Vector2 velocity = mousePos - player.Center;
                    velocity.Normalize();
                    velocity *= 12.5f;

                    projectile.velocity = velocity;
                    player.channel = false;
                    projectile.tileCollide = true;

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 perturbedSpeed = projectile.velocity.RotatedByRandom(MathHelper.ToRadians(12));
                        float scale = 1f - (Main.rand.NextFloat() * .3f);
                        perturbedSpeed = perturbedSpeed * scale;

                        Projectile.NewProjectile(projectile.position, perturbedSpeed,
                            projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0, 1);
                    }
                }
                if (done && projectile.FindClosestNPC(200) >= 0)
                {
                    projectile.GoTo(Main.npc[projectile.FindClosestNPC(200)].Center, 0.1f);
                }
            }
            else
            {
                if (projectile.FindClosestNPC(200) >= 0)
                {
                    projectile.GoTo(Main.npc[projectile.FindClosestNPC(200)].Center, 0.1f);
                }
                projectile.tileCollide = true;
                projectile.friendly = true;
                projectile.alpha = 0;
            }
            GetFrame(true);
        }

        private bool Charging => Main.player[projectile.owner].channel && !done;

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

        public const int BarHeight = 60;
        public static readonly Color BarColor = new Color(1f, 1f, 1f, 1f);
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Charging && projectile.ai[1] == 0)
            {
                lightColor = BarColor;
                Texture2D texture = ModContent.GetTexture(mod.Name + "/Items/Sanguine/SanguineUI");

                Vector2 position = (Main.player[projectile.owner].Center - Vector2.UnitY * BarHeight);
                Rectangle rect = texture.Frame(1, 5, 0, GetFrame());

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / 5) * 0.5f);
                Vector2 drawPos = position - Main.screenPosition;

                spriteBatch.Draw(texture, drawPos, rect, lightColor, 0f, drawOrigin, 1f, SpriteEffects.None, 0f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Sanguine"), Scale: 1f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal(damage / 3);
            }
            target.immune[projectile.owner] = 2;
        }
    }
}