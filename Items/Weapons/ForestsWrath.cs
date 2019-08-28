using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Weapons
{
    public class ForestsWrath : NewModItem
    {
        protected override string Tooltip => "Rapidly fires throwing knives" +
            "\nHas a chance to throw a poisoned knife";

        protected override UseTypes UseType => UseTypes.Book;
        protected override int[] Dimensions => new int[] { 28, 30 };
        protected override int Rarity => 1;

        protected override int Damage => 5;
        protected override int Mana => 7;
        protected override float Knockback => 1.5f;
        protected override int UseSpeed => 10;

        protected override Vector2? HoldoutOffSet => new Vector2(-4, 0);
        protected override float ShootSpeed => 10f;
        protected override float ShootInaccuracy => 13;
        protected override int ShootType => ProjectileID.ThrowingKnife;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            degreeSpread = ShootInaccuracy;
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            if (Main.rand.NextBool(10))
            {
                type = ProjectileID.PoisonedKnife;
            }
            Projectile projectile = Main.projectile[Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI)];
            projectile.noDropItem = true;
            projectile.extraUpdates = 1;
            return false;
        }
    }
}
