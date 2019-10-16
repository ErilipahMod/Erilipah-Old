using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Taranys
{
    public class VoidSpike : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Hunts enemies down when they're close to death\nRight click to return it");
        }

        public override void SetDefaults()
        {
            item.damage = 45;
            item.knockBack = 2.2f;
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

        public override bool AltFunctionUse(Player player) => true;

        private bool AnyShards(Player player)
        {
            for (int i = 0; i < 1000; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == ProjectileType<VoidSpikeProj>())
                    return true;
            }
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && player.itemTime <= 0)
            {
                Main.PlaySound(2, player.Center, 29);

                Projectile proj/*changed to pv error*/ = Main.projectile.FirstOrDefault(p => p.active && p.type == ProjectileType<VoidSpikeProj>() && p.owner == player.whoAmI);
                if (proj != null)
                {
                    proj.ai[0] = -2;
                    proj.netUpdate = true;
                }

                player.itemTime = 35;

                return false;
            }
            return !AnyShards(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                Dust.NewDustDirect(hitbox.Location.ToVector2(), hitbox.Width, hitbox.Height, DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Scale: 1.25f).noGravity = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            bool smallTarget = target.life < 700 && !target.boss && target.life < 0.50 * target.lifeMax;
            bool lowLife = smallTarget || target.life < 0.30 * target.lifeMax;
            if (!AnyShards(player) && target.life > damage && lowLife && !target.immortal && !target.dontTakeDamage)
            {
                if (Main.netMode != 1)
                {
                    Projectile p = Main.projectile[Projectile.NewProjectile(
                        player.itemLocation,
                        new Vector2(player.direction * 6, Main.rand.NextFloat(-3.5f, 3.5f)),
                        ProjectileType<VoidSpikeProj>(),
                        player.GetWeaponDamage(item), item.knockBack, player.whoAmI, target.whoAmI, 21)];
                    p.magic = item.magic;
                    p.melee = item.melee;
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
            projectile.timeLeft = 10000;

            projectile.maxPenetrate = 250;
            projectile.penetrate = 250;

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

            if (projectile.Distance(Owner.Center) > 1200 || projectile.damage <= 0 || !projectile.friendly || projectile.hostile)
            {
                projectile.Kill();
            }

            Vector2 tip = projectile.position + new Vector2(36, 0).RotatedBy(projectile.rotation, projectile.Size / 2);
            Vector2 lerp = (projectile.position - projectile.oldPosition) / 2 + projectile.oldPosition + new Vector2(36, 0).RotatedBy(projectile.rotation, projectile.Size / 2);
            Dust.NewDustPerfect(tip, DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Vector2.Zero, Scale: 1.4f).noGravity = true;
            Dust.NewDustPerfect(lerp, DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Vector2.Zero, Scale: 1.4f).noGravity = true;

            if (Target == -1)
            {
                projectile.ai[1] = 0;
                projectile.netUpdate = true;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].boss && ValidNPC(Main.npc[i]))
                    {
                        projectile.ai[0] = i;
                        return;
                    }
                    bool close = Vector2.Distance(Main.npc[i].Center, projectile.Center) < 350 && Vector2.Distance(Main.npc[i].Center, Owner.Center) < 500;
                    if (close && !Main.npc[i].boss && ValidNPC(Main.npc[i]))
                    {
                        projectile.ai[0] = i;
                        return;
                    }
                }
                projectile.ai[0] = -2;
            }
            else if (Target == -2)
            {
                // Slow down other velocities; accealerate to player when done swingin
                if (projectile.ai[1] > 20)
                {
                    float speed = MathHelper.Lerp(6f, 12f, Vector2.Distance(Owner.Center, projectile.Center) / 500f);
                    projectile.velocity = projectile.Center.To(Owner.Center, speed);
                    projectile.localAI[0] = 3;
                }
                else
                {
                    projectile.velocity *= 0.90f;
                }

                // Die when returned
                if (projectile.Distance(Owner.Center) < 20)
                    projectile.Kill();
            }
            else if (ValidNPC(Main.npc[Target]))
            {
                if (projectile.ai[1] <= 20)
                {
                    projectile.netUpdate = true;
                    projectile.velocity *= 0.90f;
                    projectile.localAI[0] = 0;
                }
                else
                {
                    if (projectile.Hitbox.Intersects(Main.npc[Target].Hitbox))
                    {
                        projectile.ai[1] = 0;
                    }

                    projectile.velocity = projectile.Center.To(Main.npc[Target].Center, 17);
                    projectile.localAI[0] = 3;
                }
            }
            else
            {
                projectile.ai[0] = -1;
            }
        }

        private bool ValidNPC(NPC n)
        {
            return n.active && !n.immortal && !n.friendly && !n.dontTakeDamage && projectile.CanHit(n) && (n.life < n.lifeMax * 0.30 || n.life < 700 && n.defense < 50 && !n.boss);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (damage <= 1)
                projectile.ai[0] = -1;
            //if (target.whoAmI == Target && projectile.ai[1] > 20)
            //    projectile.ai[1] = 0;
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
                    Color color = projectile.GetAlpha(drawColor) * ((variable - i) / variable) * 0.8f;
                    SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: rect, color: color, rotation: projectile.rotation,
                        origin: rect.Size() / 2, scale: projectile.scale, effects: effects, layerDepth: 0
                        );
                }
            }

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, Color.White * 0.8f, projectile.rotation, texture.Size() / 2, 1, 0, 0);
            return false;
        }
    }
}
