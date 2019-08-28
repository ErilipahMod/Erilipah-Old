using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class CrescentMoon : ModItem
    {
        private int Damage => 15; //22

        private float Knockback => 1.5f;

        private int UseSpeed => 24; //20

        private float ShootSpeed => 8f;

        public override void SetDefaults()
        {
            // most important
            item.width = 48;
                item.height = 36;
            item.useStyle = 1;
            item.maxStack = 1;

            // most changed
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.crit = 10;
            item.noMelee = true;
            item.thrown = true;
            item.noUseGraphic = true;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = SoundID.Item1;
            item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }
        public override void SetStaticDefaults() => Tooltip.SetDefault("On critical strikes, the projectile with explode into five smaller bolts");

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType("SynthesizedLunaesia"), 6);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class CrescentMoonProj : ModProjectile
    {
        private int Pierce => 1;

        private int Dust => mod.DustType<MoonFire>();

        private int FlightTime => 15;

        public override string Texture
        {
            get
            {
                string name = GetType().Name.ToString();
                string Namespace = GetType().Namespace.Replace('.', '/') + '/';
                return Namespace + name.Remove(name.Length - 4);
            }
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 30;
            height = 30;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        public override void SetDefaults()
        {
            projectile.width = 48;
                projectile.height = 36;
            projectile.friendly = true;
            projectile.thrown = true;
            projectile.ai[0] = 2;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (--projectile.ai[0] >= 0)
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
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (crit)
            {
                Helper.FireInCircle(projectile.Center, 5, mod.ProjectileType<CrescentMoonProjProj>(),
                    projectile.damage, 8, 1.5f, owner: projectile.owner);
                projectile.Kill();
            }
        }

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;

            if (++projectile.ai[1] > FlightTime)
            {
                projectile.velocity.Y += 0.165f;
            }

            projectile.rotation += Helper.RadiansPerTick(3.75f * direction);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust, Scale: 0.92f);
            }
        }
    }
    public class CrescentMoonProjProj : ModProjectile
    {
        private int Dust => mod.DustType<MoonFire>();
        public override string Texture => "Terraria/Projectile_" + ProjectileID.ShadowBeamFriendly;
        public override void SetDefaults()
        {
            projectile.width =
                projectile.height = 4;
            projectile.friendly = true;
            projectile.thrown = true;
            projectile.timeLeft = 60;

            projectile.penetrate = 1;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 8));
        }

        public override void AI()
        {
            Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust, Scale: 0.92f);
        }
    }
}