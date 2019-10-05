﻿using Erilipah.Biomes.ErilipahBiome.Hazards;
using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.NPCs.ErilipahBiome
{
    class WarmedSoul : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warmed Soul");
        }
        public override void SetDefaults()
        {
            projectile.hide = true;

            projectile.width = 6;
            projectile.height = 6;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 1200;

            projectile.hostile = projectile.friendly = false;
        }

        private Point Torch { get => new Point((int)projectile.ai[0], (int)projectile.ai[1]); set { projectile.ai[0] = value.X; projectile.ai[1] = value.Y; } }

        public override void AI()
        {
            for (int c = 0; c < 2; c++)
                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6, 6), mod.DustType<FlowerDust>(), Vector2.Zero, Scale: 1.15f).noGravity = true;

            if (Torch == Point.Zero || Main.tile[Torch.X, Torch.Y].frameX < 66)
            {
                Torch = SearchForUnlit();
            }

            if (Torch == Point.Zero)
            {
                float 
                    distSpeed  = Vector2.Distance(projectile.Center, Main.player[projectile.owner].Center);
                    distSpeed /= 100f;

                projectile.velocity = projectile.Center.To(Main.player[projectile.owner].Center, distSpeed);
            }
            else
            {
                if (projectile.velocity.Length() < 4)
                    projectile.velocity += projectile.Center.To(Torch.ToWorldCoordinates()) / 30f;
                else
                    projectile.velocity = projectile.Center.To(Torch.ToWorldCoordinates()) * 4;

                if (Vector2.Distance(projectile.Center, Torch.ToWorldCoordinates()) < 5)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        float rot = i / 25f * MathHelper.TwoPi;
                        Dust.NewDustPerfect(projectile.Center, mod.DustType<FlowerDust>(), rot.ToRotationVector2() * 5).noGravity = true;
                    }
                    Main.tile[Torch.X, Torch.Y].frameX -= 66;
                    Main.PlaySound(Terraria.ID.SoundID.Item45, projectile.Center);
                    projectile.Kill();
                }
            }
        }

        /// <returns>Zero if nothing found.</returns>
        private Point SearchForUnlit()
        {
            Point tilePos = projectile.Center.ToTileCoordinates();
            Point closest = Point.Zero;
            float closestDistance = 1000;

            for (int i = tilePos.X - 20; i < tilePos.X + 20; i++)
            {
                for (int j = tilePos.Y - 20; j < tilePos.Y + 20; j++)
                {
                    Tile candidate = Main.tile[i, j];

                    bool noOthers = true;
                    for (int p = 0; p < Main.maxProjectiles; p++)
                    {
                        if (p != projectile.whoAmI && 
                            Main.projectile[p].active && Main.projectile[p].type == projectile.type &&
                            (Main.projectile[p].ai[0], Main.projectile[p].ai[1]) != (0, 0) &&
                            (Main.projectile[p].ai[0], Main.projectile[p].ai[1]) == (Torch.X, Torch.Y))
                        {
                            noOthers = false;
                            break;
                        }
                    }

                    float currentDistance = Vector2.Distance(new Vector2(i, j), tilePos.ToVector2());
                    bool validTile = candidate.active() && candidate.type == mod.TileType<ArkenTorchTile>() && candidate.frameX >= 66;
                    if (noOthers && currentDistance < closestDistance && validTile)
                    {
                        closestDistance = currentDistance;
                        closest = new Point(i, j);
                    }
                }
            }

            return closest;
        }
    }
}
