using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine
{
    public class SanguineBlade : ModItem
    {
        public override void SetDefaults()
        {
            item.width =
                item.height = 48;

            item.damage = 27;
            item.melee = true;
            item.knockBack = 2;

            item.useTime = 17;
            item.useAnimation = 17;
            item.useStyle = 1;
            item.value = Item.sellPrice(0, 0, 8, 0);
            item.rare = 3;
            item.UseSound = SoundID.Item1;

            item.autoReuse = true;
            item.shoot = ProjectileType<SanguineBladeBolt>();
            item.shootSpeed = 7;
        }

        private int shoot;
        public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (shoot > 50)
            {
                shoot = 0;
                return true;
            }
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem.type != item.type)
            {
                ++shoot;
            }
        }
        public override void HoldItem(Player player)
        {
            shoot++;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Saber");
            Tooltip.SetDefault("Projectile heals you for 33% of damage dealt");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType("SanguineAlloy"), 6);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class SanguineBladeBolt : ModProjectile
    {
        public override string Texture => mod.Name + "/Items/Sanguine/SanguineStaffProj";
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<SanguineDust>());
            }
        }
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 12;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 120;
        }

        public override void AI()
        {
            if (++projectile.ai[0] % 2 == 0)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<SanguineDust>());

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(180);
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal(damage / 3);
            }
        }
    }
}