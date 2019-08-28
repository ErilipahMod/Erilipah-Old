using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class Gate : ModItem
    {
        private new string DisplayName => "Arcanum " + GetType().Name;

        private new string Tooltip => "'Reveals what is hidden...'";

        private const int Rarity = ItemRarityID.Orange;

        private int[] Dimensions => new int[] { 30, 44 };

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault(DisplayName);
            base.Tooltip.SetDefault(Tooltip);
        }
        public override void SetDefaults()
        {
            //item.accessory = true;
            item.maxStack = 999; //1
            item.rare = Rarity;

            item.width = Dimensions[0];
            item.height = Dimensions[1];
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            return !player.armor.Take(10).Any(x => x.GetType().Namespace.StartsWith("Erilipah.Items.Accessories.Arcanums"));
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddRecipeGroup(Erilipah.Gold, 10);
            r.AddRecipeGroup(Erilipah.Gem);
        }
    }
}
