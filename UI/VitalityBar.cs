using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Erilipah.UI
{
    public class VitalityBar : UIState
    {
        public bool Visible => alpha > 0;
        public float alpha;
        public int charge;
        public int vitality;
        public int maxVital;

        private const int frameCount = 24;
        private int Frame => Math.Max(0, vitality / (maxVital / frameCount));
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            if (charge > 4) charge = 4;
            alpha = MathHelper.Clamp(alpha, 0f, 1f);
            Texture2D texture = ModContent.GetTexture("Erilipah/UI/VitalityBar" + charge);

            Color color = new Color(alpha, alpha, alpha, alpha);
            Rectangle rect = texture.Frame(1, frameCount, 0, Frame);
            Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f - 42);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / frameCount * 0.5f);

            spriteBatch.Draw(texture, position, rect, color, 0, drawOrigin, 1, SpriteEffects.None, 0);
        }
    }
}