using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI.Notes
{
    public class NoteUIPages : DragableUIPanel
    {
        private UIHoverImageButton backPage;
        private UIHoverImageButton turnPage;
        private UIHoverImageButton[] indeces;
        private bool[] CollectedNotes => GetInstance<NoteHandler>().GetCollectedNotes();

        private readonly Texture2D pagesTexture; // Cache texture

        private int page;
        private float fade = 0;
        public bool IsVisible { get; private set; } = false;

        public const int pageCount = 7;
        private const int framesX = 3;
        private const int framesY = 3;

        public NoteUIPages()
        {
            pagesTexture = GetTexture("Erilipah/UI/Notes/Pages");
        }

        public void Refresh()
        {
            RemoveAllChildren();
            OnInitialize();
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
            backPage.Width.Set(32, 0);
            backPage.Height.Set(24, 0);
            backPage.OnClick += BackPage;
            Append(backPage);

            Texture2D tpTex = GetTexture("Erilipah/UI/Notes/TurnPage");
            turnPage = new UIHoverImageButton(tpTex, "Turn page")
            {
                HAlign = 1.0f,
                VAlign = 1.0f
            };
            turnPage.Width.Set(32, 0);
            turnPage.Height.Set(24, 0);
            turnPage.OnClick += TurnPage;
            Append(turnPage);

            Texture2D iuTex = GetTexture("Erilipah/UI/Notes/Index");
            Texture2D ilTex = GetTexture("Erilipah/UI/Notes/IndexLocked");
            indeces = new UIHoverImageButton[pageCount];
            for (int i = 0; i < pageCount; i++)
            {
                float hAlign    = i % 6 / 5f;
                float top       = i / 6 * 28;
                indeces[i] = new UIHoverImageButton(CollectedNotes[i] ? iuTex : ilTex, $"{i + 1}")
                {
                    HAlign = hAlign,
                    VAlign = 0
                };
                indeces[i].Top.Set(top, 0);
                indeces[i].Width.Set(24, 0);
                indeces[i].Height.Set(24, 0);
                indeces[i].OnClick += IndexClick;
                Append(indeces[i]);
            }
        }

        public void ToggleVisible()
        {
            IsVisible = !IsVisible;
            PlayNoteSfx("turn_page");
        }

        private void IndexClick(UIMouseEvent evt, UIElement listeningElement)
        {
            string text = ((UIHoverImageButton)listeningElement).HoverText;
            int index = int.Parse(text) - 1;
            
            if (index > page)
            {
                page = index;
                PlayNoteSfx("turn_page");
            }
            if (index < page)
            {
                page = index;
                PlayNoteSfx("back_page");
            }
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

            if (fade < 1 && IsVisible)
            {
                fade += 0.05f;
            }
            if (fade > 0 && !IsVisible)
            {
                fade -= 0.05f;
            }

            for (int i = 0; i < pageCount; i++)
            {
                var visibility = i != page ? (0.7f, 0.4f) : (1f, 1f);
                indeces[i].AcceptingInput = i != page;
                indeces[i].SetVisibility(visibility.Item1, visibility.Item2);
            }

            turnPage.AcceptingInput = page < pageCount - 1;
            backPage.AcceptingInput = page > 0;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsVisible || fade > 0)
                base.Draw(spriteBatch);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Color color = CollectedNotes[this.page] ? Color.White * fade : Color.Black;

            // base.DrawSelf(spriteBatch);
            // Draw the note at the bottom of the ui state
            Rectangle calcDims = GetDimensions().ToRectangle();
            Vector2 dims = new Vector2(calcDims.X + calcDims.Width / 2, calcDims.Y + calcDims.Height - 10);

            Rectangle page = pagesTexture.Frame(framesX, framesY, this.page % framesX, this.page / framesX);
            Vector2 origin = new Vector2(page.Width / 2f, page.Height);
            spriteBatch.Draw(pagesTexture, dims, page, color, 0, origin, Main.UIScale, 0, 0);
        }
    }
}
