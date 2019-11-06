using Erilipah.Biomes.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Erilipah.NPCs.ErilipahBiome;
using Erilipah.NPCs.Taranys;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace Erilipah
{
    public class InfectionPlr : ModPlayer
    {
        private float infection = 0;
        private float infectionRate = 0f;
        public float infectionMax = 100;
        public float Infection
        {
            get => infection;
            set => infection = MathHelper.Clamp(value, -5, infectionMax * 1.25f);
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
                int type = player.InErilipah() ? DustType<VoidParticle>() : player.ZoneCrimson ? DustID.Blood : DustID.GreenBlood;
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
                        player.statLifeMax2 / 4 + player.statDefense / 2 + Main.rand.Next(15), Main.rand.Next(-1, 2));
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
