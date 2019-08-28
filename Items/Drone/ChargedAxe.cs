using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Erilipah.Items.Drone
{
    public class ChargedAxe : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 34, 34 };
        protected override string Tooltip => "Sticks to tiles";

        protected override int Damage => 21;
        protected override int[] UseSpeedArray => new int[] { 20, 20 };
        protected override float Knockback => 2;
        protected override bool? Consumable => true;

        protected override bool AutoReuse => false;
        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9;
        protected override int Rarity => 2;

        protected override int[,] CraftingIngredients => new int[,] { { mod.ItemType<PowerCoupling>(), 1 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
        protected override int CraftingResultAmount => 75;
    }
    public class ChargedAxeProj : NewModProjectile
    {
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = height = 10;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => 5;

        protected override int Pierce => 3;
        protected override int Bounce => 0;
        protected override float Gravity
        {
            get
            {
                if (IsSticking)
                    return 0;
                return 0.135f;
            }
        }
        protected override int FlightTime => 10;

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.rotation + (IsSticking ? 0 : Helper.RadiansPerTick(2.5f) * RotateDirection);

        private bool IsSticking
        {
            get
            {
                return projectile.ai[0] == 1;
            }

            set
            {
                projectile.ai[0] = value ? 1 : 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            IsSticking = true;
            projectile.velocity = Vector2.Zero;
            return false;
        }
        public override void AI()
        {
            base.AI();
            if (IsSticking)
                projectile.velocity = Vector2.Zero;
        }
    }
}