using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Dracocide
{
    public class Detonader : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Arms a short time after being thrown\n" +
                "After arming, it explodes into many small proximity mines");
        }
        public override void SetDefaults()
        {
            item.damage = 19;
            item.knockBack = 0.1f;
            item.crit = 6;
            item.ranged = true;
            item.noMelee = true;
            item.consumable = true;
            item.noUseGraphic = true;

            item.maxStack = 999;
            item.useTime =
            item.useAnimation = 35;
            item.useStyle = 5;
            item.autoReuse = true;
            item.useTurn = true;
            item.UseSound = SoundID.Item1;

            item.width = 16;
            item.height = 22;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = ProjectileType<DetonaderNade>();
            item.shootSpeed = 7;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemType<Dracocell>(), 2);

            recipe.AddTile(TileID.MythrilAnvil);

            recipe.SetResult(this, 25);
            recipe.AddRecipe();
        }
    }

    public class DetonaderNade : ModProjectile
    {
        public override string Texture => "Erilipah/Items/Dracocide/Detonader";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Detonader");
        }
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.timeLeft = 450;

            projectile.ranged = true;
            projectile.tileCollide = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            if (Main.netMode != 1)
                for (int i = 0; i < 15; i++)
                {
                    if (i < 8) // for 8
                    {
                        Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<DracocideDust>(), 3, 3);
                        Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Fire, 3, 3);
                    }

                    Projectile.NewProjectile(
                        projectile.Center,
                        new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6, -1.5f)),
                        ProjectileType<DetonaderProj>(),
                        19,
                        0.2f,
                        projectile.owner);
                }
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.18f;
            projectile.rotation += projectile.velocity.X / 22f;
        }
    }

    public class DetonaderProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Detonader");
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 500;

            projectile.ranged = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void AI()
        {
            if (projectile.velocity.Length() < 0.1f)
            {
                if (++projectile.frameCounter % 5 == 0)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center,
                        DustType<DracocideDust>());

                    dust.noLight = true;
                    dust.velocity = Vector2.Zero;
                    dust.customData = 0;
                    dust.fadeIn = 15;
                }
            }
            else
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center,
                    DustType<DracocideDust>());

                dust.noLight = true;
                dust.velocity = Vector2.Zero;
                dust.customData = 0;
                dust.fadeIn = 15;
            }

            int targetIndex = projectile.FindClosestNPC(400, x =>
            {
                bool closest = Collision.CanHit(projectile.position, projectile.width, projectile.height, x.position, x.width, x.height);
                return closest;
            });
            if (targetIndex == -1)
            {
                // Reduces speed so it doesn't overshoot
                projectile.velocity *= 0.96f;
                return;
            }

            NPC target = Main.npc[targetIndex];
            projectile.velocity = projectile.GoTo(target.Center, 0.28f);
        }

        public override void Kill(int timeLeft)
        {
            // Dusty dust
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.Center, 0, 0, 6);
            }
        }
    }
}
