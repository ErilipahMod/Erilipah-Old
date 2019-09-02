using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Erilipah.Items.ErilipahBiome
{
    public class Aboryc : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sigil of Darkness");
            Tooltip.SetDefault("It whispers for you to hold it still in Erilipah's darkness\n" +
                "'Do not fear the night\nWhen you hold my hand\nFor in the dark, I am light'");
        }

        public override void SetDefaults()
        {
            item.maxStack = 1;
            item.channel = true;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.noUseGraphic = true;

            item.width = 24;
            item.height = 20;

            item.value = 0;
            item.rare = ItemRarityID.LightRed;
        }

        // Held item hook is managed in Infection.cs

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<BioluminescentSinew>(), 2);
            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 3);
            recipe.AddTile(TileID.DemonAltar);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public class AbProj : ModProjectile
        {
            public override void SetStaticDefaults()
            {
                Main.projFrames[projectile.type] = 2;
            }

            public override void SetDefaults()
            {
                projectile.width = 24;
                projectile.height = 20;

                projectile.tileCollide = false;
                projectile.timeLeft = 2;
            }

            public override void AI()
            {
                const float scaleTime = 180;
                const float time = 107;
                const float dist = 20;

                Player player = Main.player[projectile.owner];
                var pos = new Vector2(player.Center.X, player.Center.Y - 100);

                // Ai0 = floating counter
                // Ai1 = scale counter
                // LocalAi0 = rising counter

                projectile.localAI[0]++;

                if (projectile.ai[0] < time)
                    projectile.ai[0]++;
                else
                    projectile.ai[0] = -time;

                if (projectile.ai[1] < scaleTime)
                    projectile.ai[1]++;
                else
                    projectile.ai[1] = -scaleTime;

                if (projectile.frame > 0)
                    Lighting.AddLight(projectile.Center, 
                        new Vector3(1.2f, 1.0f, 0.9f * projectile.scale) * projectile.scale * ((255 - projectile.alpha) / 255f));

                projectile.ai[1]++;
                projectile.scale = MathHelper.SmoothStep(0.92f, 1.08f, Math.Abs(projectile.ai[1]) / scaleTime);
                projectile.netUpdate = true;

                const int summonTime = 200;
                if (projectile.localAI[0] > 450)
                {
                    if (projectile.localAI[0] < 450 + summonTime)
                    {
                        // Lock the player in
                        player.channel = true;
                        player.itemAnimation = 30;
                        player.itemAnimationMax = 30;
                        player.itemTime = 30;
                        player.heldProj = projectile.whoAmI;
                    }
                    projectile.timeLeft = 2;
                }

                if (projectile.localAI[0] > 500 + summonTime)
                {
                    if (projectile.alpha > 0)
                        projectile.alpha--;
                    projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X + player.direction * 17.5f, player.Center.Y), 0.08f);
                    return;
                }
                if (projectile.localAI[0] == 450 + summonTime)
                {
                    if (Main.netMode != 1)
                        NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType<NPCs.Taranys.Taranys>());
                    NPC t = Main.npc[NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>())];
                    t.Center = projectile.Center;
                    projectile.velocity = Vector2.Zero;
                    projectile.alpha = 255;
                    return;
                }
                if (projectile.localAI[0] >= 450 && projectile.localAI[0] < 450 + summonTime)
                {
                    Vector2 newPos = new Vector2(player.Center.X, player.Center.Y - 200);
                    Vector2 distance = new Vector2(0, summonTime - (projectile.localAI[0] - 450)) / 2f;
                    float rotation = (float)Math.Pow(projectile.localAI[0] - 450, 1.81f);
                    rotation /= 100f;

                    projectile.Center = newPos + distance.RotatedBy(rotation);
                    return;
                }

                if (projectile.localAI[0] < 400)
                    projectile.Center = new Vector2(pos.X,MathHelper.SmoothStep(pos.Y - dist, pos.Y + dist, Math.Abs(projectile.ai[0]) / time));
                else if (projectile.localAI[0] < 450)
                    projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X, player.Center.Y - 100), 0.1f);
            }

            public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];
                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, 
                    texture.Frame(1, 2, 0, projectile.frame), Color.White * ((255 - projectile.alpha) / 255f), 0f, 
                    Main.projectileTexture[projectile.type].Size() / 2, projectile.scale, 0, 1);
                return false;
            }
        }
    }
}
