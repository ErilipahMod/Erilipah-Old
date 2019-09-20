using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome
{
    public class FallingAsh
    {
        private Vector2 pos;
        private float rot;
        private readonly float grav;
        private float scale;
        private readonly float maxScale;
        private int time;

        public bool active = true;

        public FallingAsh(Vector2 position, float gravity, float scale)
        {
            active = true;
            pos = position;
            grav = gravity;
            this.scale = 0f;
            this.maxScale = scale;
        }

        public void Update()
        {
            time++;

            // Tile collision!
            Vector2 velocity = new Vector2(Main.windSpeed * 2, grav);
            if (grav > 2.35f)
                velocity = Collision.TileCollision(pos, velocity, 1, 1);

            pos += velocity;

            if (velocity.Y < 0.15f || MathHelper.Distance(Main.windSpeed, velocity.X) > 0.2f)
                rot += Main.windSpeed / 2f;
            else
                rot += Main.windSpeed;

            if (time <= 300 && scale < maxScale)
            {
                scale += 0.08f;
            }
            else if (time > 300)
            {
                scale -= 0.03f;
                if (scale <= 0)
                    active = false;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture2D = ModContent.GetTexture("Erilipah/NPCs/ErilipahBiome/VoidParticle");
            Rectangle frame = texture2D.Frame(1, 3, 0, 1);
            spriteBatch.Draw(texture2D,
                pos - Main.screenPosition, frame, Color.White * 1f, rot, new Vector2(4.5f, 4.5f), scale, 0, 0);
        }
    }
    public class ErilipahSky : CustomSky
    {
        public override void Activate(Vector2 position, params object[] args) => _active = true;
        public override void Deactivate(params object[] args)
        {
            _active = false;
            ashes.Clear();
        }
        public override bool IsActive() => _active || Opacity > 0;
        public override void Reset() => _active = false;

        private bool _active = false;
        private float Intensity => MathHelper.SmoothStep(0, 0.5f, Opacity);
        private List<FallingAsh> ashes = new List<FallingAsh>();

        public override void Update(GameTime gameTime)
        {
            if (_active && Opacity < 1f)
            {
                Opacity += 0.01f;
            }
            else if (!_active && Opacity > 0f)
            {
                Opacity -= 0.01f;
            }
            Opacity = MathHelper.Clamp(Opacity, 0, 1f);

            bool noAshes = Main.myPlayer < 0 || Main.gameMenu || !Main.LocalPlayer.active || Main.LocalPlayer.Center.Y > Main.worldSurface * 16;
            if (noAshes)
            {
                ashes.Clear();
            }
            else if (ashes.Count < (Main.raining || Sandstorm.Happening ? 1200 : 900))
            {
                float grav = (Main.raining || Sandstorm.Happening ? 1.3f : 1.0f) * Main.rand.NextFloat(1f, 5f);
                ashes.Add(new FallingAsh(
                    new Vector2(
                        Main.rand.NextFloat(Main.screenPosition.X - 100, Main.screenPosition.X + Main.screenWidth + 100),
                        Main.screenPosition.Y - 50),
                    grav,
                    MathHelper.Lerp(0.6f, 1.5f, (grav - 1f) / 5f)
                    ));
            }
            ashes.ForEach(ash => ash.Update());
            ashes.RemoveAll(ash => !ash.active);
            foreach (var droplet in Main.rain)
            {
                droplet.active = false;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                ashes.ForEach(ash => ash.Draw(spriteBatch));
                spriteBatch.Draw(Main.blackTileTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (1f - Intensity));
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Vector4 start = new Vector4(0.4f, 0.1f, 0.4f, 1f);
            Vector4 end = Vector4.Lerp(start, inColor.ToVector4(), 1 - Intensity);
            return new Color(end);
        }
        public override float GetCloudAlpha() => 0;
    }
}
