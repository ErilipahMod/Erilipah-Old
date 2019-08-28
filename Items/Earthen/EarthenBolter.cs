using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenBolter : ModItem
    {
        private const int Damage = 14;
        private const int UseSpeed = 38;
        private const float Knockback = 2;
        private const int Crit = 6;

        private int[] Ingredient => new int[2] { mod.ItemType("SoilComposite"), 6 };

        private int[] Dimensions => new int[2] { 22, 30 };

        private new string DisplayName = null;
        private new string Tooltip = "Flings a soil cube" +
            "\nThe cube can poison enemies";
        private const float ShootSpeed = 7;
        private const bool Staff = false;

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
            item.useTime = UseSpeed;
            item.useAnimation = UseSpeed;
            if (ShootSpeed >= 0)
                item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.ranged = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = false;
            item.useTurn = false;
            item.channel = false;

            // occasionally changed integers
            item.rare = 0;
            item.UseSound = Terraria.ID.SoundID.Item5;
            item.useAmmo = Terraria.ID.AmmoID.Arrow;
            if (ShootSpeed >= 0)
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
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 2);
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(Ingredient[0], Ingredient[1]);
            r.AddTile(TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                type = mod.ProjectileType("EarthenBolterProj");
            }
            return true;
        }
    }
    public class EarthenBolterProj : ModProjectile
    {
        private int[] Dimensions => new int[2] { 16, 16 };

        private int DustType => Terraria.ID.DustID.Grass;

        private const int Pierce = 1;
        private const int Bounce = 0;
        private const int FlightTime = 10;
        private const float Gravity = 0.085f;
        private const bool TileCollide = true;

        public override string Texture
        {
            get
            {
                return "Erilipah/Items/Earthen/SoilCube";
            }
        }
        public override void SetDefaults()
        {
            projectile.width = Dimensions[0];
            projectile.height = Dimensions[1];
            projectile.tileCollide = TileCollide;
            projectile.friendly = true;
            projectile.ranged = true;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (++projectile.ai[1] < Bounce)
            {
                for (int i = 0; i < 4; i++)
                {
                    Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        DustType, projectile.velocity.X / 10, projectile.velocity.Y / 10, Scale: 0.92f);
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
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType, Scale: 0.92f);
            }
        }

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;
            if (++projectile.localAI[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
            }

            projectile.rotation += Helper.RadiansPerTick(2.75f * direction);
            //Terraria.Dust.NewDustPerfect(projectile.Center, DustType).noGravity = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int chance = 7;
            if (Main.rand.NextBool(chance))
            {
                target.AddBuff(BuffID.Poisoned, 300);
            }
        }
    }
}
