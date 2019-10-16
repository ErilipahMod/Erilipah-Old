using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.LunarBee
{
    public class LunaemiaSaber : ModItem
    {
        public override void SetDefaults()
        {
            item.damage = 14;
            item.crit = 14;
            item.melee = true;
            item.width = 50;
            item.height = 50;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = 1;
            item.knockBack = 4;
            item.value = Item.buyPrice(0, 65, 20, 0);
            item.rare = 2;
            item.UseSound = SoundID.Item1;
            item.autoReuse = false;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType("SynthesizedLunaesia"), 6);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (crit)
                target.AddBuff(BuffType<LunarBreakdown>(), 300);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunacritical");
            Tooltip.SetDefault("Inflicts enemies with Lunar Breakdown on critical strikes");
        }
    }
}