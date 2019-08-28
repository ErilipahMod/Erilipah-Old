using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class WoodenEnd : NewModItem
    {
        protected override string Tooltip => "Has a chance to poison enemies";
        protected override UseTypes UseType => UseTypes.SwordSwing;
        protected override int[] Dimensions => new int[] { 56 };
        protected override int Rarity => 2;

        protected override int Damage => 28;
        protected override int UseSpeed => 30;
        protected override float Knockback => 7;
        protected override bool AutoReuse => false;

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (Main.rand.NextBool(3))
                target.AddBuff(BuffID.Poisoned, 180);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.RichMahoganySword);
            r.AddIngredient(ItemID.Stinger, 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
