using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Erilipah.UI
{
    public class UIHoverImageButton : UIImageButton
    {
        public string HoverText;

        public bool AcceptingInput
        {
            get => acceptingInput;
            set
            {
                acceptingInput = value; 
                SetVisibility(value ? 1f : 0.4f, 0.4f);
            }
        }
        private bool acceptingInput;

        public UIHoverImageButton(Texture2D texture, string hoverText) : base(texture)
        {
            HoverText = hoverText;
            AcceptingInput = true;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            if (AcceptingInput)
                base.MouseOver(evt);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (IsMouseHovering && AcceptingInput)
                Main.hoverItemName = HoverText;
        }
    }
}