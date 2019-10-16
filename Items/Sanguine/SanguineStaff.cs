using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine
{
    public class SanguineStaff : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 48;

            item.damage = 19;
            item.knockBack = 10f;
            item.magic = true;

            item.useTime = 16;
            item.useAnimation = 16;
            item.mana = 11;
            item.noMelee = true;
            item.useStyle = 5;
            item.UseSound = SoundID.Item8;
            item.autoReuse = true;
            item.channel = true;

            item.rare = 3;
            item.shoot = ProjectileType<SanguineStaffProj>();
            item.shootSpeed = 9.5f;
        }
        public override void SetStaticDefaults()
        {
            Item.staff[item.type] = true;
            Tooltip.SetDefault("Left click for a greater bolt which heals 33% of damage dealt" +
                "\nGreater bolt costs 3 times more mana; damage halves after first hit" +
                "\nSmaller bolt heals you for 15% of damage dealt");
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "SanguineAlloy", 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse != 2 && player.statMana > item.mana * 4)
            {
                player.statMana -= item.mana * 3;
                const float speed = 1.5f;
                speedX *= speed;
                speedY *= speed;

                damage = 68;

                knockBack = 10;

                type = ProjectileType<GreaterSanguineStaffProj>();
                return true;
            }
            return true;
        }
    }

    public class SanguineStaffProj : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.penetrate = 2;
            projectile.width = 24;
            projectile.height = 12;
            projectile.magic = true;
            projectile.timeLeft = 180;
            projectile.friendly = true;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Staff");
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal((int)(damage * 0.15f));
            }
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(180);
            Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Sanguine"), Scale: 0.85f);
            Lighting.AddLight(projectile.Center, 0.2f, 0, 0);

            if (projectile.FindClosestNPC(200) >= 0)
            {
                projectile.GoTo(Main.npc[projectile.FindClosestNPC(200)].Center, 0.07f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Sanguine"), Scale: 1f);
            }
        }
    }
    public class GreaterSanguineStaffProj : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.penetrate = 3;
            projectile.width = 48;
            projectile.height = 20;
            projectile.magic = true;
            projectile.friendly = false;
            projectile.tileCollide = false;

            projectile.alpha = 255;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Staff");
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal(damage / 3);
            }
        }


        // The maximum charge value
        private const float MaxChargeValue = 150;
        //The distance charge particle from the player center
        private const float MoveDistance = 120f;

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

        // Are we at max charge? With c#6 you can simply use => which indicates this is a get only property
        public bool AtMaxCharge { get { return Charge == MaxChargeValue; } }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(180);
            Lighting.AddLight(projectile.Center, 0.2f, 0, 0);

            Vector2 mousePos = Main.MouseWorld;
            Player player = Main.player[projectile.owner];

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
                Vector2 pos = player.Center + offset - new Vector2(15, 10);
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
                    Dust dust = Main.dust[Dust.NewDust(pos, 30, 30, mod.DustType("Sanguine"), projectile.velocity.X / 2f,
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
                Main.PlaySound(SoundID.Item8, player.Center);
                projectile.alpha = 0;
                projectile.friendly = true;

                Vector2 velocity = mousePos - player.Center;
                velocity.Normalize();
                velocity *= 12.5f;

                projectile.velocity = velocity;
                player.channel = false;
                projectile.tileCollide = true;
                projectile.friendly = true;
            }
            if (done && projectile.FindClosestNPC(200) >= 0)
            {
                projectile.GoTo(Main.npc[projectile.FindClosestNPC(200)].Center, 0.1f);
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

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Charging && projectile.ai[1] == 0)
            {
                lightColor = SanguineDominatorProj.BarColor;
                Texture2D texture = ModContent.GetTexture(mod.Name + "/Items/Sanguine/SanguineUI");

                Vector2 position = (Main.player[projectile.owner].Center - Vector2.UnitY * SanguineDominatorProj.BarHeight);
                Rectangle rect = texture.Frame(1, 5, 0, GetFrame());

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / 5) * 0.5f);
                Vector2 drawPos = position - Main.screenPosition;

                spriteBatch.Draw(texture, drawPos, rect, lightColor, 0f, drawOrigin, 1f, SpriteEffects.None, 0f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("Sanguine"), Scale: 1.3f);
            }
        }
    }
}