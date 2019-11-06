using Erilipah.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;

namespace Erilipah.Items.Drone
{
    public class EnergyDagger : NewModItem
    {
        protected override int Damage => 10;
        protected override int[] UseSpeedArray => new int[] { 15, 15 };
        protected override float Knockback => 3;

        protected override int[,] CraftingIngredients => new int[,] { { mod.ItemType("PowerCoupling"), 9 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
        protected override string Tooltip => "Throws three daggers in an even spread";

        protected override bool AutoReuse => false;
        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9;

        protected override int[] Dimensions => new int[] { 20, 20 };
        protected override int Rarity => 2;
        protected override UseTypes UseType => UseTypes.Thrown;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);

            float numberProjectiles = 3;
            float rotation = MathHelper.ToRadians(10);
            position += Vector2.Normalize(new Vector2(speedX, speedY)) * 25f;
            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                Projectile.NewProjectile(position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, type, damage, knockBack, player.whoAmI);
            }
            return false;
        }
    }
    public class EnergyDaggerProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => 5;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0.3f;
        protected override int FlightTime => 20;

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.None;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(2) * RotateDirection;
    }
}
