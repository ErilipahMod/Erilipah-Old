using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class Moonbow : ModItem
    {
        public override void SetDefaults()
        {
            item.damage = 13;
            item.ranged = true;
            item.width = 32;
            item.height = 38;
            item.useTime = 22;
            item.useAnimation = 22;
            item.useStyle = 5;
            item.noMelee = true;
            item.knockBack = 4;
            item.value = Item.buyPrice(0, 1, 10, 0);
            item.rare = 2;
            item.UseSound = SoundID.Item5;
            item.autoReuse = false;
            item.shoot = 3;
            item.shootSpeed = 9f;
            item.useAmmo = AmmoID.Arrow;
        }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("10% chance to not consume arrows");
        }

        public override bool ConsumeAmmo(Player player)
        {
            return Main.rand.NextFloat() >= .10f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-1, 1);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType("SynthesizedLunaesia"), 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}