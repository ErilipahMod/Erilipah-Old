﻿using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.Taranys;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Taranys
{
    [AutoloadBossHead]
    public partial class Taranys : ModNPC
    {
        private const float goIntoEatingPhase = 0.32f;
        private const float dashSpeed = 13f;
        private const float dashChange = 0.3f;

        public new const string Name = "Taranys";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Name);
            Main.npcFrameCount[npc.type] = 12;
            NPCID.Sets.TrailCacheLength[npc.type] = 3;
            NPCID.Sets.TrailingMode[npc.type] = 0;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 6500;
            npc.defense = 20;
            npc.damage = 25;
            npc.knockBackResist = 0f;
            npc.SetInfecting(4.5f);

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.boss = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit2;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 96;
            npc.height = 102;

            npc.value = npc.AutoValue();

            bossBag = mod.ItemType<TaranysBag>();
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Taranys");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.65f * bossLifeScale) + (numPlayers > 1 ? numPlayers * 1250 : 0);
            npc.defense = (int)(npc.defense * 1.5f);
            npc.damage = (int)(npc.damage * 0.65f);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            ErilipahWorld.downedTaintedSkull = true;
            potionType = ItemID.HealingPotion;

            Loot.DropItem(npc, mod.ItemType("PureFlower"), 3, 4, 100, 1.5f);
            if (Main.expertMode)
            {
                npc.DropBossBags();
            }
            else
            {
                Loot.DropItem(npc, mod.ItemType<ShellChunk>(), 6, 11);
                Loot.DropItem(npc, mod.ItemType<MadnessFocus>(), 9, 16);

                System.Collections.Generic.List<int> types = new System.Collections.Generic.List<int> {
                mod.ItemType<TyrantEye>(),
                mod.ItemType<VoidSpike>(),
                mod.ItemType<TorchOfSoul>(),
                mod.ItemType<ScepterOfEternalAbyss>(),
                mod.ItemType<LEECH>()
                };

                for (int i = 0; i < 2; i++)
                {
                    int chosen = Main.rand.Next(types);
                    Loot.DropItem(npc, chosen);
                    types.Remove(chosen);
                }
            }
        }

        public override bool CheckActive()
        {
            return !Main.player.Any(p => p.active && !p.dead && p.Distance(npc.Center) < 3000);
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }
        public override void BossHeadRotation(ref float rotation)
        {
            rotation = npc.rotation;
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
