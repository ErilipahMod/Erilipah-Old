using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
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
        public override string MapName => "Infected Pustule";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style => TileObjectData.Style2x1;

        public override bool KillSound(int i, int j)
        {
            Main.PlaySound(SoundID.NPCHit, i * 16, j * 16, 19, 1, 0.2f);
            return false;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
        {

        }

        public override void DrawEffects(int i, int j, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 1.2f;
        }

        public override void PostDraw(int i, int j, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Microsoft.Xna.Framework.Graphics.Texture2D texture = ModContent.GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Flower_Glowmask");
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }

            Color color = Lighting.GetColor(i, j) * 2;
            Main.spriteBatch.Draw(
                texture,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                new Rectangle(tile.frameX, tile.frameY, 16, 16), color, 0f, Vector2.Zero, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameX == 36 && ErilipahTile.OnScreen(i, j))
            {
                WorldGen.KillTile(i, j);
            }

            if (tile.frameX == 0 && tile.frameY == 0 && Main.netMode != 1)
            {
                tile.frameX = 36;
                if (Main.tile[i + 1, j].type == Type)
                    Main.tile[i + 1, j].frameX = 54;

                Burst(i, j);
            }
        }

        private void Burst(int i, int j)
        {
            for (int a = 0; a < 3; a++)
            {
                Vector2 rand = Main.rand.NextVector2CircularEdge(6, 6);
                Projectile.NewProjectile(
                    i * 16f + 16 + 8, j * 16f + 4,
                    rand.X, rand.Y,
                    mod.ProjectileType<FlowerProj>(), 21, 1);
            }

            for (int h = 0; h < 10; h++)
            {
                float rotation = h / 10f * MathHelper.Pi + MathHelper.Pi;
                Dust.NewDustPerfect(new Vector2(i * 16 + 16, j * 16), mod.DustType<FlowerDust>(), rotation.ToRotationVector2() * 6, Scale: 2).noGravity = true;
            }

            Main.PlaySound(SoundID.PlayerKilled, i * 16, j * 16, 0, 1, 0.625f);
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            int frameX = Main.tile[i, j].frameX;
            if (frameX > 18)
                return true;

            int left = i - frameX / 18;
            Main.tile[left, j].frameX = 36;
            Main.tile[left + 1, j].frameX = 54;

            Burst(i, j);

            return false;
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
            projectile.SetInfecting(1f);
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
            Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(5, 5), mod.DustType<FlowerDust>(), Vector2.Zero, Scale: 1.5f);
            if (projectile.timeLeft < 60)
                projectile.scale -= 1 / 90f;
        }
    }
}
