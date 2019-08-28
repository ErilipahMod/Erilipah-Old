using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class LunacritaStaff : ModItem
    {
        public override void SetDefaults()
        {
            item.damage = 18;
            item.summon = true;
            item.mana = 21;
            item.width = 46;
            item.height = 46;

            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = 1;
            item.noMelee = true;
            item.knockBack = 2;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = 2;
            item.UseSound = SoundID.Item44;
            item.shoot = mod.ProjectileType("LunacritaProj");
            item.shootSpeed = 1f;
            item.buffTime = 100;
            item.buffType = mod.BuffType("Lunacrita");
        }

        public override bool AltFunctionUse(Player player) => false;
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType("SynthesizedLunaesia"), 4);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunacrita Sigil");
            Tooltip.SetDefault("Summons a Lunacrita to fight for you");
        }
    }
}