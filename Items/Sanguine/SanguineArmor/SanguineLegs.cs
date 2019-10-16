using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine.SanguineArmor
{
    [AutoloadEquip(EquipType.Legs)]
    public class SanguineLegs : ModItem
    {

        public override void SetDefaults()
        {
            item.width = 26;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 80);
            item.rare = 3;
            item.defense = 3;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Sanguine Greaves");
            Tooltip.SetDefault("5% increased move speed"
                + "\n5% increased damage"
                + "\n'Weak when separate, powerful when together'");
        }

        public override void UpdateEquip(Player player)
        {
            player.allDamage += 0.05f;

            player.moveSpeed *= 1.05f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(GetInstance<SanguineAlloy>(), 6);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}