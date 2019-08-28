using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonPickaxe : NewModItem
    {
        protected override int Damage => 15;
        protected override int[] UseSpeedArray => new int[2] { 14, 29 };
        protected override float Knockback => 5;
        protected override int Pick => 115;

        protected override bool FiresProjectile => false;
        protected override int[] Dimensions => new int[] { 34, 34 };
        protected override int Rarity => Terraria.ID.ItemRarityID.Orange;
        protected override UseTypes UseType => UseTypes.Swing;

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 6 }, { Terraria.ID.ItemID.HellstoneBar, 4 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            int chance = 60 / UseSpeedArray[0];
            if (Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}