using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class DesertConquerer : ModItem
    {
        private const int Damage = 14;
        private const int UseSpeed = 19;
        private const float Knockback = 1;
        private const float ShootSpeed = 12;
        private static readonly int[] Dimensions = new int[] { 28, 48 };
        private new string DisplayName = "Desert Conqueror";
        private new string Tooltip = "Fires super fast Raze Arrows if wooden arrows are used as ammo" +
            "\nRaze Arrows bounce and pierce enemies twice and are unaffected by gravity" +
            "\n'It cuts through the air itself!'";

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.HoldingOut;
            item.maxStack = 1;

            // most changed
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.ranged = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item5;
            item.useAmmo = AmmoID.Arrow;
            item.shoot = mod.ProjectileType("RazeArrow"); //mod.ProjectileType(GetType().Name.ToString() + "Proj")
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-1, 2);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                const float speed = 1.5f;
                type = item.shoot;

                speedX *= speed;
                speedY *= speed;
            }

            return true;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType<SacraciteIngot>(), 2);
            r.AddIngredient(mod.ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class RazeArrow : ModProjectile
    {
        private int Pierce => 2;

        private int Dust => mod.DustType<GreenGemDust>();

        private int FlightTime => 0;

        private float Gravity => 0;

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 38;
            projectile.friendly = true;
            projectile.thrown = true;
            projectile.timeLeft = 180;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (++projectile.ai[1] < 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        Dust, projectile.velocity.X / 10, projectile.velocity.Y / 10, Scale: 0.92f);
                }

                if (projectile.velocity.X != oldVelocity.X)
                    projectile.velocity.X = -oldVelocity.X;

                if (projectile.velocity.Y != oldVelocity.Y)
                    projectile.velocity.Y = -oldVelocity.Y;
            }
            else
                projectile.Kill();
            return false;
        }

        public override void AI()
        {
            if (++projectile.ai[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
            }
            for (int i = 0; i < 6; i++)
            {
                Terraria.Dust.NewDustPerfect(projectile.Center, Dust);
            }

            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust, Scale: 0.92f);
            }
        }
    }
}
