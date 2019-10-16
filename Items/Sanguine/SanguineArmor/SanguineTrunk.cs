using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine.SanguineArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class SanguineTrunk : ModItem
    {

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 26;
            item.value = Item.sellPrice(0, 1, 50);
            item.rare = 3;
            item.defense = 4;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Sanguine Trunk");
            Tooltip.SetDefault("6% increased damage"
                + "\n'Weak when separate, powerful when together'");
        }

        public override void UpdateEquip(Player player)
        {
            player.allDamage += 0.06f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SanguineAlloy>(), 9);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}