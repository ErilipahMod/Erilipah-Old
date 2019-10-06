using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class Nidorose : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Enemies around your cursor are damaged and weakened");
        }
        public override void SetDefaults()
        {
            item.width = 48;
            item.height = 38;

            item.damage = 45;
            item.knockBack = 0;
            item.crit = 6;
            item.magic = true;

            item.maxStack = 1;
            item.holdStyle = ItemHoldStyleID.HoldingOut;
            item.autoReuse = false;

            item.value = 1500;
            item.rare = ItemRarityID.Pink;

            item.shoot = mod.ProjectileType<NidoroseProj>();
            item.shootSpeed = 0.01f;
        }

        public override bool CanUseItem(Player player) => false;

        public override void HoldItem(Player player)
        {
            int index = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == item.shoot)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, item.shoot, item.damage, item.knockBack, player.whoAmI);
            }
            else
            {
                Main.projectile[index].Center = player.Center;
                Main.projectile[index].timeLeft = 2;
            }

            player.itemLocation.X -= item.width / 2 * player.direction;
            player.itemLocation += new Vector2(6 * -player.direction, 6);
            // Add projectile spawn here

            if (player.whoAmI == Main.myPlayer)
                for (int i = 0; i < 50; i++)
                {
                    Vector2 dustPos = Main.MouseWorld + Vector2.UnitX.RotatedBy(i / 50f * MathHelper.TwoPi) * 100;
                    float bright = Lighting.Brightness((int)(dustPos.X / 16), (int)(dustPos.Y / 16));

                    Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(1, 1), mod.DustType<NPCs.ErilipahBiome.VoidParticle>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = bright * 0.8f;

                    dust = Dust.NewDustPerfect(dustPos, mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = (1 - bright) * 0.8f;
                }
        }

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);
        public override bool AltFunctionUse(Player player) => player.statMana >= 15;
    }

    public class NidoroseProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        private float Pulse { get => projectile.ai[1]; set => projectile.ai[1] = value; }

        public override void AI()
        {
            projectile.netUpdate = true;

            Focus();
        }

        private void Focus()
        {
            Player player = Main.player[projectile.owner];

            Pulse++;

            if (Pulse % 15 == 0)
                player.statMana -= 1;

            // Using Pulse to sync it up
            projectile.ai[0] = -1;
            if (Main.myPlayer == player.whoAmI)
            {
                projectile.netUpdate = true;
                projectile.Center = Main.MouseWorld;
                projectile.ai[0] = projectile.FindClosestNPC(110, true, true);
            }

            if (projectile.ai[0] > -1)
            {
                NPC target = Main.npc[(int)projectile.ai[0]];
                if (target.immune[player.whoAmI] <= 0)
                {
                    target.netUpdate = true;
                    target.immune[player.whoAmI] = (int)(25 * (1 - Pulse / (300 + Pulse)));
                    player.ApplyDamageToNPC(target, projectile.damage + (int)Pulse / 30, 0, 0, false);
                    player.addDPS((int)((projectile.damage + Pulse / 30) / (60 / (25 * (1 - Pulse / (300 + Pulse))))));
                }
            }
            else
            {
                Pulse = 0;
            }
        }

        public override void Kill(int timeLeft)
        {
        }
    }

}
