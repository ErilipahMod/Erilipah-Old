using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine.SanguineArmor
{
    [AutoloadEquip(EquipType.Head)]
    public class SanguineHelm : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.value = Item.sellPrice(0, 1);
            item.rare = 3;
            item.defense = 3;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Sanguine Helm");
            Tooltip.SetDefault("5% increased move speed" +
                "\n4% increased damage" +
                "\n'Weak when separate, powerful when together'");
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
            => body.type == mod.ItemType("SanguineTrunk") && legs.type == mod.ItemType("SanguineLegs");

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Sanguine/normal items heal you 5/10% of damage dealt, respectively" +
                "\n5% increased damage taken and potion cooldowns are 1.5 times longer";
            Lighting.AddLight(player.Center, 0.3f, 0.1f, 0);
            player.GetModPlayer<SanguineSet>().sanguineSet = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.allDamage += 0.04f;
            player.moveSpeed *= 1.05f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(GetInstance<SanguineAlloy>(), 7);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}