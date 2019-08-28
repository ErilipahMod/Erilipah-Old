using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class AridImpaler : ModItem
    {
        private const int Damage = 19;
        private const int Crit = 6;
        private const int UseSpeed = 24;
        private const float Knockback = 5f;
        private const float ShootSpeed = 3.75f;
        private static readonly int[] Dimensions = new int[] { 40, 40 };
        private new string DisplayName = null;
        private new string Tooltip = null;
        private const bool Staff = false;
        private const bool Shoots = true;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.HoldingOut;
            item.maxStack = 1;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime = UseSpeed; //             CHANGE
            item.useAnimation = UseSpeed - 6;
            if (Shoots)
                item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.melee = true;
            item.noUseGraphic = true;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = Terraria.ID.AmmoID.None;
            if (Shoots)
                item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);

            Terraria.Item.staff[item.type] = Staff;
        }

        private int shoot;
        public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (shoot > 48)
            {
                shoot = 0;
                Projectile.NewProjectile(position, new Vector2(speedX, speedY) * 2.5f, mod.ProjectileType<SacraciteGrandstaffProjProj>(), (int)(damage * 0.85f), knockBack, player.whoAmI);
            }
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem.type != item.type)
            {
                ++shoot;
            }
        }
        public override void HoldItem(Player player)
        {
            shoot++;
        }

        public override bool CanUseItem(Player player)
        {
            // Ensures no more than one spear can be thrown out, use this when using autoReuse
            return player.ownedProjectileCounts[item.shoot] < 1;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType<SacraciteIngot>(), 3);
            r.AddIngredient(mod.ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }
    public class AridImpalerProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("AridImpaler");
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
        public float MovementFactor // Change this value to alter how fast the spear moves
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }

        // It appears that for this AI, only the ai0 field is used!
        public override void AI()
        {
            // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            // Sadly, Projectile/ModProjectile does not have its own
            Player projOwner = Main.player[projectile.owner];
            // Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            projectile.direction = projOwner.direction;
            projOwner.heldProj = projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;
            projectile.position.X = ownerMountedCenter.X - projectile.width / 2;
            projectile.position.Y = ownerMountedCenter.Y - projectile.height / 2;
            // As long as the player isn't frozen, the spear can move
            if (!projOwner.frozen)
            {
                if (MovementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    MovementFactor = 3f; // Make sure the spear moves forward when initially thrown out
                    projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2.75f) // Somewhere along the item animation, make sure the spear moves back
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
            if (projOwner.itemAnimation == 0)
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

            // These dusts are added later, for the 'ExampleMod' effect
            if (Main.rand.Next(3) == 0)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, mod.DustType<GreenGemDust>(),
                    projectile.velocity.X * .2f, projectile.velocity.Y * .2f, 200, Scale: 1.2f);
                dust.velocity += projectile.velocity * 0.3f;
                dust.velocity *= 0.2f;
            }
            if (Main.rand.Next(4) == 0)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, mod.DustType<GreenGemDust>(),
                    0, 0, 254, Scale: 0.3f);
                dust.velocity += projectile.velocity * 0.5f;
                dust.velocity *= 0.5f;
            }
        }
    }
}
