using Terraria;
using Terraria.ID;

namespace Erilipah.Items.LunarBee
{
    public class LunacritaProj : HoverShoot
    {
        public override string GlowTexture => "Erilipah/Items/LunarBee/LunacritaProjGlow";
        public override void SetDefaults()
        {
            projectile.netImportant = true;
            projectile.width = 48;
            projectile.height = 42;
            projectile.friendly = true;
            projectile.minion = true;
            projectile.tileCollide = false;
            projectile.minionSlots = 1;
            projectile.penetrate = -1;
            projectile.timeLeft = 18000;
            projectile.ignoreWater = true;
            inertia = 16f;
            shoot = mod.ProjectileType("MoonStingerFriendly");
            shootSpeed = 14f;
            shootCool = 120f;
            viewDist = 65f;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 4;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void CheckActive()
        {
            Player player = Main.player[projectile.owner];
            if (player.HasBuff(mod.BuffType("Lunacrita")))
            {
                projectile.timeLeft = 2;
            }
        }

        public override void SelectFrame()
        {
            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frameCounter = 0;
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
            }
        }
    }
}