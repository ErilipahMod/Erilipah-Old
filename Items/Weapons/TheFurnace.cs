using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class TheFurnace : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Gun;
        protected override int[] Dimensions => new int[] { 54, 16 };
        protected override int Rarity => 2;

        protected override int Damage => 11;
        protected override int[] UseSpeedArray => new int[] { 6, 30 };
        protected override int Crit => 16;
        protected override float Knockback => 0.1f;
        protected override LegacySoundStyle UseSound => SoundID.Item34;

        protected override float ShootSpeed => 12;
        protected override Vector2? HoldoutOffSet => new Vector2(-4, 2);
        protected override float ShootDistanceOffset => -15;
        protected override int ShootType => ProjectileID.Flames;

        protected override string Tooltip => "Uses gel for ammo";
        public override void SetDefaults()
        {
            base.SetDefaults();
            item.useAmmo = AmmoID.Gel;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            if (Main.rand.NextBool(10))
            {
                Main.PlaySound(SoundID.Item20, position);
                Projectile.NewProjectile(position.X, position.Y, speedX, speedY, ProjectileID.BallofFire, damage * 2, knockBack, player.whoAmI);
            }
            type = ShootType;
            Main.projectile[Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI)]
                .timeLeft = 30;
            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Handgun, 1);
            r.AddIngredient(ItemID.HellstoneBar, 3);
            r.AddIngredient(ItemID.MeteoriteBar, 10);
            r.AddIngredient(ItemID.Fireblossom, 5);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}