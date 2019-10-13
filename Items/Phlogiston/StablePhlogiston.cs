using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Phlogiston
{
    public class StablePhlogiston : NewModItem
    {
        protected override int Damage => 0;
        protected override int[] UseSpeedArray => new int[] { 0, 0 };
        protected override float Knockback => 0;

        protected override bool FiresProjectile => false;
        protected override int[] Dimensions => new int[] { 32, 30 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Material;
        protected override string Tooltip => "Items crafted with Phlogiston have a small chance to ignite enemies for a long time";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ItemID.Sets.ItemIconPulse[item.type] = true;
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }
        public override void GrabRange(Player player, ref int grabRange) => grabRange *= 2;
    }
}
