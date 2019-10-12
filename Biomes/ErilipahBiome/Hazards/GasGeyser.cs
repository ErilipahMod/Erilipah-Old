﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
//using Microsoft.Xna.Framework.Graphics;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class GasGeyser : HazardTile
    {
        public override string MapName => "Gas Geyser";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinatePadding = 2;
                TileObjectData.newTile.CoordinateHeights = new int[] { 18 };
                TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType<Tiles.InfectedClump>() };

                return TileObjectData.newTile;
            }
        }

        public override void DrawEffects(int i, int j, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 2.25f;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
        {
            height = 16;
            offsetY = 2;
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX / 18;

            for (int pIndex = 0; pIndex < Main.maxProjectiles; pIndex++)
            {
                if (Main.projectile[pIndex].active && Main.projectile[pIndex].type == mod.ProjectileType<GasSpew>() && Main.projectile[pIndex].localAI[0] == left)
                {
                    return;
                }
            }

            Projectile p = Main.projectile[Projectile.NewProjectile(left * 16f + 16 + 6, j * 16f + 6, 0, 0, mod.ProjectileType<GasSpew>(), 25, 1)];
            p.ai[1] = Main.rand.Next(3);
            p.localAI[0] = left;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            for (int c = 0; c < Main.maxProjectiles; c++)
            {
                if (Main.projectile[c].type == mod.ProjectileType<GasSpew>())
                {
                    Main.projectile[c].Kill();
                }
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
            if (Main.netMode != 1 && projectile.ai[0] % 3 == 0)
            {
                projectile.netUpdate = true;
                Projectile.NewProjectile(
                    projectile.Center.X, projectile.Center.Y,
                    Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-7f, -5.5f),
                    mod.ProjectileType<GasSpewProj>(), Main.expertMode ? projectile.damage / 2 : projectile.damage, 1, 255);
            }

            if (projectile.ai[0] % 2 == 0)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<FlowerDust>(),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-8f, -7f))).noGravity = true;

                Dust dust = Dust.NewDustPerfect(projectile.Center, mod.DustType<AshDust>());
                dust.noGravity = false;
                dust.velocity = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-11f, -9f));
                dust.customData = 0.05f;
            }

            else
                Dust.NewDustPerfect(projectile.Center, mod.DustType<Items.Crystalline.CrystallineDust>(),
                    new Vector2(Main.rand.NextFloat(-0.32f, 0.32f), Main.rand.NextFloat(-10f, -8f)), Scale: 1.5f).noGravity = false;

            if (projectile.ai[0] % 8 == 0)
            {
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 45, 0.7f, -0.15f);
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
            projectile.timeLeft = 320;

            projectile.hostile = projectile.friendly = true;
            projectile.SetInfecting(1.5f);
        }

        public override void AI()
        {
            projectile.rotation += Math.Sign(Main.windSpeed) * 0.1f;

            projectile.velocity.X += Main.windSpeed * 0.02f;
            projectile.velocity.X = MathHelper.Clamp(projectile.velocity.X, -4, 4);

            projectile.velocity.Y += 0.06f;
            if (projectile.velocity.Y > 0)
                projectile.tileCollide = true;
            if (projectile.velocity.Y > 1.7f)
                projectile.velocity.Y = 1.7f;

            if (projectile.timeLeft < 60)
                projectile.scale -= 1 / 75f;
        }
    }
}
