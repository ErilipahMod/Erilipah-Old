using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class DracocideReaper : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dracocide Reaper");
            Tooltip.SetDefault("Occasionally fires an orb that crackles with energy\n" +
                "The orb breaks off into three homing energies");
        }
        public override void SetDefaults()
        {
            item.damage = 46;
            item.knockBack = 5;
            item.crit = 6;
            item.melee = true;
            item.noMelee = false;

            item.maxStack = 1;
            item.useTime =
            item.useAnimation = 16;
            item.useStyle = 1;
            item.autoReuse = true;
            item.useTurn = true;
            item.UseSound = SoundID.Item1;

            item.width = 36;
            item.height = 36;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<DracocideOrb>();
            item.shootSpeed = 2f;
        }

        private int swingCount;
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return (swingCount++ % 3 == 0);
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

    public class DracocideOrb : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dracocide Reaper Orb");
            Main.projFrames[projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            projectile.width = 56;
            projectile.height = 56;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 180;

            projectile.knockBack = 1f;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.hostile = !
                (projectile.friendly = true);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int offSetX = 12;
            int offSetY = 8;

            hitbox.Offset(offSetX, offSetY);
            hitbox.Width -= offSetX;
            hitbox.Height -= offSetY;
        }
        public override void AI()
        {
            projectile.Animate(5, 7);
            Lighting.AddLight(projectile.Center, 0.22f, 0.18f, 0f);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Main.netMode != 1)
                    Projectile.NewProjectile(projectile.position, Main.rand.NextVector2Unit() * 5f, mod.ProjectileType<DracocideHomingEnergy>(),
                        projectile.damage / 3, 0f, projectile.owner);
                Dust.NewDust(projectile.position, projectile.width, projectile.height, mod.DustType<DracocideDust>());
            }
        }
    }

    public class DracocideHomingEnergy : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dracocide Energy");
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.knockBack = 0f;
            projectile.magic = true;
            projectile.penetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }
        public override void AI()
        {
            int npc = projectile.FindClosestNPC(900, x =>
            {
                bool closest = Collision.CanHit(projectile.position, projectile.width, projectile.height, x.position, x.width, x.height);
                return closest;
            });

            if (npc > -1)
            {
                NPC target = Main.npc[npc];
                projectile.velocity = projectile.GoTo(target.Center, 0.5f);
            }

            if (projectile.ai[1]++ % 3 == 0)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<DracocideDust>(), Main.rand.NextVector2Unit() * 0.05f);
                Lighting.AddLight(projectile.Center, 0.20f, 0.10f, 0f);
                return;
            }
            Lighting.AddLight(projectile.Center, 0.10f, 0.05f, 0f);
        }
    }
}
