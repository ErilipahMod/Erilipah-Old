using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class VoidSpike : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("");
        }
        public override void SetDefaults()
        {
            item.damage = 32;
            item.knockBack = 1;
            item.crit = 0;
            item.melee = true;
            item.noMelee = false;

            item.maxStack = 1;
            item.useTime =
            item.useAnimation = 20;
            item.useStyle = 1;
            item.autoReuse = true;
            item.UseSound = SoundID.Item1;

            item.width = 32;
            item.height = 32;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.Blue;

            item.shoot = 0;
            item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    public class VoidSpikeProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Void");
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 3600;

            projectile.melee = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void AI()
        {
            base.AI();
        }
    }
}
