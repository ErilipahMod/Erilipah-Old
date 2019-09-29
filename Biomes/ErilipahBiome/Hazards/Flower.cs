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
            return new Color(200, 150, 180);
        }
    }

    class Flower : HazardTile
    {
        public override string MapName => "Cursed Flower";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style => TileObjectData.Style2x1;

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameX == 0 && tile.frameY == 0 && Main.netMode != 1)
            {
                tile.frameY = 18;
                if (Main.tile[i + 1, j].type == Type)
                    Main.tile[i + 1, j].frameY = 18;
                for (int a = 0; a < 3; a++)
                {
                    Vector2 rand = Main.rand.NextVector2CircularEdge(6, 6);
                    Projectile.NewProjectile(
                        i * 16f + 16 + 8, j * 16f + 4,
                        rand.X, rand.Y,
                        mod.ProjectileType<FlowerProj>(), 25, 1);
                }
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
            projectile.timeLeft = 1000;

            projectile.hostile = projectile.friendly = true;
            projectile.SetInfecting(2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
            if (projectile.velocity.X != oldVelocity.X)
                projectile.velocity.X = -oldVelocity.X;
            if (projectile.velocity.Y != oldVelocity.Y)
                projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(5, 5), mod.DustType<FlowerDust>());
            if (projectile.timeLeft < 60)
                projectile.scale -= 1 / 90f;
        }
    }
}
