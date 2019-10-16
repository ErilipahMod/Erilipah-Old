using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteGrandstaff : ModItem
    {
        private const int Damage = 19;
        private const int Crit = 3;
        private const int UseSpeed = 19;
        private const float Knockback = 4;
        private const float ShootSpeed = 9.25f;
        private static readonly int[] Dimensions = new int[] { 40, 40 };
        private new string DisplayName = null;
        private new string Tooltip = null;
        private const bool Staff = true;
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
            item.useTime =
                item.useAnimation = UseSpeed;
            if (Shoots)
                item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.magic = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.mana = 13;
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item8;
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
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(ItemType<SacraciteIngot>(), 4);
            r.AddIngredient(ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }

    public class SacraciteGrandstaffProj : ModProjectile
    {
        private int Pierce => 0;

        private int Dust => DustType<GreenGemDust>();

        private int FlightTime => 0;

        private float Gravity => 0.198f;

        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.height = 20;
            projectile.friendly = true;
            projectile.magic = true;
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
            if (++projectile.localAI[1] <= 1)
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
            {
                projectile.ai[0] = 1;
                projectile.Kill();
            }
            return false;
        }

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;
            if (++projectile.localAI[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
            }

            Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust);

            projectile.rotation = projectile.velocity.ToRotation();
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust, Scale: 0.92f);
            }
            if (projectile.ai[0] != 1)
            {
                Helper.FireInCircle(projectile.Center, 5, ProjectileType<SacraciteGrandstaffProjProj>(), projectile.damage / 3, 10, owner: projectile.owner);
            }
            else
            {
                const int degSpread = 25;
                foreach (Projectile proj in Helper.FireInCircle(projectile.Center, 10, ProjectileType<SacraciteGrandstaffProjProj>(), projectile.damage / 3, 10))
                {
                    projectile.ai[1] = Main.rand.Next(-degSpread, degSpread + 1) + Main.rand.NextFloat() - Main.rand.NextFloat();
                    proj.velocity.RotatedBy(MathHelper.ToRadians(projectile.ai[1]));
                }
            }
        }
    }
    public class SacraciteGrandstaffProjProj : ModProjectile
    {
        private int Pierce => 0;

        private int Dust => DustType<GreenGemDust>();

        private int FlightTime => 0;

        private float Gravity => 0;

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.magic = true;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }
        /*public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (++projectile.ai[1] > 1)
            {
                if (projectile.velocity.X != oldVelocity.X)
                    projectile.velocity.X = -oldVelocity.X;

                if (projectile.velocity.Y != oldVelocity.Y)
                    projectile.velocity.Y = -oldVelocity.Y;
                for (int i = 0; i < 4; i++)
                {
                    Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        Dust, projectile.velocity.X, projectile.velocity.Y, Scale: 0.92f);
                }
            }
            else
                projectile.Kill();
            return false;
        }
        */

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;
            if (++projectile.ai[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
            }
            Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust);

            projectile.rotation = projectile.velocity.ToRotation();
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
