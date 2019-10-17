using Terraria;

namespace Erilipah.Items.Templar
{
    public class SigilOfVitality : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 30, 26 };
        protected override int Rarity => 2;

        protected override string Tooltip => "Doubles maximum vitality to a total of 240\n" +
            "Increases vitality gain by one and lowers vitality degradation three-fold\n" +
            "You can heal while Potion Sickness is active using 120 vitality";
        //"If you die at maximum vitality, you will be saved and lose all vitality\n" +
        //"Revival has a three minute cooldown";
        public override void UpdateEquip(Player player)
        {
            Vitality vital = player.GetModPlayer<Vitality>();
            vital.maxVitality += 120;
            vital.degradeDelay += 60;
        }
    }
}
