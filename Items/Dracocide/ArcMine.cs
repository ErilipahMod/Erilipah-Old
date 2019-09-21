using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class ArcMine : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Fires Arc Mines that interconnect and damage NPCs between them\n" +
                "Right click to activate the Mines or deactivate them\n" +
                "They never despawn but moving too far away deactivates them");
        }
        public override void SetDefaults()
        {
            item.damage = 30;
            item.knockBack = 1;
            item.crit = 6;
            item.magic = true;
            item.noMelee = true;
            item.noUseGraphic = true;

            item.mana = 8;
            item.maxStack = 1;
            item.useTime =
            item.useAnimation = 20;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.autoReuse = true;
            item.useTurn = true;

            item.width = 18;
            item.height = 48;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<ArcMineProj>(); // FINISH
            item.shootSpeed = 6f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<ArcJoint>(), 3);
            recipe.AddIngredient(mod.ItemType<Dracocell>(), 8);

            recipe.AddTile(TileID.MythrilAnvil);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltip = tooltips.Find(x => x.Name == "UseMana");
            if (tooltip == null)
                return;
            tooltip.text = "Uses 3 mana per arc per second";
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.shoot = 0;
                var mines = Main.projectile.Where(x => x.active && x.type == mod.ProjectileType<ArcMineProj>());

                if (player.altFunctionUse == 2)
                    foreach (var mine in mines)
                        mine.ai[0]++;
            }
            else
            {
                item.shoot = mod.ProjectileType<ArcMineProj>();
            }
            return base.CanUseItem(player);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }

    public class ArcMineProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 12;
            DisplayName.SetDefault("Arc Mine");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 48;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 99999999;

            projectile.magic = true;
            projectile.maxPenetrate = -1;
            projectile.hostile = projectile.friendly = false; // don't damage anything directly
        }

        private Player player => Main.player[projectile.owner];
        private List<Projectile> LikeMe => Main.projectile.Where(x => x.active && x.type == projectile.type && x.ai[0] == 1).ToList();

        private float dustSpawnPos { get => projectile.ai[1]; set => projectile.ai[1] = value; }

        public override void AI() // finish AI tomorrow
        {
            Lighting.AddLight(projectile.Center, 0.3f, 0.1f, 0);
            List<Projectile> Friends = LikeMe;
            Friends.Remove(projectile);

            if (projectile.localAI[1]++ % 20 == 0 && projectile.ai[0] == 1) // reduce mana
            {
                player.manaRegenDelay = 180;
                player.statMana--;
            }

            if (player.statMana < 6 && projectile.ai[0] == 1 && LikeMe[0] == projectile)
            {
                if (player.manaFlower) // If they have a mana flower, use it
                    player.QuickMana();
                if (player.statMana < 5) // If they're still low mana, d i e
                    projectile.Kill();
            }

            projectile.velocity *= 0.95f;

            // Animation handling
            if (projectile.ai[0] == 0) // If unactivated, cycle through.
            {
                projectile.Animate(5, 4, 0);
                return;
            }

            if (projectile.ai[0] == 1) // If activated, buckle up and cycle through.
            {
                if (projectile.frameCounter < 32)
                {
                    projectile.frameCounter++;
                    projectile.frame = (int)(projectile.frameCounter / 4f);
                }
                else
                {
                    projectile.Animate(5, 4, 8);
                }
            }

            if (projectile.ai[0] == 2 || player.Distance(projectile.Center) > 2200) // KILL YOURSELF.
                projectile.Kill();

            foreach (var friend in Friends)
            {
                // Set the NPC's rotation between its other friends.
                if (Friends.Count == 1) // if only one, just face it.
                    projectile.rotation = (Friends[0].Center - projectile.Center).ToRotation() + MathHelper.PiOver2;

                else if (Friends.IndexOf(projectile) == 1)
                    projectile.rotation = MathHelper.Lerp(Friends[0].rotation, Friends[1].rotation, 0.5f) + MathHelper.Pi;
                else // otherwise, average.
                    projectile.rotation = Friends.Average(x => (x.Center - projectile.Center).ToRotation()) + MathHelper.PiOver2;
            }

            // Also set up the "arcs," which are just Collision checks. If the player is colliding with a line, hurt him.
            foreach (var friend in Friends)
            {
                foreach (var Target in Main.npc)
                {
                    // Is target colliding with a line?
                    bool hittingLine = Collision.CheckAABBvLineCollision(
                    Target.position,
                    new Vector2(Target.Hitbox.Width, Target.Hitbox.Height),
                    projectile.Center,
                    friend.Center);

                    if (hittingLine && !Target.townNPC && !Target.friendly && Target.immune[projectile.owner] <= 0) // Otherwise (and if yes), run the NPC's code. 
                    {
                        Target.StrikeNPC((int)(30 * player.magicDamage), 3f, Target.velocity.X > 0 ? -1 : 1);
                        Target.immune[projectile.owner] = 15;
                    }
                }
                // Zoop
                dustSpawnPos += Main.rand.NextFloat() / 20f;
                dustSpawnPos %= 1f;

                Vector2 dir = (friend.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 18;

                Dust dust = Dust.NewDustPerfect(
                    Vector2.Lerp(projectile.Center + dir, friend.Center - dir, dustSpawnPos),
                    mod.DustType<DracocideDust>());
                dust.noLight = true;
                dust.velocity = Vector2.Zero;
                dust.customData = -20;
                dust.fadeIn = 10;
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
                Dust.NewDust(projectile.Center, 0, 0, DustID.Fire, Main.rand.NextFloat() * 2, Main.rand.NextFloat() * -2);
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
        }
    }
}