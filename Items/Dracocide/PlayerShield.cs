using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Dracocide
{
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
                Erilipah.shieldBrokenUI.alpha = brokenTimer / 450f;
            else // Otherwise if the timer is active, half opacity
                Erilipah.shieldBrokenUI.alpha = 2 / 3f;

            if (brokenTimer < 60) // Add a decimal place if below 1 second
                Erilipah.shieldBrokenUI.time = Math.Round(brokenTimer / 60f, 1).ToString();
            else // Else just do normally
                Erilipah.shieldBrokenUI.time = (brokenTimer / 60).ToString();
        }
    }

    public class NPCHitShield : GlobalNPC
    {
        public override void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (target.type == NPCType<ShieldProjection>())
            {
                if (npc.velocity.Length() >= 4.5f)
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
}
