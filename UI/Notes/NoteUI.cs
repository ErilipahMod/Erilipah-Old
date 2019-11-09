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
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI.Notes
{
    public class NoteUI : UIState
    {
        private UIHoverImageButton showButton;
        private NoteUIPages pages;

        public static bool Visible => !Main.gameMenu && Main.playerInventory;

        public override void OnInitialize()
        {
            Texture2D showTex = GetTexture("Erilipah/UI/Notes/ShowNotes");
            showButton = new UIHoverImageButton(showTex, "Show notes");
            showButton.Left.Set(410, 0);
            showButton.Top.Set(260, 0);
            showButton.Width.Set(26, 0);
            showButton.Height.Set(28, 0);
            showButton.OnClick += ShowButtonClicked;
            Append(showButton);

            pages = new NoteUIPages { HAlign = 0.5f, VAlign = 0.5f };
            pages.Left.Set(0, 0);
            pages.Top.Set(0, 0);
            pages.Width.Set(124, 0);
            pages.Height.Set(162, 0);
            Append(pages);
        }

        private void ShowButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            pages.ToggleVisible();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    public class NoteUIPages : DragableUIPanel
    {
        private UIHoverImageButton backPage;
        private UIHoverImageButton turnPage;

        private readonly Texture2D pagesTexture; // Cache texture

        private int page;
        private float fade = 0;
        private bool visible = false;

        private const int pageCount = 7;
        private const int framesX = 3;
        private const int framesY = 3;

        public NoteUIPages()
        {
            pagesTexture = GetTexture("Erilipah/UI/Notes/Pages");
        }

        private static void PlayNoteSfx(string filename)
        {
            Main.PlaySound(GetInstance<Erilipah>().GetLegacySoundSlot(SoundType.Custom, $"Sounds/Notes/{filename}")
                .WithPitchVariance(0.4f));
        }

        public override void OnInitialize()
        {
            Texture2D bpTex = GetTexture("Erilipah/UI/Notes/BackPage");
            backPage = new UIHoverImageButton(bpTex, "Back a page")
            {
                HAlign = 0.0f,
                VAlign = 1.0f
            };
            backPage.Left.Set(-10, 0);
            backPage.Top.Set(10, 0);
            backPage.Width.Set(32, 0);
            backPage.Height.Set(24, 0);
            backPage.OnClick += BackPage;
            Append(backPage);

            Texture2D tpTex = GetTexture("Erilipah/UI/Notes/TurnPage");
            turnPage = new UIHoverImageButton(bpTex, "Turn page")
            {
                HAlign = 1.0f,
                VAlign = 1.0f
            };
            turnPage.Left.Set(10, 0);
            turnPage.Top.Set(10, 0);
            turnPage.Width.Set(32, 0);
            turnPage.Height.Set(24, 0);
            turnPage.OnClick += TurnPage;
            Append(turnPage);
        }

        public void ToggleVisible()
        {
            visible = !visible;
            PlayNoteSfx("turn_page");
        }

        private void TurnPage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (page < pageCount - 1)
            {
                page++;
                PlayNoteSfx("turn_page");
            }
        }

        private void BackPage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (page > 0)
            {
                page--;
                PlayNoteSfx("back_page");
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (fade < 1 && visible)
            {
                fade += 0.06f;
            }
            if (fade > 0 && !visible)
            {
                fade -= 0.06f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (visible && fade > 0)
                base.Draw(spriteBatch);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 dims = GetDimensions().Center();
            Rectangle page = pagesTexture.Frame(framesX, framesY, this.page % framesX, this.page / framesX);
            spriteBatch.Draw(pagesTexture, dims, page, Color.White, 0, page.Size() / 2, 1f, 0, 0);
        }
    }
}
