using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class DuneDuster : ModItem
    {
        private const int Damage = 7;
        private const int Crit = 3;

        //const int UseSpeed = 19;
        private const float Knockback = 0.3f;
        private const float ShootSpeed = 10;
        private static readonly int[] Dimensions = new int[] { 28, 30 };
        private new string DisplayName = null;
        private new string Tooltip = "Gushes out sand to shroud your foes; extremely rapid";
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
            item.useTime = 5;
            item.useAnimation = 30;
            if (Shoots)
                item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.magic = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;
            item.useTurn = false;

            // occasionally changed integers
            item.mana = 15;
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item34;
            item.useAmmo = Terraria.ID.AmmoID.None;
            if (Shoots)
                item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-1.5f, 1);
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

            r.AddIngredient(mod.ItemType<SacraciteIngot>(), 2);
            r.AddIngredient(mod.ItemType<SacraciteCore>(), 2);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
    }
    public class DuneDusterProj : ModProjectile
    {
        private int DustType => mod.DustType<GreenGemDust>();

        private int DustType2 => mod.DustType<Sand>();

        private const int Pierce = 1;
        private const int FlightTime = 0;
        private const float Gravity = -0.05f;

        public override string Texture => Helper.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.alpha = 255;
            projectile.extraUpdates = 1;

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
            ++projectile.ai[0];
            if (projectile.ai[0] > FlightTime)
            {
                projectile.velocity.Y += Gravity;
            }
            if (projectile.ai[0] > 30)
            {
                projectile.Kill();
            }
            for (int i = 0; i < 2; i++)
            {
                Terraria.Dust.NewDustPerfect(projectile.position, DustType, Scale: 1.85f).noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Terraria.Dust.NewDustPerfect(projectile.position, DustType2, Scale: 1.6f).noGravity = true;
            }

            projectile.rotation = projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = 5;
            projectile.damage /= 2;
        }
    }
}
