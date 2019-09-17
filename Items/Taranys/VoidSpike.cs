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
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Shatters when hitting an enemy at low health, mauling it");
        }
        public override void SetDefaults()
        {
            item.damage = 45;
            item.knockBack = 3.5f;
            item.crit = 9;
            item.mana = 0;

            item.melee = true;
            item.noMelee = false;
            item.autoReuse = true;
            item.useTurn = true;

            item.maxStack = 1;
            item.useTime = item.useAnimation = 21;
            item.useStyle = 1;
            item.UseSound = SoundID.Item1;

            item.width = 52;
            item.height = 66;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = 0;
            item.shootSpeed = 0f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (item.melee)
            {
                tooltips.Add(new TooltipLine(mod, "RC", "Right click to use magic damage"));
            }
            else
            {
                tooltips.Add(new TooltipLine(mod, "RC", "Right click to use melee damage"));
            }
        }

        public override bool AltFunctionUse(Player player) => true;

        private bool AnyShards(Player player)
        {
            for (int i = 0; i < 1000; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == mod.ProjectileType<VoidSpikeProj>())
                    return true;
            }
            return false;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Main.PlaySound(2, player.Center, 29);
                item.magic = !item.magic;
                item.melee = !item.magic;

                if (item.magic)
                {
                    item.damage = 38;
                    item.useTime = item.useAnimation = 16;
                    item.mana = 4;
                }
                else
                {
                    item.damage = 45;
                    item.useTime = item.useAnimation = 21;
                    item.mana = 0;
                }
                return true;
            }
            return !AnyShards(player);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (!AnyShards(player) && target.lifeMax > 200 && target.life < 0.25 * target.lifeMax && !target.immortal && !target.dontTakeDamage)
            {
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(
                        player.itemLocation,
                        new Vector2(player.direction * Main.rand.NextFloat(3, 5), Main.rand.NextFloat(-2, 2)),
                        mod.ProjectileType<VoidSpikeProj>(),
                        55, item.knockBack, player.whoAmI, target.whoAmI);
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
            projectile.width = 42;
            projectile.height = 54;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 100;

            projectile.maxPenetrate = 100;
            projectile.penetrate = 100;

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
            projectile.rotation += projectile.velocity.Length() / 20;
            projectile.timeLeft = 2;

            if (Target == -1 || Main.npc[Target].dontTakeDamage)
            {
                // Slow down other velocities; accelerate to player
                projectile.velocity = projectile.Center.To(Owner.Center, 8f);
                projectile.localAI[0] = 2;

                // Die when returned
                if (projectile.Distance(Owner.Center) < 20)
                    projectile.Kill();
            }
            else if (Main.npc[Target].active)
            {
                if (!ValidNPC(Main.npc[Target]))
                {
                    projectile.ai[0] = -1;
                    return;
                }

                if (projectile.ai[1] < 20)
                {
                    projectile.velocity *= 0.90f;
                    projectile.localAI[0] = 0;
                }
                else
                {
                    projectile.velocity = projectile.Center.To(Main.npc[Target].Center, 17);
                    projectile.localAI[0] = 3;
                }
            }
            else
            {
                // Find next target; if none, return to player.
                projectile.netUpdate = true;
                var nextTargets = from n in Main.npc
                                  where ValidNPC(n) && n.Distance(projectile.Center) < 300
                                  orderby n.life descending
                                  select n.whoAmI;
                int? selection = nextTargets.FirstOrDefault();
                if (selection == null)
                    projectile.ai[0] = -1;
                else
                    projectile.ai[0] = (int)selection;
            }
        }

        private static bool ValidNPC(NPC n)
        {
            return n.active && !n.friendly && !n.dontTakeDamage && (n.life < n.lifeMax * 0.20 || n.lifeMax < 200 && n.defense < 50);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[projectile.owner] = 5;
            if (damage <= 1)
                projectile.ai[0] = -1;
            if (target.whoAmI == Target)
                projectile.ai[1] = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            if (projectile.localAI[0] > 0 && projectile.velocity.Length() > 0)
            {
                Rectangle rect = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
                float variable = Math.Min(projectile.oldPos.Length, projectile.localAI[0]);
                for (int i = 0; i < variable; i++)
                {
                    Vector2 drawPos = projectile.oldPos[i] - Main.screenPosition + rect.Size() / 2 + new Vector2(0, projectile.gfxOffY);
                    Color color = projectile.GetAlpha(drawColor) * ((variable - i) / (float)variable);
                    SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: rect, color: color, rotation: projectile.oldRot[i],
                        origin: rect.Size() / 2, scale: projectile.scale, effects: effects, layerDepth: 0
                        );
                }
            }

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, Color.White * 0.8f, projectile.rotation, texture.Size() / 2, 1, 0, 0);
            return false;
        }
    }
}
