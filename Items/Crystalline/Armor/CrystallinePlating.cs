using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class CrystallinePlating : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 36, 22 };
        protected override int Rarity => 2;
        protected override int Defense => 6;

        protected override string Tooltip => "5% increased acceleration" +
            "\n6% increased melee speed" +
            "\n40 increased maximum mana";

        protected override int[,] CraftingIngredients => new int[,] {
            {
                mod.ItemType<InfectionModule>(), 10
            }
        };
        protected override int CraftingTile => mod.TileType<ShadaineCompressorTile>();
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.05f;
            player.meleeSpeed /= 1.06f;
            player.statManaMax2 += 40;
        }
    }
}