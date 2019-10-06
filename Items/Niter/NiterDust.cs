using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Niter
{
    public class NiterDust : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.X *= 0.9f;
                dust.velocity.Y -= 0.08f;
            }
            dust.position += dust.velocity;

            dust.scale -= 0.0175f;
            if (dust.scale < 0.1f)
                dust.active = false;

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White * 0.75f;
        }
    }
}
