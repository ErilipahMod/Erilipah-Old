using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineKunai : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 22;

            item.damage = 9;
            item.thrown = true;
            item.knockBack = 0.3f;

            item.useTime = 11;
            item.useAnimation = 11;
            item.useStyle = 1;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.consumable = true;
            item.autoReuse = true;
            item.maxStack = 999;

            item.rare = 2;
            item.shootSpeed = 9;
            item.shoot = ProjectileType<CrystallineKunaiProj>();
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "InfectionModule", 1);
            r.AddTile(mod, "ShadaineCompressorTile");
            r.SetResult(this, 200);
            r.AddRecipe();
        }
    }

    public class CrystallineKunaiProj : ModProjectile
    {
        public override string Texture => mod.Name + "/Items/Crystalline/CrystallineKunai";
        public override void SetDefaults()
        {
            projectile.width =
                projectile.height = 24;
            projectile.friendly = true;
            projectile.thrown = true;

            projectile.penetrate = 2;
        }
        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystalline Kunai");

        public override void AI()
        {
            if (++projectile.ai[0] > 30)
                projectile.velocity.Y += 0.145f;
            if (projectile.ai[0] > 140)
                projectile.Kill();

            projectile.rotation += Helper.RadiansPerTick(-3);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType("CrystallineDust"));
            }
            if (projectile.thrown)
            {
                Loot.DropItem(projectile.Hitbox, mod.ItemType(GetType().Name.Remove(GetType().Name.Length - 4)), 1, 1, 25);
            }
        }
    }
}
