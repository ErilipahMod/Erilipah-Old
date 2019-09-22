using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class ModelMoon : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.maxStack = 999;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = 4;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = 2;
            item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(mod.NPCType("LunarBee")) && !Main.dayTime;
        }

        public override bool UseItem(Player player)
        {
            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("LunarBee"));
            Main.PlaySound(15, (int)player.position.X, (int)player.position.Y, 0);
            return true;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Tooltip.SetDefault("Usable at night\n'Beautiful, even for a space rock'");
            DisplayName.SetDefault("Model Moon");
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.FallenStar, 4);
            r.AddRecipeGroup("Erilipah:AnyGem", 3);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}