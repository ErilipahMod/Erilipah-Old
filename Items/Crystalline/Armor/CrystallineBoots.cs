using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class CrystallineBoots : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 18, 12 };
        protected override int Rarity => 2;
        protected override int Defense => 5;

        protected override string Tooltip => "15% increased maximum run speed";
        protected override int[,] CraftingIngredients => new int[,] {
            {
                ItemType<InfectionModule>(), 7
            }
        };
        protected override int CraftingTile => TileType<ShadaineCompressorTile>();
        public override void UpdateEquip(Player player)
        {
            player.maxRunSpeed += 0.15f;
        }
    }
}