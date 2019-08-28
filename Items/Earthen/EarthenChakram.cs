using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenChakram : ModItem
    {
        private const int Damage = 12;
        private const int UseSpeed = 20;
        private const float Knockback = 2;
        private const int Crit = 6;

        private int[] Ingredient => new int[2] { mod.ItemType("SoilComposite"), 1 }; // change

        private int[] Dimensions => new int[2] { 22, 22 };

        private new string DisplayName = null;
        private new string Tooltip = null;
        private const float ShootSpeed = 9;
        private const bool Staff = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.SwingThrow;
            item.maxStack = 999;

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
            item.thrown = true;
            item.noUseGraphic = true;
            item.consumable = true;
            item.autoReuse = false;
            item.useTurn = true;
            item.channel = false;

            // occasionally changed integers
            item.rare = 0;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = Terraria.ID.AmmoID.None;
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
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(Ingredient[0], Ingredient[1]);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 120);
            r.AddRecipe();
        }
    }
    public class EarthenChakramProj : ModProjectile
    {
        private int[] Dimensions => new int[2] { 20, 20 };

        private int DustType => Terraria.ID.DustID.Grass;

        private const int Pierce = 1;
        private const int FlightTime = 20;
        private const float Gravity = 0.2f;
        private const bool TileCollide = true;

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
            projectile.width = Dimensions[0];
            projectile.height = Dimensions[1];
            projectile.tileCollide = TileCollide;
            projectile.friendly = true;
            projectile.thrown = true;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
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

            projectile.rotation += Helper.RadiansPerTick(3.75f * direction);
            //Terraria.Dust.NewDustPerfect(projectile.Center, DustType).noGravity = true;
        }
    }
}
