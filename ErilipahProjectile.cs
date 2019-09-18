using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if (Main.netMode != 1 || projectile.damage > 0 || !Main.LocalPlayer.InErilipah() || 
                projectile.light < 0.15f || Main.rand.Chance(ErilipahItem.LightSnuffRate * 1.2f))
                return;

            Main.PlaySound(SoundID.LiquidsWaterLava, projectile.Center);
            ErilipahItem.SnuffFx(projectile.Center);
            projectile.Kill();
        }
    }
}
