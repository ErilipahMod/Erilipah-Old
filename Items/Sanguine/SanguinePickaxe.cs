using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguinePickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Heals you for 10% of damage dealt");
            DisplayName.SetDefault("Sanguine Pickaxe");
        }

        public override void SetDefaults()
        {
            item.width =
                item.height = 42;

            item.damage = 8;
            item.knockBack = 1;
            item.melee = true;
            item.knockBack = 0.8f;
            item.useTime = 17;
            item.useAnimation = 17;

            item.useStyle = 1;
            item.UseSound = SoundID.Item1;
            item.rare = 3;
            item.autoReuse = true;

            item.pick = 95;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                player.Heal(damage / 10);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox) =>
            Lighting.AddLight(hitbox.Center.ToVector2(), 0.2f, 0, 0);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SanguineAlloy"), 9);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}