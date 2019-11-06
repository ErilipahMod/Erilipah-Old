using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.ErilipahBiome
{
    public class DragonPeepee : NewModItem
    {
        protected override UseTypes UseType => UseTypes.SwordSwing;
        protected override int[] Dimensions => new int[] { 44, 44 };
        protected override int Rarity => 5;

        protected override int Damage => 40;
        protected override int UseSpeed => 18;
        protected override int Crit => 18;
        protected override float Knockback => 5.75f;

        protected override string DisplayName => "Wither";
        protected override string Tooltip =>
            "Inflicts a curse that tears crowds apart one-by-one" +
            "\nThe curse strengthens the more it is applied" +
            "\n'Locked away where light fades and reality crumbles... for good reason'";
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            target.AddBuff(BuffType<Wither>(), 360);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.LightsBane);
            r.AddIngredient(ItemID.BlackLens);
            r.AddIngredient(ItemID.HellstoneBar, 3);
            r.AddIngredient(ItemID.Diamond, 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(ItemID.BloodButcherer);
            r.AddIngredient(ItemID.BlackLens);
            r.AddIngredient(ItemID.HellstoneBar, 3);
            r.AddIngredient(ItemID.Diamond, 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
