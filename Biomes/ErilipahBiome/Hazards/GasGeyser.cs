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
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile = TileObjectData.Style3x2;
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinateHeights = new int[] { 18 };

                return TileObjectData.newTile;
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameX != 0)
                return;

            if (Main.projectile.Any(x => x.active && x.type == mod.ProjectileType<GasSpew>() && x.localAI[0] == i))
                return;

            if (Main.netMode != 1)
            {
                Projectile p = Main.projectile[Projectile.NewProjectile(i * 16f + 16 + 6, j * 16f + 6, 0, 0, mod.ProjectileType<GasSpew>(), 25, 1)];
                p.ai[1] = Main.rand.Next(3);
                p.localAI[0] = i;
            }
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
                    Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-8f, -7f),
                    mod.ProjectileType<GasSpewProj>(), Main.expertMode ? projectile.damage / 2 : projectile.damage, 1, 255);
            }

            if (projectile.ai[0] % 2 == 0)
            {
                if (projectile.ai[0] % 4 == 0)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, mod.DustType<AshDust>());
                    dust.noGravity = false;
                    dust.velocity = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-11f, -9f));
                    dust.customData = 0.05f;
                }

                else
                    Dust.NewDustPerfect(projectile.Center, mod.DustType<Items.Crystalline.CrystallineDust>(),
                        new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-10f, -8f)), Scale: 1.5f).noGravity = false;
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
            projectile.timeLeft = 280;

            projectile.hostile = projectile.friendly = true;
            projectile.SetInfecting(1.5f);
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.06f;
            if (projectile.velocity.Y > 0)
                projectile.tileCollide = true;
            if (projectile.velocity.Y > 1.7f)
                projectile.velocity.Y = 1.7f;

            if (projectile.timeLeft < 60)
                projectile.scale -= 1 / 90f;
        }
    }
}
