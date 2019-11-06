using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI
{
    public class ShieldBrokenUI : UIState
    {
        public bool Visible => alpha > 0;
        public float alpha = 0;
        public string time;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (alpha <= 0)
                return;

            Texture2D texture = GetTexture("Erilipah/UI/ShieldBrokenUI");

            Color color = new Color(alpha, alpha, alpha, alpha);
            Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f - 56);

            // If the vitality bar is already here, then move upwards
            if (Erilipah.vitalityBar.Visible)
                position -= new Vector2(0, 24);

            spriteBatch.Draw(texture, position, texture.Bounds, color, 0, texture.Bounds.Center(), 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(Main.fontMouseText, time, position - new Vector2(11, 40), color); // Draw text depicting time til recreation just above
        }
    }
}
