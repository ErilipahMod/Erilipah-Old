using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonHamaxe : NewModItem
    {
        protected override int Damage => 23;
        protected override int[] UseSpeedArray => new int[2] { 12, 20 };
        protected override float Knockback => 4;
        protected override int Axe => 130;
        protected override int Hammer => 65;

        protected override bool FiresProjectile => false;
        protected override int[] Dimensions => new int[] { 38, 34 };
        protected override int Rarity => Terraria.ID.ItemRarityID.Orange;
        protected override UseTypes UseType => UseTypes.Swing;

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 6 }, { Terraria.ID.ItemID.HellstoneBar, 5 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;

        public override void OnHitNPC(Terraria.Player player, Terraria.NPC target, int damage, float knockBack, bool crit)
        {
            int chance = 60 / UseSpeedArray[0];
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}
