using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class VolatileDracocide : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Conjures a flask of energetic, explosive dracocide");
        }
        public override void SetDefaults()
        {
            item.damage = 80;
            item.knockBack = 6;
            item.crit = 0;
            item.magic = true;
            item.noMelee = true;
            item.mana = 10;

            item.noUseGraphic = true;
            item.maxStack = 1;
            item.useTime =
            item.useAnimation = 26;
            item.useStyle = 1;
            item.autoReuse = true;
            item.UseSound = SoundID.Item106;

            item.width = 36;
            item.height = 36;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<VolatileDracocideProj>();
            item.shootSpeed = 9;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<Dracocell>(), 8);
            recipe.AddTile(TileID.MythrilAnvil);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class VolatileDracocideProj : ModProjectile
    {
        public override string Texture => "Erilipah/Items/Dracocide/VolatileDracocide";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile Dracocide");
        }
        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;

            projectile.tileCollide = true;
            projectile.aiStyle = 2;

            projectile.magic = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }
        public override void AI()
        {
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, mod.ProjectileType<DracocideExplosion>(), projectile.damage, 6.5f, projectile.owner);
        }
    }

    public class DracocideExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dracocide Explosion");
            Main.projFrames[projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            projectile.width = 64;
            projectile.height = 64;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;

            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.hostile = !
                (projectile.friendly = true);
        }
        public override void AI()
        {
            float intensity = MathHelper.Lerp(1, 0, projectile.frame / 6f);
            Lighting.AddLight(projectile.Center, intensity, intensity * 0.5f, 0);

            if (++projectile.frameCounter % 3 == 0)
                projectile.frame++;

            if (projectile.frameCounter == 1)
            {
                for (int i = 0; i < 9; i++)
                {
                    Vector2 randomVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 4f);
                    Dust.NewDustPerfect(projectile.Center, DustID.Fire, randomVelocity);
                }
            }
            if (projectile.frame == 6)
            {
                projectile.frame = 5;
                projectile.Kill();
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 450);
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 450);
        }
    }
}
