using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Dracocide
{
    public abstract class DracocideDrone : ModNPC
    {
        protected int DeathCounter
        {
            get { return (int)npc.localAI[0]; }
            set { npc.localAI[0] = value; }
        }
        protected int TargetIndex
        {
            get { return (int)npc.ai[1]; }
            set { npc.ai[1] = value; }
        }
        protected bool PlayerTarget
        {
            get { return npc.ai[2] == 1; }
            set { npc.ai[2] = value ? 1 : 0; }
        }

        protected Entity Target
        {
            get
            {
                if (TargetIndex == -1)
                    return null;
                if (PlayerTarget)
                    return Main.player[TargetIndex];
                return Main.npc[TargetIndex];
            }
        }

        protected bool fuckingRun = false;

        // Copypasted from Observer
        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0 && DeathCounter == 0)
            {
                npc.life = 1;
                npc.dontTakeDamage = true;
                DeathCounter = 1;

                npc.velocity = new Vector2(hitDirection * 7.5f, -3);
                npc.noGravity = false;
                npc.netUpdate = true;
            }
        }

        // Scale expert
        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            float mult = 1;
            if (NPC.downedMechBossAny) mult += 0.2f;
            if (NPC.downedPlantBoss) mult += 0.35f;
            if (NPC.downedMoonlord) mult += 0.5f;

            npc.life = npc.lifeMax = (int)(npc.lifeMax * mult);
            npc.damage = (int)(npc.damage * mult);
            npc.defense = (int)(npc.defense * mult);
        }

        private int appear = 0;
        private bool lootDropped = false;
        protected bool Base()
        {
            // just copypasted from Observer
            if (DeathCounter > 0)
            {
                DeathCounter++;
                npc.noGravity = false;
                npc.netUpdate = true;
                npc.noTileCollide = false;
                npc.rotation += Helper.RadiansPerTick(2);

                npc.buffImmune[BuffID.OnFire] = false;
                npc.onFire = true;

                if (DeathCounter >= 80 || npc.collideY || npc.collideX || (npc.velocity.Y == 0 && DeathCounter > 30))
                {
                    npc.life = 0;
                    HitEffect(0, 0);
                    Main.PlaySound(npc.DeathSound, npc.Center);

                    if (!lootDropped && PreNPCLoot() & !SpecialNPCLoot())
                    {
                        lootDropped = true; // fix the double drop thing
                        NPCLoot();
                    }
                    npc.active = false;
                }
                return false;
            }
            if (fuckingRun || TargetIndex == -1 || Target == null || !Target.active || (Target is Player player && player.dead))
            {
                TargetIndex = -1;
                int motherIndex = npc.FindClosestNPC(1500, mod.NPCType<Observer>());
                if (motherIndex == -1 || npc.type != mod.NPCType<AssaultDrone>())
                {
                    npc.netUpdate = true;
                    npc.velocity.Y -= 0.1f;
                    return false;
                }

                npc.active = true;
                npc.dontTakeDamage = true;
                npc.spriteDirection = -1;
                Vector2 atMother = Main.npc[motherIndex].Center;

                if (Vector2.Distance(atMother, npc.Center) > 400)
                {
                    npc.rotation = npc.velocity.ToRotation();

                    npc.velocity = npc.Center.To(atMother, 3);
                    npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -5, Vector2.One * 5);
                }

                return false;
            }

            // Wait 90 ticks in between each drone's AI being run.
            npc.dontTakeDamage = true;
            var siblings = Main.npc.Where(x => x.active && x.modNPC != null && x.modNPC is DracocideDrone).ToList();
            if (++appear < siblings.IndexOf(npc) * 180)
                return false;

            npc.dontTakeDamageFromHostiles = false;
            npc.dontTakeDamage = false;
            npc.spriteDirection = -1;
            npc.alpha -= 10;
            return true;
        }
    }
}
