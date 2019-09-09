using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class VoidSpike : ModItem
    {
        // TEST
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Shatters when hitting an enemy at low health, mauling it");
        }
        public override void SetDefaults()
        {
            item.damage = 45;
            item.knockBack = 3.5f;
            item.crit = 9;
            item.melee = true;
            item.noMelee = false;
            item.mana = 9;

            item.maxStack = 1;
            item.useTime =
            item.useAnimation = 27;
            item.useStyle = 1;
            item.autoReuse = true;
            item.UseSound = SoundID.Item1;

            item.width = 40;
            item.height = 48;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = 0;
            item.shootSpeed = 0f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (item.melee)
            {
                var manaTool = tooltips.FirstOrDefault(n => n.mod == "Terraria" && n.Name == "UseMana");
                if (manaTool != null)
                    manaTool.text = "Uses " + item.mana + " life";

                tooltips.Add(new TooltipLine(mod, "RC", "Right click to use mana"));
            }
            else
            {
                tooltips.Add(new TooltipLine(mod, "RC", "Right click to use life"));
            }
        }

        public override bool AltFunctionUse(Player player) => true;

        private bool AnyShards(Player player) => !player.ownedProjectileCounts.Any(p => p == mod.ProjectileType<VoidSpikeProj>());
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.magic = !item.magic;
                item.melee = !item.magic;
                return false;
            }
            return !AnyShards(player);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (!AnyShards(player) && target.life < 0.15 * target.lifeMax && !target.immortal && !target.dontTakeDamage)
            {
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(
                        player.itemLocation,
                        new Vector2(player.direction * Main.rand.NextFloat(3, 5), Main.rand.NextFloat(-2, 2)),
                        mod.ProjectileType<VoidSpikeProj>(),
                        item.damage, item.knockBack, player.whoAmI, target.whoAmI);
                }
            }
        }
    }

    public class VoidSpikeProj : ModProjectile
    {
        private int Target => (int)projectile.ai[0];
        private Player Owner => Main.player[projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            DisplayName.SetDefault("Void");
        }
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 26;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 100;

            projectile.melee = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void AI()
        {
            // Ai1 = timer
            // Local0 = blur length
            projectile.ai[1]++;
            projectile.timeLeft = 2;

            if (Target == -1 || Main.npc[Target].dontTakeDamage)
            {
                // Slow down other velocities; accelerate to player
                projectile.velocity *= 0.8f;
                projectile.velocity += projectile.Center.To(Owner.Center, 0.15f);
                projectile.velocity = Vector2.Clamp(projectile.velocity, Vector2.One * -5, Vector2.One * 5);
                projectile.localAI[0] = 2;

                // Die when returned
                if (projectile.Distance(Owner.Center) < 20)
                    projectile.Kill();
            }
            else if (Main.npc[Target].active)
            {
                if (projectile.ai[1] < 100)
                {
                    projectile.velocity *= 0.93f;
                    projectile.localAI[0] = 0;
                }
                else
                {
                    projectile.rotation = projectile.velocity.X / 13f;
                    projectile.velocity =
                        projectile.Center.To(Main.npc[Target].Center, 5) +
                        projectile.GoTo(Main.npc[Target].Center, 0.15f, 5 + Target / 100f);
                    projectile.localAI[0] = Target / 125f;
                }
            }
            else
            {
                // Find next target; if none, return to player.
                projectile.netUpdate = true;
                var nextTargets = from n in Main.npc
                                  where n.active && n.life < n.lifeMax * 0.15 && n.Distance(projectile.Center) < 350
                                  orderby n.life descending
                                  select n.whoAmI;
                int? selection = nextTargets.FirstOrDefault();
                if (selection == null)
                    projectile.ai[0] = -1;
                else
                    projectile.ai[0] = (int)selection;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.whoAmI == Target)
                projectile.ai[1] = 0;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.localAI[0] <= 0)
                return;

            Texture2D texture = Main.projectileTexture[projectile.type];
            int frames = Main.projFrames[projectile.type];
            Rectangle rect = texture.Frame(1, frames, 0, projectile.frame);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / Main.projFrames[projectile.type]) * 0.5f);
            for (int i = 0; i < Math.Min(projectile.oldPos.Length, projectile.localAI[0]); i++)
            {
                Vector2 drawPos = projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, projectile.gfxOffY);
                Color color = projectile.GetAlpha(lightColor) * ((projectile.localAI[0] - i) / projectile.localAI[0]);
                SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                spriteBatch.Draw(
                    texture: texture, position: drawPos, sourceRectangle: rect, color: color, rotation: projectile.oldRot[i],
                    origin: drawOrigin, scale: projectile.scale, effects: effects, layerDepth: 0
                    );
            }
        }
    }
}
