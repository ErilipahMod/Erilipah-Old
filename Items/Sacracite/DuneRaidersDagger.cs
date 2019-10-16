using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class DuneRaidersDagger : ModItem
    {
        private const int Damage = 22;
        private const int Crit = 5;
        private const int UseSpeed = 24;
        private const float Knockback = 4;
        private const float ShootSpeed = 9.5f;
        private static readonly int[] Dimensions = new int[] { 28, 30 };
        private new string DisplayName = "Dune Raider's Dagger";
        private new string Tooltip = null;
        private const bool Staff = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.SwingThrow;
            item.maxStack = 1;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.thrown = true;
            item.noUseGraphic = true;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = AmmoID.None;
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
    public class DuneRaidersDaggerProj : ModProjectile
    {
        private int Pierce => 1;

        private int Dust => DustType<GreenGemDust>();

        private int FlightTime => 20;

        private float Gravity => 0.23085f;

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
            }

            projectile.rotation += Helper.RadiansPerTick(3) * direction;
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
