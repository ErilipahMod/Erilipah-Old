using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories
{
    public class ChaosAmulet : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 36, 30 };
        protected override int Rarity => ItemRarityID.LightPurple;
        protected override string Tooltip => "Duration of Chaos State reduced to two seconds";

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType("SanguineAlloy"), 3);
            r.AddIngredient(ItemID.SoulofNight, 6);
            r.AddIngredient(ItemID.AdamantiteBar, 3);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType("SanguineAlloy"), 3);
            r.AddIngredient(ItemID.SoulofNight, 6);
            r.AddIngredient(ItemID.TitaniumBar, 3);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();
        }

        protected override int Damage => 0;
        protected override int[] UseSpeedArray => new int[0];
        protected override float Knockback => 0;
        protected override bool FiresProjectile => false;
        protected override int? Value => Item.sellPrice(0, 2);

        public override void UpdateEquip(Player player)
        {
            int i = player.FindBuffIndex(BuffID.ChaosState);
            if (i != -1 && player.buffTime[i] > 120)
            {
                player.buffTime[i] = 120;
            }
        }
    }
}
