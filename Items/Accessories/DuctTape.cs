using Terraria;

namespace Erilipah.Items.Accessories
{
    public class DuctTape : NewModItem
    {
        protected override UseTypes UseType => UseTypes.None;
        protected override string Tooltip => "Applies item buff Duct Tape" +
            "\nRight click in inventory; affects held item" +
            "\n'Silence is golden...'";
        protected override int? Value => Item.buyPrice(0, 2);
        protected override int[] Dimensions => new int[] { 26, 26 };
        protected override int? MaxStack => 999;
        protected override int Rarity => 1;

        public override bool CanRightClick() => true;

        private bool consume = false;
        public override void RightClick(Player player)
        {
            Item i = player.HeldItem;
            if (i.damage > 0 && i.useTime > 0)
            {
                consume = true;
                mod.GetGlobalItem<ItemBuff>().NewBuff(ItemBuffID.DuctTape, 3600 * 8);
            }
            else
                consume = false;
        }
        public override bool ConsumeItem(Player player)
        {
            return consume;
        }
    }
}