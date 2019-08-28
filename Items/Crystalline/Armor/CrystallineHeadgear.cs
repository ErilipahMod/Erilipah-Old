using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class CrystallineHeadgear : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 26, 24 };
        protected override int Rarity => 2;
        protected override int Defense => 6;

        protected override string Tooltip => "5% increased acceleration and maximum run speed" +
            "\n20% chance to not consume ammo";

        public override bool IsArmorSet(Item head, Item body, Item legs) =>
            body.type == mod.ItemType<CrystallinePlating>() && legs.type == mod.ItemType<CrystallineBoots>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Increases jump speed and you gain an ability to dash";
            player.jumpSpeedBoost += 2;
            player.dash = 1;
        }

        protected override int[,] CraftingIngredients => new int[,] {
            {
                mod.ItemType<InfectionModule>(), 8
            }
        };
        protected override int CraftingTile => mod.TileType<ShadaineCompressorTile>();
        public override void UpdateEquip(Player player)
        {
            player.maxRunSpeed += 0.05f;
            player.moveSpeed += 0.05f;
            player.ammoCost80 = true;
        }
    }
}