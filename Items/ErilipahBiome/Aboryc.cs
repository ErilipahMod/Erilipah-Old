using Erilipah.Items.Crystalline;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class Aboryc : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sigil of Darkness");
            Tooltip.SetDefault("It whispers for you to hold it still in Erilipah's absolute darkness\n" +
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

        public override bool UseItem(Player player)
        {
            Projectile.NewProjectile(player.Center, Vector2.Zero, mod.ProjectileType<AbProj>(), 0, 0, player.whoAmI);
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            bool noProjs = !Main.projectile.Any(p => p.active && p.type == mod.ProjectileType<AbProj>());
            bool noTaranys = !NPC.AnyNPCs(mod.NPCType<NPCs.Taranys.Taranys>());
            return noProjs && noTaranys && player.velocity == Vector2.Zero && player.Brightness() <= 0.1f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 5);
            recipe.AddIngredient(mod.ItemType<InfectionModule>(), 5);
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

            private float Timer { get => projectile.ai[1]; set => projectile.ai[1] = value; }
            public override void AI()
            {
                Player player = Main.player[projectile.owner];
                Vector2 pos = new Vector2(player.Center.X, player.Center.Y - 100);
                projectile.timeLeft = 10;

                // Ai0 = floating counter
                // LocalAi0 = scale counter

                Timer += 1f;

                Effects(180, 107, 20, pos);

                if (Timer < 730 || NPC.AnyNPCs(mod.NPCType<NPCs.Taranys.Taranys>()))
                    Ritual(player);
                else
                    AlAltar(player);
            }

            private void Effects(float scaleTime, float hoverTime, float hoverDist, Vector2 pos)
            {
                if (projectile.ai[0] < hoverTime)
                    projectile.ai[0]++;
                else
                    projectile.ai[0] = -hoverTime;

                if (projectile.localAI[0] < scaleTime)
                    projectile.localAI[0]++;
                else
                    projectile.localAI[0] = -scaleTime;

                if (Timer < 500 && (Timer < 300 || Vector2.Distance(projectile.Center, pos) > 3))
                    projectile.Center = new Vector2(pos.X, MathHelper.SmoothStep(pos.Y - hoverDist, pos.Y + hoverDist, Math.Abs(projectile.ai[0]) / hoverTime));
                else if (Timer < 500)
                    projectile.Center = pos;

                if (projectile.frame > 0)
                    Lighting.AddLight(projectile.Center, new Vector3(1.2f, 1.0f, 1.2f * projectile.scale) * projectile.scale);

                projectile.scale = MathHelper.SmoothStep(0.92f, 1.08f, Math.Abs(projectile.localAI[0]) / scaleTime);
                projectile.netUpdate = true;
            }

            private void Ritual(Player player)
            {
                if ((!player.channel && Timer < 220) || (player.dead && NPC.AnyNPCs(mod.NPCType<NPCs.Taranys.Taranys>())))
                {
                    projectile.Kill();
                }
                player.GetModPlayer<InfectionPlr>().darknessCounter--;

                const int summonTime = 200;
                if (Timer < 220) { }
                else if (Timer == 220)
                {
                    Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, 0.2f);
                }
                else if (Timer < 300)
                {
                    Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 103, 0.2f, 0.3f);
                    Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                        .customData = 0f;
                }
                else if (Timer == 300)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                            .customData = 0f;
                        Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<VoidParticle>(), Main.rand.NextVector2CircularEdge(5, 5))
                            .customData = 0f;
                    }
                    projectile.frame = 1;
                    Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, -0.4f);
                }

                if (Timer >= 220)
                {
                    if (Timer < 450 + summonTime)
                    {
                        // Lock the player in
                        player.itemTime = 2;
                        player.itemAnimation = 2;
                        player.heldProj = projectile.whoAmI;
                        player.GetModPlayer<ErilipahPlayer>().canMove = false;

                        player.immune = true;
                        player.immuneTime = 30;
                    }
                }

                if (Timer > 530 + summonTime)
                {
                    projectile.velocity = Vector2.Zero;
                    projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X + player.direction * 17.5f, player.Center.Y), 0.1f);
                }
                else if (Timer > 450 + summonTime)
                {
                    projectile.velocity *= 0.92f;
                }
                else if (Timer == 450 + summonTime)
                {
                    if (Main.netMode != 1)
                        NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType<NPCs.Taranys.Taranys>());

                    NPC t = Main.npc[NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>())];
                    t.frame = new Rectangle(96, 0, 96, 102);
                    t.Center = projectile.Center;

                    projectile.velocity = new Vector2(0, -6);
                }
                else if (Timer >= 450 && Timer < 450 + summonTime)
                {
                    Vector2 newPos = new Vector2(player.Center.X, player.Center.Y - 200);
                    Vector2 distance = new Vector2(0, summonTime - (Timer - 450)) / 2f;
                    float rotation = (float)Math.Pow(Timer - 450, 1.815f);
                    rotation /= 100f;

                    projectile.Center = newPos + distance.RotatedBy(rotation);
                }
            }

            private void AlAltar(Player player)
            {
                
            }

            public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];
                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
                    texture.Frame(1, 2, 0, projectile.frame), Color.White * ((255 - projectile.alpha) / 255f), 0f,
                    Main.projectileTexture[projectile.type].Size() / 2, projectile.scale, 0, 1);
                return false;
            }

            private void DropItemInstanced(Rectangle r, int itemType, int itemStack = 1)
            {
                if (itemType <= 0)
                    return;
                if (Main.netMode == 2)
                {
                    int number = Item.NewItem(r, itemType, itemStack, true, 0, false, false);
                    Main.itemLockoutTime[number] = 54000;
                    for (int remoteClient = 0; remoteClient < 255; ++remoteClient)
                    {
                        if (Main.player[remoteClient].active)
                            NetMessage.SendData(90, remoteClient, -1, null, number, 0.0f, 0.0f, 0.0f, 0, 0, 0);
                    }
                    Main.item[number].active = false;
                }
                else if (Main.netMode == 0)
                    Item.NewItem(r, itemType, itemStack, false, 0, false, false);
            }
        }
    }
}
