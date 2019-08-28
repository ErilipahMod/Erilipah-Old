using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class PhlogistonBracing : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 32, 22 };
        protected override int Rarity => 3;
        protected override string Tooltip => "9% increased swing speed";

        protected override int Defense => 10;
        public override void UpdateEquip(Player player)
        {
            player.meleeSpeed /= 1.09f;
        }

        protected override int CraftingTile => TileID.Anvils;
        protected override int[,] CraftingIngredients => new int[,] {
            { mod.ItemType<StablePhlogiston>(), 11 },
            { ItemID.HellstoneBar, 7 }
        };
    }
}
