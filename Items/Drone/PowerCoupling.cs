namespace Erilipah.Items.Drone
{
    public class PowerCoupling : NewModItem
    {
        protected override int Damage => 0;
        protected override int[] UseSpeedArray => new int[] { 5, 5 };
        protected override float Knockback => 0;

        protected override bool FiresProjectile => false;
        protected override int[] Dimensions => new int[] { 18, 32 };
        protected override int Rarity => 1;

        protected override UseTypes UseType => UseTypes.Material;
        protected override string Tooltip => "'A salvaged power coupling from a rogue drone'";
    }
}