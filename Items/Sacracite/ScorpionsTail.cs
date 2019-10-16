using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class ScorpionsTail : ModItem
    {
        private const int Damage = 16;
        private const int UseSpeed = 15;
        private const float Knockback = 1;
        private const float ShootSpeed = 9;
        private static readonly int[] Dimensions = new int[] { 14, 36 };
        private new string DisplayName = "Scorpion's Tail";
        private new string Tooltip = null;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.SwingThrow;
            item.maxStack = 999;

            // most changed
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.thrown = true;
            item.noUseGraphic = true;
            item.consumable = true;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 333);
            r.AddRecipe();
        }
    }
    public class ScorpionsTailProj : ModProjectile
    {
        private int Pierce => 3;

        private int Dust => DustType<GreenGemDust>();

        private int FlightTime => 30;

        private float Gravity => 0.3285f;

        public override string Texture
        {
            get
            {
                string name = GetType().Name.ToString();
                string Namespace = GetType().Namespace.Replace('.', '/') + '/';
                return Namespace + name.Remove(name.Length - 4);
            }
        }

        public override void SetDefaults()
        {
            projectile.width =
                projectile.height = 34;
            projectile.friendly = true;
            projectile.thrown = true;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;
            if (++projectile.ai[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
                projectile.rotation += Helper.RadiansPerTick(3) * direction;
            }
            else
            {
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            }
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
