using System;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine.SanguineArmor
{
    internal class SanguineSet : ModPlayer
    {
        public bool sanguineSet = false;
        public override void ResetEffects()
        {
            sanguineSet = false;
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (sanguineSet && !target.immortal)
            {
                if (!item.GetType().Namespace.StartsWith("Erilipah.Items.Sanguine"))
                {
                    player.Heal(Math.Min(10, damage / 10));
                }
                else
                {
                    player.Heal(Math.Min(10, damage / 20));
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (sanguineSet && !target.immortal)
            {
                if (!proj.GetType().Namespace.StartsWith("Erilipah.Items.Sanguine"))
                {
                    player.Heal(Math.Min(10, damage / 10));
                }
                else
                {
                    player.Heal(Math.Min(10, damage / 20));
                }
            }
        }
    }
}
