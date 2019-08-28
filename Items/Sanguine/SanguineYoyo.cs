using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguineYoyo : ModItem
    {
        public override void SetStaticDefaults()
        {
            // These are all related to gamepad controls and don't seem to affect anything else
            ItemID.Sets.Yoyo[item.type] = true;
            ItemID.Sets.GamepadExtraRange[item.type] = 10;
            ItemID.Sets.GamepadSmartQuickReach[item.type] = true;
            Tooltip.SetDefault("Heals you for 15% of damage dealt");
        }
        public override void SetDefaults()
        {
            item.width = 36;
            item.height = 30;

            item.useStyle = 5;
            item.noUseGraphic = true;
            item.UseSound = SoundID.Item1;
            item.melee = true;
            item.channel = true;
            item.noMelee = true;

            item.shoot = mod.ProjectileType<SanguineYoyoProj>();
            item.useAnimation = 26;
            item.useTime = 26;
            item.shootSpeed = 10;

            item.knockBack = 3;
            item.damage = 21;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SanguineAlloy"), 6);
            recipe.AddIngredient(ItemID.WhiteString);
            recipe.AddIngredient(ItemID.WoodYoyo);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class SanguineYoyoProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // The following sets are only applicable to yoyo that use aiStyle 99.
            // YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
            // Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
            ProjectileID.Sets.YoyosLifeTimeMultiplier[projectile.type] = 7f;
            // YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
            // Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f
            ProjectileID.Sets.YoyosMaximumRange[projectile.type] = 272;
            // YoyosTopSpeed is top speed of the yoyo projectile. 
            // Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f
            ProjectileID.Sets.YoyosTopSpeed[projectile.type] = 13.5f;
        }

        public override void SetDefaults()
        {
            projectile.extraUpdates = 0;
            projectile.width = 22;
            projectile.height = 22;
            projectile.aiStyle = 99;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.melee = true;
            projectile.scale = 0.9f;
        }

        public override void AI() => Lighting.AddLight(projectile.Center, 0.2f, 0, 0);

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal((int)(damage * 0.15f));
            }
        }
    }
}
