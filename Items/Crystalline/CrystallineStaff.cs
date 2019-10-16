using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineStaff : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 32;

            item.damage = 17;
            item.knockBack = 0.5f;
            item.magic = true;

            item.useTime = 18;
            item.useAnimation = 18;
            item.mana = 6;
            item.noMelee = true;
            item.useStyle = 5;
            item.UseSound = SoundID.Item8;
            item.autoReuse = true;

            item.rare = 2;
            item.shoot = ProjectileType<CrystallineStaffProj>();
            item.shootSpeed = 7.5f;
        }
        public override void SetStaticDefaults() => Item.staff[item.type] = true;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "InfectionModule", 5);
            r.AddTile(mod, "ShadaineCompressorTile");
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class CrystallineStaffProj : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.ShadowBeamFriendly;
        public override void SetDefaults()
        {
            projectile.width = projectile.height = 4;
            projectile.penetrate = 3;
            projectile.magic = true;
            projectile.timeLeft = 80;
            projectile.friendly = true;
        }
        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystalline Staff");

        public override void AI()
        {
            if (Main.rand.NextBool(2))
                Dust.NewDustPerfect(projectile.position, mod.DustType("CrystallineDust"), Scale: 1.1f);
        }
    }
}
