﻿using Erilipah.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Projectiles
{
    public class ErilipahProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            bool light = projectile.damage <= 0 && !projectile.friendly && !projectile.hostile && projectile.light >= 0.12f;
            if (Main.netMode != 1 && Main.LocalPlayer.InErilipah() && !Main.rand.Chance(ErilipahItem.LightSnuffRate * 1.3f) && light)
            {
                Main.PlaySound(SoundID.LiquidsWaterLava, projectile.Center);
                ErilipahItem.SnuffFx(projectile.Center);
                projectile.Kill();
            }
        }
    }
}
