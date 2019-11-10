using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI.Notes
{
    public class NoteUI : UIState
    {
        private UIHoverImageButton showButton;
        private NoteUIPages pages;
        private bool flashing = false;
        private float flash_c = 0.4f;

        public static bool Visible => !Main.gameMenu && Main.playerInventory;

        public void Refresh(bool notif = true)
        {
            pages.Refresh();
            if (notif && !pages.IsVisible)
                flashing = true;
        }

        public override void OnInitialize()
        {
            bool autotrash = Terraria.ModLoader.ModLoader.GetMod("AutoTrash") != null;
            Texture2D showTex = GetTexture("Erilipah/UI/Notes/ShowNotes");
            showButton = new UIHoverImageButton(showTex, "Show notes");
            showButton.Left.Set(autotrash ? 350 : 410, 0);
            showButton.Top.Set(260, 0);
            showButton.Width.Set(26, 0);
            showButton.Height.Set(28, 0);
            showButton.OnClick += ShowButtonClicked;
            Append(showButton);

            pages = new NoteUIPages { HAlign = 0.5f, VAlign = 0.5f };
            pages.SetPadding(0);
            pages.Left.Set(0, 0);
            pages.Top.Set(0, 0);
            pages.Width.Set(140 + 20, 0); // Page size + index width + turn/back page
            pages.Height.Set(162 + 58, 0); // Page size + index height + turn/back page
            Append(pages);
        }

        private void ShowButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            pages.ToggleVisible();

            if (pages.IsVisible)
            {
                showButton.HoverText = "Hide notes";
            }
            else
            {
                showButton.HoverText = "Show notes";
            }

            flashing = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (flashing)
            {
                flash_c += 0.1f;
                double amp = (Math.Sin(flash_c) + 1.3) / 2.0;
                showButton.SetVisibility(1f, (float)amp);
            }
            else
            {
                flash_c = 0.4f;
                showButton.SetVisibility(1f, 0.4f);
            }
        }
    }
}
