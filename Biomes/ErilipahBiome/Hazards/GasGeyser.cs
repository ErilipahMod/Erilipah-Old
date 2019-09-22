using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class GasGeyser : HazardTile
    {
        public override string MapName => "Gas Geyser";
        public override int DustType => DustID.Stone;
        public override TileObjectData Style => TileObjectData.Style2x1;

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

            if (Main.projectile.Any(x => x.active && x.type == mod.ProjectileType<GasSpew>() && x.ai[1] == i))
                return;

            if (Main.netMode != 1)
                Projectile.NewProjectile(i * 16f + 16 + 8, j * 16f + 8, 0, 0, mod.ProjectileType<GasSpew>(), 25, 1, ai1: i);
        }
    }

    public class GasSpew : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Name");
        }
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 90;

            projectile.hostile = projectile.friendly = false;
        }

        public override void AI()
        {
            projectile.velocity = Vector2.Zero;
            projectile.ai[0]++;
            if (Main.netMode != 1 && projectile.ai[0] % 5 == 0)
            {
                projectile.netUpdate = true;
                Projectile.NewProjectile(
                    projectile.Center.X, projectile.Center.Y,
                    Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-6f, -5f),
                    mod.ProjectileType<GasSpewProj>(), Main.expertMode ? projectile.damage / 2 : projectile.damage, 1);
            }

            if (projectile.ai[0] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item45, projectile.Center);
            }
        }
    }

    public class GasSpewProj : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override string Texture => base.Texture.Replace("SpewProj", "");
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spew");
        }
        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.hostile = projectile.friendly = true;
            projectile.SetInfecting(1.5f);
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
