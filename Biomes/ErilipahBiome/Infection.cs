using Erilipah.Items.ErilipahBiome.Potions;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Erilipah.Biomes.ErilipahBiome
{
    public class InfectionUI : UIState
    {
        public override void OnInitialize()
        {
            SetPadding(0);
            Left.Set(Main.screenWidth - 450, 0f);
            Top.Set(32, 0f);
            Width.Set(130, 0);
            Height.Set(28, 0);
        }

        float alpha = 60f;
        bool ActiveOther(Player p) => p.active && !p.dead && p.I().Infection > 0;
        bool ActiveBar() => Main.LocalPlayer.active && Main.LocalPlayer.I().Infection > 0;
        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw others' percentages
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (i == Main.myPlayer)
                    continue;

                var player = Main.player[i];
                if (ActiveOther(player))
                {
                    float infection = player.I().Infection;
                    float infectionMax = player.I().infectionMax;
                    string peepeepoopoo = Math.Round(infection * 100 / infectionMax, 1).ToString() + "%";
                    Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                    Vector2 center = player.Center - new Vector2(0, 40);

                    Utils.DrawBorderString(spriteBatch, peepeepoopoo,
                        center - origin - Main.screenPosition, Color.MediumVioletRed,
                        infection > infectionMax ? infection / infectionMax : 1f); //, SpriteEffects.None, 0);
                }
            }
            if (ActiveBar())
            {
                alpha--;
            }
            else
            {
                alpha++;
            }
            alpha = MathHelper.Clamp(alpha, 0, 60);

            if (alpha < 60)
            {
                float infection = Math.Max(0, Main.LocalPlayer.I().Infection);
                float infectionMax = Main.LocalPlayer.I().infectionMax;

                Texture2D texture2D = ModContent.GetTexture("Erilipah/Biomes/ErilipahBiome/Infection");
                float amount = Math.Min(1, Math.Max(0, infection) / infectionMax);
                int frameY = (int)Math.Round(MathHelper.Lerp(0, 20, amount));
                Rectangle frame = texture2D.Frame(1, 21, 0, frameY);
                Vector2 drawCenter = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);
                Color color = Color.Lerp(Color.White, Color.White * 0, alpha / 60f);

                spriteBatch.Draw(texture2D, drawCenter + new Vector2(0, 9), frame, color, 0f, frame.Size() / 2f, 1f, 0, 0);
                if (IsMouseHovering)
                {
                    string peepeepoopoo = Math.Round(infection, 1).ToString() + "/" + infectionMax.ToString();
                    Vector2 center = Main.MouseScreen + new Vector2(15, 15);
                    Utils.DrawBorderString(spriteBatch, peepeepoopoo, center, Main.mouseTextColorReal);
                    //spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal);
                }
                {
                    string peepeepoopoo = "Infection: " + Math.Floor(infection * 100 / infectionMax).ToString() + "%";
                    Vector2 origin = Main.fontMouseText.MeasureString(peepeepoopoo) / 2f;
                    Vector2 center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels - 3);

                    spriteBatch.DrawString(Main.fontMouseText, peepeepoopoo, center, Main.mouseTextColorReal * ((60 - alpha) / 60f), 0f, origin,
                        1f, SpriteEffects.None, 0);
                }
            }
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
                infection = MathHelper.Clamp(value, -5, infectionMax * 1.25f);
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
                CombatText.NewText(loc, Color.CornflowerBlue, Math.Round(-infection, 1).ToString(), infection > infectionMax / 3f);
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
                int dustInd = Dust.NewDust(player.position, player.width, player.height, mod.DustType<VoidParticle>(), 0, 0);
                Main.playerDrawDust.Add(dustInd);

                Dust dust = Main.dust[dustInd];
                dust.noGravity = false;
                dust.velocity = Vector2.Zero;
                dust.customData = 0.05f;
            }
        }

        private int darknessCounter = 0;
        private int counter = 0;
        public override void PreUpdate()
        {
            counter++;

            if (!player.HasBuff(mod.BuffType<EffulgencePot.EffulgencePotBuff>()))
            {
                Erilipah.erilipahIsBright = false;
            }
            if (player.InErilipah())
            {
                player.AddBuff(mod.BuffType<Watched>(), 2);
            }

            for (int i = 0; i < 6000; i++)
            {
                Main.dust[i].noLight = true;
            }

            if (Infection > infectionMax)
            {
                player.lifeRegenTime = 0;
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;

                if (counter % (int)Math.Max(30 / (Infection - infectionMax), 1) == 0)
                    player.statLife--;

                if (counter % 30 == 0)
                    CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, (int)Math.Ceiling(Infection - infectionMax), true, true);

                if (player.statLife <= 0)
                {
                    string biome = player.ZoneCorrupt || player.ZoneCrimson ? "evil" : "decay";
                    player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " was infested with " + biome + "."), 30, 0);
                }
            }

            Rectangle loc = new Rectangle(player.getRect().X / 16, player.getRect().Y / 16, 2, 3);
            float playerBrightness = Lighting.BrightnessAverage(loc.X, loc.Y, loc.Width, loc.Height);
            if (!player.InErilipah())
                return;

            if (playerBrightness < 0.1f)
            {
                darknessCounter++;
                if (darknessCounter == 220)
                {
                    Main.PlaySound(15, (int)player.Center.X, (int)player.Center.Y, 0, 1f, Main.rand.NextFloat(-0.825f, -0.715f));
                }
                if (darknessCounter >= 400 && darknessCounter % 120 == 0)
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason("Darkness overtook " + player.name + "."), player.statLifeMax2 / 5 + Main.rand.Next(15), Main.rand.Next(-1, 2));
                }
            }
            else
            {
                if (darknessCounter > 400) darknessCounter = 400;
                darknessCounter -= 3;
            }

            if (darknessCounter < 0)
                darknessCounter = 0;
        }
        public override void PostUpdate()
        {
            const float infectionInErilipah = 10f / 3600f;
            if (player.InErilipah())
            {
                infectionRate += infectionInErilipah * reductionRate;
            }
            else if (player.ZoneCorrupt)
            {
                infectionRate += infectionInErilipah * 0.50f * reductionRate;
            }
            else if (player.ZoneCrimson)
            {
                infectionRate += infectionInErilipah * 0.75f * reductionRate;
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
                infectionRate += infectionInErilipah * 0.75f;
            }
            if (Main.expertMode)
            {
                infectionRate += infectionInErilipah * 0.25f;
            }

            Infection += infectionRate;

            if (Main.LocalPlayer == player)
            {
                if (Infection > infectionMax * 0.8f)
                    player.AddBuff(BuffID.Weak, 1);
                if (Infection > infectionMax * 0.9f)
                    player.AddBuff(BuffID.Slow, 1);
            }
        }

        private float added = 0f;
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            if (player.InErilipah() && damage > 1)
            {
                Infect((float)damage / 17.5f + added);
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

        // Infection packet manager: <type, player, infection, infection max>
        public override void clientClone(ModPlayer clientClone)
        {
            InfectionPlr clone = clientClone as InfectionPlr;
            clone.Infection = Infection;
            clone.infectionMax = infectionMax;
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)Erilipah.PacketType.Infection);
            packet.Write((byte)player.whoAmI);
            packet.Write(Infection);
            packet.Write(infectionMax);
            packet.Send();
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            InfectionPlr clone = clientPlayer as InfectionPlr;
            if (clone.Infection != Infection || clone.infectionMax != infectionMax)
            {
                var packet = mod.GetPacket();
                packet.Write((byte)Erilipah.PacketType.Infection);
                packet.Write((byte)player.whoAmI);
                packet.Write(Infection);
                packet.Write(infectionMax);
                packet.Send();
            }
        }
    }
}
