using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    class FlowerDust : ModDust
    {
        public override bool MidUpdate(Dust dust)
        {
            dust.velocity *= 0.8f;
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 10, lightColor.G + 20, lightColor.B + 50);
        }
    }
    class Flower : HazardTile
    {
        public override string MapName => "Cursed Flower";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style => TileObjectData.Style2x2;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Helper.Invisible;
            return true;
        }
        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameX > 0)
                return;

            if (Main.projectile.Any(x => x.active && x.type == mod.ProjectileType<FlowerProj>() && x.ai[1] == i))
                return;

            if (Main.netMode != 1)
            {
                Vector2 rand = Main.rand.NextVector2CircularEdge(6, 6);
                Projectile.NewProjectile(
                    i * 16f + 16 + 8, j * 16f + 4,
                    rand.X, rand.Y,
                    mod.ProjectileType<GasSpew>(), 25, 1, ai1: i);
            }
        }
    }

    public class FlowerProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spew");
        }
        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.hostile = projectile.friendly = true;
            projectile.SetInfecting(2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Vector2 wouldBeVel = Collision.AnyCollision(projectile.position, projectile.velocity, projectile.width, projectile.height);
            if (wouldBeVel.X == 0)
                projectile.velocity.X *= -1;
            if (wouldBeVel.Y == 0)
                projectile.velocity.Y *= -1;
            return false;
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.06f;
            if (projectile.velocity.Y > 0)
                projectile.tileCollide = true;
            if (projectile.velocity.Y > 1)
                projectile.velocity.Y = 1;

            if (projectile.timeLeft < 60)
                projectile.scale -= 1 / 90f;
        }
    }
}
