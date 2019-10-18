using Erilipah.Biomes.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Erilipah.NPCs.ErilipahBiome;
using Erilipah.NPCs.Taranys;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah
{
    public class InfectionUI : UIState
    {
        public override void OnInitialize()
        {
            SetPadding(0);
            Width.Set(130, 0);
            Height.Set(28, 0);
            Left.Set(Main.screenWidth - 450, 0.6614f);
            Top.Set(32, 0.04539f);
        }

        private float alpha = 60f;
        private int counter = 0;
        private int scaleCounter = 0;
        private int frameY = 0;
        private int frameX = 0;

        private readonly List<InfectionBarDust> infectedDusts = new List<InfectionBarDust>();


        // 0 = Erilipah
        // 1 = Corrupt
        // 2 = Crimson
        private int GetBiome(Player player)
        {
            if (player.InErilipah())
                return 0;
            if (player.ZoneCorrupt)
                return 1;
            if (player.ZoneCrimson)
                return 2;
            return frameX;
        }

        private bool ActiveOther(Player p) => p.active && !p.dead && p.I().Infection > 0;
        private bool ActiveBar() => Main.LocalPlayer.active && Main.LocalPlayer.I().Infection > 0;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                DrawOthers(spriteBatch);
            }

            if (!ActiveBar() || frameX != GetBiome(Main.LocalPlayer))
            {
                alpha++;
            }
            else
            {
                alpha--;
            }

            alpha = MathHelper.Clamp(alpha, 0, 90);

            if (alpha < 60)
            {
                Vector2 drawPos = new Vector2(Left.Percent * Main.screenWidth / Main.UIScale, Top.Percent * Main.screenHeight);
                DrawBar(spriteBatch, drawPos);
            }
            else if (alpha > 70)
            {
                frameX = GetBiome(Main.LocalPlayer);
            }
        }

        private void DrawBar(SpriteBatch spriteBatch, Vector2 position)
        {
            float infection = Math.Max(0, Main.LocalPlayer.I().Infection);
            float infectionMax = Main.LocalPlayer.I().infectionMax;
            float amount = Math.Max(0, infection);

            Texture2D texture2D = GetTexture("Erilipah/Biomes/ErilipahBiome/Infection");
            Color color = Color.Lerp(Color.White, Color.White * 0, alpha / 60f);

            counter++;

            if (amount > infectionMax)
            {
                // FrameY animation
                float speedMult = infectionMax / infection / 2;
                int countModulo = (int)(10 * speedMult);
                if (counter % countModulo == 0)
                {
                    frameY++;
                    if (frameY > 22)
                    {
                        frameY = 20;
                    }
                }

                SpewDust(position + new Vector2(Main.rand.NextFloat(115, 120), Main.rand.NextFloat(8, 16)));
            }
            else
            {
                // FrameY animation
                int max = (int)MathHelper.Lerp(0, 20, amount / infectionMax);
                if (counter % 10 == 0)
                {
                    frameY++;
                    if (frameY > max)
                    {
                        frameY = max;
                    }
                }

                // Scale
                if (scaleCounter > 0)
                    scaleCounter--;
                if (scaleCounter < 0)
                    scaleCounter++;
            }

            Rectangle frame = texture2D.Frame(3, 23, frameX, frameY);
            spriteBatch.Draw(texture2D, new Rectangle((int)position.X, (int)position.Y, (int)Width.Pixels, (int)Height.Pixels), frame, color);

            Rectangle bar = new Rectangle((int)position.X, (int)position.Y, 130, 28);
            bool isHovering = bar.Contains(Main.MouseScreen.ToPoint());
            if (isHovering)
            {
                string peepeepoopoo = Math.Round(infection, 1).ToString() + "/" + infectionMax.ToString();
                Vector2 center = Main.MouseScreen + new Vector2(15, 15);

                Main.LocalPlayer.showItemIconText = peepeepoopoo;
                Utils.DrawBorderString(spriteBatch, peepeepoopoo, center, Main.mouseTextColorReal);
                //spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal);
            }
            {
                string peepeepoopoo = "Infection: " + Math.Floor(infection * 100 / infectionMax).ToString() + "%";
                Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                Vector2 center = new Vector2(position.X + Width.Pixels / 2, position.Y - 12);

                spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal * ((60 - alpha) / 60f), 0f, origin,
                    1f, SpriteEffects.None, 0);
            }

            if (Main.rand.NextBool(8))
                DripDust(position + new Vector2(20, 12) + Main.rand.NextVector2Circular(12, 10));
            DrawDusts(spriteBatch);
        }

        private void DripDust(Vector2 position)
        {
            float scale = Main.rand.NextFloat(0.75f, 0.9f) * ((60 - alpha) / 60);
            Vector2 vel = new Vector2(0, 0.5f);
            Vector2 accel = new Vector2(0, 0.08f * scale);

            infectedDusts.Add(new InfectionBarDust(position, vel, accel, scale, frameX));
        }

        private void SpewDust(Vector2 position)
        {
            float scale = Main.rand.NextFloat(0.9f, 1.1f) * ((60 - alpha) / 60);
            Vector2 vel = new Vector2(2.5f, 0);
            Vector2 accel = new Vector2(-0.06f, -0.08f * scale);

            infectedDusts.Add(new InfectionBarDust(position, vel, accel, scale, frameX));
        }

        private void DrawDusts(SpriteBatch spriteBatch)
        {
            infectedDusts.ForEach(d => d.Update());
            infectedDusts.RemoveAll(d => !d.Active);
            infectedDusts.ForEach(d => d.Draw(spriteBatch));
        }

        private void DrawOthers(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (i == Main.myPlayer)
                    continue;

                Player player = Main.player[i];
                if (ActiveOther(player))
                {
                    float infection = player.I().Infection;
                    float infectionMax = player.I().infectionMax;
                    string peepeepoopoo = Math.Ceiling(infection / infectionMax * 100).ToString() + "%";
                    Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                    Vector2 center = player.Center - new Vector2(0, 40);

                    Utils.DrawBorderString(spriteBatch, peepeepoopoo,
                        center - origin - Main.screenPosition, Color.MediumVioletRed,
                        infection > infectionMax ? infection / infectionMax : 1f); //, SpriteEffects.None, 0);
                }
            }
        }
    }

    internal class InfectionBarDust : OverlayParticle
    {
        public InfectionBarDust(Vector2 pos, Vector2 vel, Vector2 accel, float scale, int biomeType) : base(GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Gas"))
        {
            position = pos;
            velocity = vel;
            acceleration = accel;
            this.scale = scale;

            switch (biomeType)
            {
                default:
                    color = Color.Lerp(Color.White, Color.Pink, Main.rand.NextFloat());
                    break;
                case 1:
                    color = Color.Lerp(Color.White, Color.Lime, Main.rand.NextFloat());
                    break;
                case 2:
                    color = Color.Lerp(Color.White, Color.Crimson, Main.rand.NextFloat());
                    break;
            }

            rotation = Main.rand.NextFloat(6.24f);
        }

        public override void Update()
        {
            base.Update();

            scale -= 0.018f;
            color.A -= 7;
            if (color.A <= 5)
                Active = false;
        }
    }

    public class InfectionPlr : ModPlayer
    {
        private float infection = 0;
        private float infectionRate = 0f;
        public float infectionMax = 100;
        public float Infection
        {
            get => infection;
            set
            {
                infection = MathHelper.Clamp(value, -5, infectionMax * 1.255f);
            }
        }

        public float reductionRate = 1f;
        public float reductionDmg = 1f;

        public const float infectionInErilipah = 15f / 3600f;

        public void Infect(float infection)
        {
            if (Main.expertMode && infection > 0)
                infection *= 1.5f;
            infection *= reductionDmg;
            infection = Main.rand.NextFloat(infection * 0.85f, infection * 1.15f); // Randomize by 15%

            Infection += infection;

            Rectangle loc = player.getRect();
            loc.Y -= 30;
            if (infection > 0)
                CombatText.NewText(loc, Color.PaleVioletRed, Math.Round(infection, 1).ToString(), infection > infectionMax / 3f);
            if (infection < 0)
                CombatText.NewText(loc, Color.AliceBlue, Math.Round(-infection, 1).ToString(), infection > infectionMax / 3f);
        }
        public void InfectPerSecond(float perSecond)
        {
            infectionRate = 100f / perSecond / 60f;
        }

        public override void ResetEffects()
        {
            infectionRate = 0f;
            reductionRate = 1f;
            reductionDmg = 1f;
            added = 0f;
        }
        public override void UpdateDead()
        {
            Infection = 0;
            darknessCounter = 0;
        }
        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (player.InErilipah())
            {
                r /= 2f;
                g /= 2f;
                b /= 2f;
                fullBright = false;
            }

            if (Infection < 10)
                return;

            float proportion = Infection / infectionMax;
            r = MathHelper.Lerp(r, 0.45f, proportion);
            g = MathHelper.Lerp(g, 0.4f, proportion);
            b = MathHelper.Lerp(b, 0.5f, proportion);

            if (Main.rand.Chance(proportion / 4))
            {
                int type = player.InErilipah() ? DustType<VoidParticle>() : (player.ZoneCrimson ? DustID.Blood : DustID.GreenBlood);
                if (!player.InErilipah() && !player.ZoneCrimson && !player.ZoneCrimson)
                    type = DustType<VoidParticle>();
                int dustInd = Dust.NewDust(player.position, player.width, player.height, type, 0, 0);
                Main.playerDrawDust.Add(dustInd);

                Dust dust = Main.dust[dustInd];
                dust.noGravity = false;
                dust.velocity = Vector2.Zero;
                dust.customData = 0.05f;
            }
        }

        public int darknessCounter = 0;
        private int counter = 0;
        public override void PreUpdate()
        {
            counter++;

            if (!player.HasBuff(BuffType<EffulgencePot.EffulgencePotBuff>()))
            {
                Erilipah.erilipahIsBright = false;
            }

            if (Infection > infectionMax)
            {
                InfectionHurt();
            }

            if (player.InErilipah())
            {
                player.AddBuff(BuffType<Watched>(), 2);
                Darkness();
            }
        }

        private void Darkness()
        {
            float playerBrightness = player.Brightness();

            if (!NPC.AnyNPCs(NPCType<Taranys>()) && playerBrightness <= 0.1f)
            {
                darknessCounter++;
                if (darknessCounter == 360)
                {
                    Main.PlaySound(15, (int)player.Center.X + Main.rand.Next(-50, 50), (int)player.Center.Y, 0, 1f, Main.rand.NextFloat(-0.815f, -0.7f));
                }
                if (darknessCounter >= 500 && darknessCounter % 150 == 0)
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason("Darkness overtook " + player.name + "."),
                        player.statLifeMax2 / 4 + (player.statDefense / 2) + Main.rand.Next(15), Main.rand.Next(-1, 2));
                }
            }
            else
            {
                if (darknessCounter > 280)
                    darknessCounter = 280;
                darknessCounter -= 2;
            }

            if (darknessCounter < 0)
                darknessCounter = 0;
        }

        private void InfectionHurt()
        {
            player.lifeRegenTime = 0;
            if (player.lifeRegen > 0)
                player.lifeRegen = 0;

            if (counter % (int)Math.Max(30 / (Infection - infectionMax), 1) == 0)
            {
                player.netLife = true;
                player.statLife--;
            }

            if (counter % 30 == 0 && Infection - infectionMax >= 1)
                CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, (int)Math.Floor(Infection - infectionMax), true, true);
            else if (counter % 30 == 0)
                CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, 1, true, true);

            if (player.statLife <= 0)
            {
                string biome = player.ZoneCorrupt || player.ZoneCrimson ? "evil" : "darkness";
                player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " was infested with " + biome + "."), 30, 0);
            }
        }

        public override void PostUpdate()
        {
            InfectionDebuffs();
            InfectionRate();
        }

        private void InfectionDebuffs()
        {
            if (Infection > infectionMax * 1.0f)
            {
                player.AddBuff(BuffType<StageII>(), 2);
            }
            else if (Infection > infectionMax * 0.85f)
            {
                player.AddBuff(BuffType<StageI>(), 2);
            }
        }

        private void InfectionRate()
        {
            const float infectionInErilipah = 8f / 3600f;
            if (player.InErilipah())
            {
                infectionRate += infectionInErilipah * reductionRate;
            }
            else if (player.ZoneCorrupt)
            {
                infectionRate += infectionInErilipah * 0.30f * reductionRate;
            }
            else if (player.ZoneCrimson)
            {
                infectionRate += infectionInErilipah * 0.50f * reductionRate;
            }
            else if (player.ZoneHoly)
            {
                infectionRate -= infectionInErilipah * 2f / (reductionRate < 0.1f && reductionRate >= 0 ? 0.1f : reductionRate); // Prevent a DB0 error
            }
            else
            {
                infectionRate -= infectionInErilipah * 1.25f / (reductionRate < 0.1f && reductionRate >= 0 ? 0.1f : reductionRate); // Prevent a DB0 error
            }

            if (player.wet)
            {
                infectionRate += infectionInErilipah;
            }
            if (Main.expertMode)
            {
                infectionRate += infectionInErilipah * 0.25f;
            }

            Infection += infectionRate;
        }

        private float added = 0f;
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            ApplyInfection(damage);
        }

        private void ApplyInfection(double damage)
        {
            if (player.InErilipah() && (damage > 1 || added > 0))
            {
                Infect((float)damage / 20f + added);
            }
        }

        public class InfectionNPC : GlobalNPC
        {
            public override bool InstancePerEntity => true;
            public float infecting = 0f;

            public override void SetDefaults(NPC npc)
            {
                if (npc.type == NPCID.EaterofWorldsBody)
                    npc.SetInfecting(1.8f);
                if (npc.type == NPCID.EaterofSouls || npc.type == NPCID.Corruptor)
                    npc.SetInfecting(0.9f);

                if (npc.type == NPCID.Creeper || npc.type == NPCID.BrainofCthulhu)
                    npc.SetInfecting(1.1f);
                if (npc.type == NPCID.FaceMonster || npc.type == NPCID.Crimera || npc.type == NPCID.LittleCrimera || npc.type == NPCID.BigCrimera)
                    npc.SetInfecting(0.5f);
            }
            public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
            {
                if (target.InErilipah() && infecting > 0)
                {
                    target.I().added = infecting;
                }
                else if (infecting > 0)
                {
                    target.I().Infect(infecting);
                }
            }
        }
        public class InfectionProj : GlobalProjectile
        {
            public override bool InstancePerEntity => true;
            public float infecting = 0f;

            public override void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
            {
                if (target.InErilipah() && infecting > 0)
                {
                    target.I().added = infecting;
                }
                else if (infecting > 0)
                {
                    target.I().Infect(infecting);
                }
            }
        }

        // Saving
        public override TagCompound Save() => new TagCompound()
        {
            ["infection"] = Infection,
            ["infectionMax"] = infectionMax
        };
        public override void Load(TagCompound tag)
        {
            Infection = tag.GetFloat("infection");
            infectionMax = tag.GetFloat("infectionMax");
        }

        public override void clientClone(ModPlayer clientClone)
        {
            InfectionPlr clone = clientClone as InfectionPlr;
            clone.darknessCounter = darknessCounter;
            clone.Infection = Infection;
            clone.infectionMax = infectionMax;
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            InfectionPacket.SendPacket(player.whoAmI, Infection, infectionMax);
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            InfectionPlr clone = clientPlayer as InfectionPlr;
            if (clone.Infection != Infection || clone.infectionMax != infectionMax)
            {
                InfectionPacket.SendPacket(player.whoAmI, Infection, infectionMax);
            }
        }

        private static readonly InfectionHandler InfectionPacket = new InfectionHandler();
        private class InfectionHandler : PacketHandler
        {
            protected override void WritePacket(ModPacket packet, params object[] info)
            {
                packet.Write((int)info[0]);
                packet.Write((float)info[1]);
                packet.Write((float)info[2]);
            }

            public override void HandlePacket(BinaryReader reader, int whoAmI)
            {
                Player player = Main.player[reader.ReadInt32()];
                player.I().Infection = reader.ReadSingle();
                player.I().infectionMax = reader.ReadSingle();
            }
        }
    }
}
