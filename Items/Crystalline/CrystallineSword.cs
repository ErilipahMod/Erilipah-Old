using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineSword : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 26;
            item.height = 34;

            item.damage = 12;
            item.knockBack = 2.5f;
            item.melee = true;

            item.useTime = 12;
            item.useAnimation = 12;
            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;

            item.rare = 2;
            item.shoot = ProjectileType<CrystallineSwordProj>();
            item.shootSpeed = 5.5f;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "InfectionModule", 6);
            r.AddTile(mod, "ShadaineCompressorTile");
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class CrystallineSwordProj : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.ShadowBeamFriendly;
        public override void SetDefaults()
        {
            projectile.width = projectile.height = 4;
            projectile.penetrate = 1;
            projectile.melee = true;
            projectile.friendly = true;
            projectile.timeLeft = 68;
        }
        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystalline Sword Bolt");

        public override void AI()
        {
            if (Main.rand.NextBool(2))
                Dust.NewDustPerfect(projectile.position, mod.DustType("CrystallineDust"), Scale: 1.1f);
        }
    }
}
