using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonChakram : NewModItem
    {
        protected override int Damage => 28;
        protected override int[] UseSpeedArray => new int[] { 14, 14 };
        protected override float Knockback => 2;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 8.5f;
        protected override int ShootType => base.ShootType;

        protected override int[] Dimensions => new int[] { 34, 34 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override bool? Consumable => true;

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 1 }, { Terraria.ID.ItemID.HellstoneBar, 1 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
        protected override int CraftingResultAmount => 111;
    }
    public class PhlogistonChakramProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 34, 34 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override int FlightTime => 25;
        protected override float Gravity => 0.15f;
        protected override Item ItemSource => mod.GetItem<PhlogistonChakram>().item;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(3) * RotateDirection;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 20;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}
