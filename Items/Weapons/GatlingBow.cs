using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class GatlingBow : NewModItem
    {
        protected override string Tooltip => "Fires faster the longer it is used\n" +
            "Uses both arrows and bullets as ammunition";
        protected override UseTypes UseType => UseTypes.Bow;
        protected override int[] Dimensions => new int[] { 40, 62 };
        protected override int Rarity => 2;

        protected override int Damage => 11;
        protected override int UseSpeed => 38;
        protected override float Knockback => 2;
        protected override float ShootSpeed => 6.5f;
        protected override int ShootType => ProjectileID.WoodenArrowFriendly;
        protected override Vector2? HoldoutOffSet => new Vector2(-8, 0);
        protected override LegacySoundStyle UseSound => null;

        private int Ammo => Main.rand.NextBool(2) ? AmmoID.Arrow : AmmoID.Bullet;
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int a = Ammo;

            item.useAmmo = a;
            item.UseSound = Ammo == AmmoID.Arrow ? SoundID.Item5 : SoundID.Item11;
            speedX *= MathHelper.Lerp(1, 2, speed / 6f);
            speedY *= MathHelper.Lerp(1, 2, speed / 6f);

            return true;
        }

        private float speed = 1;
        public override float UseTimeMultiplier(Player player)
        {
            if (Main.mouseLeft)
                speed = MathHelper.SmoothStep(speed, 6, 0.011f);
            else
                speed = 1;

            return speed;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.IllegalGunParts, 2);
            r.AddIngredient(ItemID.PalladiumRepeater);
            r.AddIngredient(ItemID.PalladiumBar, 9);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(ItemID.IllegalGunParts, 2);
            r.AddIngredient(ItemID.CobaltRepeater);
            r.AddIngredient(ItemID.CobaltBar, 9);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
