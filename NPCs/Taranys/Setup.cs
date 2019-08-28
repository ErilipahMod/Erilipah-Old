using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Taranys
{
    //[AutoloadBossHead]
    public partial class Taranys : ModNPC
    {
        private const float dashSpeed = 13f;
        private const float dashChange = 0.275f;

        public new const string Name = "Taranys";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Name);
            Main.npcFrameCount[npc.type] = 1;
            NPCID.Sets.TrailCacheLength[npc.type] = 3;
            NPCID.Sets.TrailingMode[npc.type] = 0;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 6500;
            npc.defense = 14;
            npc.damage = 32;
            npc.knockBackResist = 0f;
            npc.SetInfecting(3f);

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.boss = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath2;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 96;
            npc.height = 102;

            npc.value = npc.AutoValue();

            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Taranys");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.65f * bossLifeScale);
            npc.defense = (int)(npc.defense * 1.35f);
            npc.damage = (int)(npc.damage * 0.65f);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
            if (Main.expertMode)
            {
                npc.DropBossBags();
            }
            else
            {
                Loot.DropItem(npc, mod.ItemType("SynthesizedLunaesia"), 16, 24, 100);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawTrail(spriteBatch, npc.oldPos.Length, drawColor);
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.75f);
            return false;
        }
    }
}
