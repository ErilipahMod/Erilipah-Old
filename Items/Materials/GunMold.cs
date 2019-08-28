using Terraria;

namespace Erilipah.Items.Materials
{
    public class GunMold : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Material;
        protected override int[] Dimensions => new int[] { 50, 48 };
        protected override int Rarity => 1;

        protected override int? Value => Item.buyPrice(0, 3, 50);
        protected override string Tooltip => "Used to make various guns";
    }
}
