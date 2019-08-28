using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class SeedChaingun : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Gun;
        protected override string Tooltip => "50% chance to not consume ammo\nAllows the collection of seeds for ammo";
        protected override int[] Dimensions => new int[] { 112, 46 };
        protected override bool AutoReuse => true;
        protected override int Rarity => 1;

        protected override int Damage => 12;
        protected override float Knockback => 0.5f;
        protected override int UseSpeed => 9;

        protected override float ShootSpeed => 11;
        protected override float ShootInaccuracy => 15;
        protected override Vector2? HoldoutOffSet => new Vector2(-28, 2);
        protected override float ShootDistanceOffset => 76;

        public override bool ConsumeAmmo(Player player)
        {
            return Main.rand.NextFloat() > 0.5f;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            degreeSpread = 10;
            item.useAmmo = ItemID.Seed;
            item.shoot = ProjectileID.Seed;
            item.UseSound = SoundID.Item63;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Daybloom, 10);
            r.AddIngredient(ItemID.DirtBlock, 50);
            r.AddIngredient(mod.ItemType("SoilComposite"), 8);
            r.AddIngredient(ItemID.IllegalGunParts);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }

    public class SeedTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
            if ((type == TileID.Plants || type == TileID.JunglePlants || type == TileID.JunglePlants2 || type == TileID.Plants2) &&
                !noItem && PlayerHasSeedChaingun(i, j))
            {
                Loot.DropItem(new Rectangle(i * 16, j * 16, 16, 16), ItemID.Seed, 1, 2, 90);
            }
        }

        private bool PlayerHasSeedChaingun(int i, int j)
        {
            for (int n = 0; n < Main.player.Length; n++)
            {
                if (Main.player[n].HasItem(mod.ItemType("SeedChaingun")) &&
                    Main.player[n].Distance(new Vector2(i * 16, j * 16)) < 300)
                    return true;
            }
            return false;
        }
    }
}
