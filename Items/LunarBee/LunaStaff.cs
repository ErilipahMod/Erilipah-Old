using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.LunarBee
{
    public class LunaStaff : ModItem
    {
        private int Damage => 19; //39

        private float Knockback => 1.5f;

        private int UseSpeed => 26;

        private float ShootSpeed => 6.75f;

        private int[] Dimensions = { 46, 46 };
        private new string DisplayName = "Luna Staff";
        private new string Tooltip = "";

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.maxStack = 1;

            // most changed
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;
            item.mana = 9;

            // occasionally changed booleans
            item.noMelee = true;
            item.magic = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = ItemRarityID.Green;
            item.UseSound = Terraria.ID.SoundID.Item8;
            item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType("SynthesizedLunaesia"), 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }

        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);

            Terraria.Item.staff[item.type] = true;
        }
    }
    public class LunaStaffProj : ModProjectile
    {
        private int Pierce => 3;

        private int Dust => DustType<MoonFire>();

        public override string Texture => "Terraria/Projectile_" + ProjectileID.ShadowBeamFriendly;
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.timeLeft = 30;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault(name.Remove(name.Length - 4));
        }

        public override void AI()
        {
            Terraria.Dust.NewDustPerfect(projectile.position, Dust).noGravity = true;
            projectile.rotation += Helper.RadiansPerTick(3.75f);
        }
        public override void Kill(int timeLeft)
        {
            if (projectile.ai[0] == 0 && Main.netMode != 1)
            {
                const float degrees = 15;
                Vector2[] vectors = {
                    projectile.velocity.RotatedBy(MathHelper.ToRadians(degrees)),
                    projectile.velocity.RotatedBy(MathHelper.ToRadians(360 - degrees))
                };

                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(projectile.position, vectors[i], projectile.type,
                        projectile.damage, projectile.knockBack, projectile.owner, 1);
                }
            }
        }
    }
}
