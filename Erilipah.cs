#undef HANDOUT_BETA
using Erilipah.Items.Dracocide;
using Erilipah.Items.Templar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Erilipah
{
    public partial class Erilipah : Mod
    {
        public Erilipah() { }

        public static ModHotKey VeritasAbilityKey;
        public static ModHotKey VitalityAbilityKey;
        public static ModHotKey Bandolier;
        public static ModHotKey SoulBank;

        public override void Load()
        {

#if HANDOUT_BETA
            ulong[] PROVIDED_IDs = { 76561198127351311U };
            ulong CURRENT_ID = Steamworks.SteamUser.GetSteamID().m_SteamID;
            if (PROVIDED_IDs.Contains(CURRENT_ID))
            {
                throw new Exception("Steam ID does not match the intended recipient ID.");
            }
#endif

            Bandolier = RegisterHotKey("Bondolier", "U");
            VeritasAbilityKey = RegisterHotKey("Veritas Ability", "V");
            VitalityAbilityKey = RegisterHotKey("Vitality Heal", "G");
            SoulBank = RegisterHotKey("Soul Bank", "Y");

            if (!Main.dedServ)
            {
                vitalityBar = new VitalityBar();
                vitalityBar.Activate();
                vitalityUI = new UserInterface();
                vitalityUI.SetState(vitalityBar);

                shieldBroken = new ShieldBroken();
                shieldBroken.Activate();
                shieldBrokenUI = new UserInterface();
                shieldBrokenUI.SetState(shieldBroken);

                infectionBar = new InfectionUI();
                infectionBar.Activate();
                infectUI = new UserInterface();
                infectUI.SetState(infectionBar);

                // Color params: x=number, y=inverse size, z=speed
                Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/ShockwaveEffect"));
                void RegisterShockwave(string name)
                {
                    Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, "Shockwave"), EffectPriority.VeryHigh);
                    Filters.Scene[name].Load();
                }

                RegisterShockwave("TaranysPulse");
                RegisterShockwave("TyrantEye1");
                RegisterShockwave("TyrantEye2");
                RegisterShockwave("Nidorose1");
                RegisterShockwave("Nidorose2");
                RegisterShockwave("AborycTake");

                SkyManager.Instance["Erilipah:ErilipahBiome"] = new Biomes.ErilipahBiome.ErilipahSky();
                Filters.Scene["Erilipah:ErilipahBiome"] = new Filter(new ScreenShaderData("FilterMoonLord").UseIntensity(0.55f), EffectPriority.Low);
            }
        }
        public override void Unload()
        {
            Bandolier = null;
            SoulBank = null;

            vitalityBar = null;
            vitalityUI = null;
            VitalityAbilityKey = null;

            shieldBrokenUI = null;
            shieldBroken = null;

            infectionBar = null;
            infectUI = null;
        }

        internal enum PacketType : byte { Infection }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            switch (reader.ReadByte())
            {
                case (byte)PacketType.Infection:
                    byte playernumber = reader.ReadByte();
                    InfectionPlr other = Main.player[playernumber].GetModPlayer<InfectionPlr>();
                    other.Infection = reader.ReadSingle();
                    other.infectionMax = reader.ReadSingle();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        var packet = GetPacket();
                        packet.Write((byte)PacketType.Infection);
                        packet.Write(playernumber);
                        packet.Write(other.Infection);
                        packet.Write(other.infectionMax);
                        packet.Send(-1, playernumber);
                    }
                    break;
            }
        }

        public static float totalBright = 1f;
        public static float erilipahBright = 1f;
        public static bool erilipahIsBright;
        public override void ModifyLightingBrightness(ref float scale)
        {
            if (Main.myPlayer < 0 || Main.gameMenu || !Main.LocalPlayer.active)
            {
                scale = 1f;
                return;
            }

            scale = totalBright;
            totalBright = 1;
        }
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (Main.myPlayer < 0 || Main.gameMenu || !Main.LocalPlayer.active)
                return;

            CustomSky erilipahSky = SkyManager.Instance["Erilipah:ErilipahBiome"];
            if (erilipahSky.IsActive())
            {
                float tile = MathHelper.Lerp(1f, erilipahIsBright ? 0.52f : 0.4f, erilipahSky.Opacity);
                float back = MathHelper.Lerp(1f, erilipahIsBright ? 0.25f : 0.22f, erilipahSky.Opacity);
                totalBright *= erilipahIsBright ? 1 : MathHelper.Lerp(1f, 0.8f, erilipahSky.Opacity);

                if (Main.raining && Main.LocalPlayer.Center.Y < Main.rockLayer * 16)
                {
                    tile /= 2f;
                    back /= 2f;
                    totalBright *= 0.8f;
                }

                tileColor = new Color(
                    (byte)(tileColor.R * tile * 0.85f),
                    (byte)(tileColor.G * tile * 0.7f),
                    (byte)(tileColor.B * tile * 1.0f), tileColor.A
                    );

                backgroundColor = new Color(
                    (byte)(backgroundColor.R * back * 0.8f),
                    (byte)(backgroundColor.G * back * 0.7f),
                    (byte)(backgroundColor.B * back * 1.0f), backgroundColor.A
                    );
            }
        }

        public static bool noMusic = false;
        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer < 0 || Main.gameMenu || !Main.LocalPlayer.active)
                return;

            if (noMusic)
            {
                music = -1;
                return;
            }

            if (Main.LocalPlayer.InErilipah())
            {
                music = GetSoundSlot(SoundType.Music, "Sounds/Music/Erilipah");
                priority = MusicPriority.BiomeHigh;
            }

            if (Main.LocalPlayer.InLostCity())
            {
                music = GetSoundSlot(SoundType.Music, "Sounds/Music/LostCity");
                priority = MusicPriority.BiomeHigh;
            }
        }

        public override void PostSetupContent()
        {
            Mod bossChecklist = ModLoader.GetMod("BossChecklist");
            if (bossChecklist != null)
            {
                void AddBoss(string name, float difficulty, bool downed, string description)
                {
                    bossChecklist.Call("AddBossWithInfo", name, difficulty, (Func<bool>)(() => downed), description);
                }

                AddBoss("Lunaemia", 2.45f, ErilipahWorld.downedLunaemia, "Use a [i:" + ItemType("ModelMoon") + "] at night");
                AddBoss("Taranys", 5.9999f, ErilipahWorld.downedTaintedSkull, "Use and hold [i:" + ItemType("ModelMoon") + "] in darkness in Erilipah");
            }
        }

        //SlimeKing = 1f;
        //EyeOfCthulhu = 2f;
        //EaterOfWorlds = 3f;
        //QueenBee = 4f;
        //Skeletron = 5f;
        //WallOfFlesh = 6f;
        //TheTwins = 7f;
        //TheDestroyer = 8f;
        //SkeletronPrime = 9f;
        //Plantera = 10f;
        //Golem = 11f;
        //DukeFishron = 12f;
        //LunaticCultist = 13f;
        //Moonlord = 14f;
    }
}