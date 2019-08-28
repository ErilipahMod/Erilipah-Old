using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class GraniteAssaultRifle : NewModItem
    {
        protected override string Tooltip => "Fires in volleys of five rounds\nOnly consumes two rounds of ammo per volley";
        protected override UseTypes UseType => UseTypes.Gun;
        protected override int[] Dimensions => new int[] { 76, 32 };
        protected override bool AutoReuse => true;
        protected override int Rarity => 2;

        protected override int Damage => 10;
        protected override float Knockback => 4;
        protected override int[] UseSpeedArray => new int[] { 6, 29 };

        protected override float ShootSpeed => 8;
        protected override float ShootInaccuracy => 3;
        protected override Vector2? HoldoutOffSet => new Vector2(-12, 6);
        protected override float ShootDistanceOffset => 35;

        public override void SetDefaults()
        {
            base.SetDefaults();
            degreeSpread = 3;
            item.reuseDelay = 20;
        }

        public override bool ConsumeAmmo(Player player)
        {
            return player.itemAnimation == 31 || player.itemAnimation == 25;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Main.PlaySound(UseSound, position);
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.GraniteBlock, 25);
            r.AddRecipeGroup("Erilipah:EvilBars", 2);
            r.AddIngredient(ItemID.IllegalGunParts);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}