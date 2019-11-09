using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI
{
    public class InfectionBar : UIState
    {
        public override void OnInitialize()
        {
            SetPadding(0);
            Width.Set(130, 0);
            Height.Set(28, 0);
            Left.Set(Main.screenWidth - 450, 0.6614f);
            Top.Set(32, 0.04539f);
        }

        private float alpha = 60f;
        private int counter = 0;
        private int scaleCounter = 0;
        private int frameY = 0;
        private int frameX = 0;

        private readonly List<InfectionBarDust> infectedDusts = new List<InfectionBarDust>();


        // 0 = Erilipah
        // 1 = Corrupt
        // 2 = Crimson
        private int GetBiome(Player player)
        {
            if (player.InErilipah())
                return 0;
            if (player.ZoneCorrupt)
                return 1;
            if (player.ZoneCrimson)
                return 2;
            return frameX;
        }

        private bool ActiveOther(Player p) => p.active && !p.dead && p.I().Infection > 0;
        private bool ActiveBar() => Main.LocalPlayer.active && Main.LocalPlayer.I().Infection > 0;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                DrawOthers(spriteBatch);
            }

            if (!ActiveBar() || frameX != GetBiome(Main.LocalPlayer))
            {
                alpha++;
            }
            else
            {
                alpha--;
            }

            alpha = MathHelper.Clamp(alpha, 0, 90);

            if (alpha < 60)
            {
                Vector2 drawPos = new Vector2(Left.Percent * Main.screenWidth / Main.UIScale, Top.Percent * Main.screenHeight);
                DrawBar(spriteBatch, drawPos);
            }
            else if (alpha > 70)
            {
                frameX = GetBiome(Main.LocalPlayer);
            }
        }

        private void DrawBar(SpriteBatch spriteBatch, Vector2 position)
        {
            float infection = Math.Max(0, Main.LocalPlayer.I().Infection);
            float infectionMax = Main.LocalPlayer.I().infectionMax;
            float amount = Math.Max(0, infection);

            Texture2D texture2D = GetTexture("Erilipah/UI/InfectionBar");
            Color color = Color.Lerp(Color.White, Color.White * 0, alpha / 60f);

            counter++;

            if (amount > infectionMax)
            {
                // FrameY animation
                float speedMult = infectionMax / infection / 2;
                int countModulo = (int)(10 * speedMult);
                if (counter % countModulo == 0)
                {
                    frameY++;
                    if (frameY > 22)
                    {
                        frameY = 20;
                    }
                }

                SpewDust(position + new Vector2(Main.rand.NextFloat(115, 120), Main.rand.NextFloat(8, 16)));
            }
            else
            {
                // FrameY animation
                int max = (int)MathHelper.Lerp(0, 20, amount / infectionMax);
                if (counter % 10 == 0)
                {
                    frameY++;
                    if (frameY > max)
                    {
                        frameY = max;
                    }
                }

                // Scale
                if (scaleCounter > 0)
                    scaleCounter--;
                if (scaleCounter < 0)
                    scaleCounter++;
            }

            Rectangle frame = texture2D.Frame(3, 23, frameX, frameY);
            spriteBatch.Draw(texture2D, new Rectangle((int)position.X, (int)position.Y, (int)Width.Pixels, (int)Height.Pixels), frame, color);

            Rectangle bar = new Rectangle((int)position.X, (int)position.Y, 130, 28);
            bool isHovering = bar.Contains(Main.MouseScreen.ToPoint());
            if (isHovering)
            {
                string peepeepoopoo = Math.Round(infection, 1).ToString() + "/" + infectionMax.ToString();
                Vector2 center = Main.MouseScreen + new Vector2(15, 15);

                Main.LocalPlayer.showItemIconText = peepeepoopoo;
                Utils.DrawBorderString(spriteBatch, peepeepoopoo, center, Main.mouseTextColorReal);
                //spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal);
            }
            {
                string peepeepoopoo = "Infection: " + Math.Floor(infection * 100 / infectionMax).ToString() + "%";
                Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                Vector2 center = new Vector2(position.X + Width.Pixels / 2, position.Y - 12);

                spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal * ((60 - alpha) / 60f), 0f, origin,
                    1f, SpriteEffects.None, 0);
            }

            if (Main.rand.Chance(amount / infectionMax / 3f))
                DripDust(position + new Vector2(20, 12) + Main.rand.NextVector2Circular(12, 10));
            DrawDusts(spriteBatch);
        }

        private void DripDust(Vector2 position)
        {
            float scale = Main.rand.NextFloat(0.75f, 0.9f) * ((60 - alpha) / 60);
            Vector2 vel = new Vector2(0, 0.5f);
            Vector2 accel = new Vector2(0, 0.08f * scale);

            infectedDusts.Add(new InfectionBarDust(position, vel, accel, scale, frameX));
        }

        private void SpewDust(Vector2 position)
        {
            float scale = Main.rand.NextFloat(0.9f, 1.1f) * ((60 - alpha) / 60);
            Vector2 vel = new Vector2(2.5f, 0);
            Vector2 accel = new Vector2(-0.06f, -0.08f * scale);

            infectedDusts.Add(new InfectionBarDust(position, vel, accel, scale, frameX));
        }

        private void DrawDusts(SpriteBatch spriteBatch)
        {
            infectedDusts.ForEach(d => d.Update());
            infectedDusts.RemoveAll(d => !d.Active);
            infectedDusts.ForEach(d => d.Draw(spriteBatch));
        }

        private void DrawOthers(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (i == Main.myPlayer)
                    continue;

                Player player = Main.player[i];
                if (ActiveOther(player))
                {
                    float infection = player.I().Infection;
                    float infectionMax = player.I().infectionMax;
                    string peepeepoopoo = Math.Ceiling(infection / infectionMax * 100).ToString() + "%";
                    Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                    Vector2 center = player.Center - new Vector2(0, 40);

                    Utils.DrawBorderString(spriteBatch, peepeepoopoo,
                        center - origin - Main.screenPosition, Color.MediumVioletRed,
                        infection > infectionMax ? infection / infectionMax : 1f); //, SpriteEffects.None, 0);
                }
            }
        }
    }
}
