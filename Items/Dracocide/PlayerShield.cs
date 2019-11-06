using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Dracocide
{
    public class ShieldBroken : UIState
    {
        public bool Visible => alpha > 0;
        public float alpha = 0;
        public string time;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (alpha <= 0)
                return;

            Texture2D texture = ModContent.GetTexture("Erilipah/Items/Dracocide/BrokenShield");

            Color color = new Color(alpha, alpha, alpha, alpha);
            Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f - 56);

            // If the vitality bar is already here, then move upwards
            if (Erilipah.vitalityBar.Visible)
                position -= new Vector2(0, 24);

            spriteBatch.Draw(texture, position, texture.Bounds, color, 0, texture.Bounds.Center(), 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(Main.fontMouseText, time, position - new Vector2(11, 40), color); // Draw text depicting time til recreation just above
        }
    }

    public class PlayerShield : ModPlayer
    {
        public int brokenTimer = 0;
        public int life = 250;

        public override void PostUpdate()
        {
            // If the shield died, break it for 7500 ticks or 125 seconds
            if (brokenTimer % 15 == 0 && life < 250)
                life++;

            if (brokenTimer <= 0 && life <= 0)
            {
                brokenTimer = 3600;
                life = -100;
            }

            brokenTimer--;

            if (brokenTimer < 300) // If approaching the end of the broken timer, gradually fade out.
                Erilipah.shieldBroken.alpha = brokenTimer / 450f;
            else // Otherwise if the timer is active, half opacity
                Erilipah.shieldBroken.alpha = 2 / 3f;

            if (brokenTimer < 60) // Add a decimal place if below 1 second
                Erilipah.shieldBroken.time = Math.Round(brokenTimer / 60f, 1).ToString();
            else // Else just do normally
                Erilipah.shieldBroken.time = (brokenTimer / 60).ToString();
        }
    }

    public class NPCHitShield : GlobalNPC
    {
        public override void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (target.type == NPCType<ShieldProjectorProj>() && npc.velocity.Length() >= 4.5f)
            {
                if (!npc.Reflect(1, !npc.boss, !npc.dontCountMe))
                    Main.player[(int)target.ai[0]].immuneTime = 45;
            }
            else
            {
                damage = 0;
            }
        }
    }
}
