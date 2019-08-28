using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class LunarFlask : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 38;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useAnimation =
                item.useTime = 17;

            item.rare = ItemRarityID.Expert;
            item.UseSound = SoundID.Item3;
            item.value = Item.sellPrice(0, 0, 80, 0);

            item.healLife = 80;
            item.expert = true;
            item.potion = true;
        }
        public override void SetStaticDefaults()
            => Tooltip.SetDefault("Restores " + amount + " mana" +
                "\nNot consumable");

        private const int amount = 100;
        public override bool UseItem(Player player)
        {
            player.ManaEffect(amount);

            if (player.statMana >= player.statManaMax2)
            {
                return true;
            }
            player.statMana += Math.Min(amount, player.statManaMax2 - player.statMana);
            return true;
        }
        public override bool ConsumeItem(Player player)
        {
            return false;
        }
    }
}