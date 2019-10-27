using Erilipah.Biomes.ErilipahBiome;
using Erilipah.Items.Dracocide;
using Erilipah.Items.Templar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
        public Erilipah()
        {
        }

        #region Load & Unload
        public static ModHotKey VeritasAbilityKey;
        public static ModHotKey VitalityAbilityKey;
        public static ModHotKey Bandolier;
        public static ModHotKey SoulBank;

        public override void Load()
        {
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

            On.Terraria.Main.DrawBuffIcon += DrawBuffIconDust;
        }

        public override void Unload()
        {
            VeritasAbilityKey = null;
            VitalityAbilityKey = null;
            Bandolier = null;
            SoulBank = null;

            vitalityUI = null;
            shieldBrokenUI = null;
            infectUI = null;

            vitalityBar = null;
            shieldBroken = null;
            infectionBar = null;

            handlers?.Clear();
            handlers = null;

            StageIIDusts?.Clear();
            StageIIDusts = null;

            On.Terraria.Main.DrawBuffIcon -= DrawBuffIconDust;
        }
        #endregion

        private static List<StageIIDust> StageIIDusts = new List<StageIIDust>();
        private int DrawBuffIconDust(On.Terraria.Main.orig_DrawBuffIcon orig, int drawBuffText, int i, int b, int x, int y)
        {
            drawBuffText = orig(drawBuffText, i, b, x, y);

            // i = index of buff; b = buff type
            if (b == ModContent.BuffType<StageII>())
            {
                if (Main.rand.NextBool(2))
                {
                    float scale = Main.rand.NextFloat(0.7f, 0.9f);
                    Vector2 pos = new Vector2(Main.rand.Next(x + 2, x + 28), y - 2);
                    Vector2 vel = new Vector2(0, -0.7f * scale);
                    Vector2 accel = new Vector2(0, -0.07f * scale);

                    StageIIDusts.Add(new StageIIDust(pos, vel, accel, scale));
                }
            }

            StageIIDusts.ForEach(d => d.Update());
            StageIIDusts.RemoveAll(d => !d.Active);
            StageIIDusts.ForEach(d => d.Draw(Main.spriteBatch));
            return drawBuffText;
        }

        #region Erilipah brightness & stuff
        public static float totalBright = 1f;
        public static float erilipahBright = 1f;
        public static bool erilipahIsBright = false;
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
        #endregion

        public static bool noMusic = false;
        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer < 0 || Main.gameMenu)
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
                /* SlimeKing = 1f;
                 * EyeOfCthulhu = 2f;
                 * EaterOfWorlds = 3f;
                 * QueenBee = 4f;
                 * Skeletron = 5f;
                 * WallOfFlesh = 6f;
                 * TheTwins = 7f;
                 * TheDestroyer = 8f;
                 * SkeletronPrime = 9f;
                 * Plantera = 10f;
                 * Golem = 11f;
                 * DukeFishron = 12f;
                 * LunaticCultist = 13f;
                 * Moonlord = 14f;
                */

                void AddBoss(string name, float difficulty, bool downed, string description)
                {
                    bossChecklist.Call("AddBossWithInfo", name, difficulty, (Func<bool>)(() => downed), description);
                }

                AddBoss("Lunaemia", 2.45f, ErilipahWorld.downedLunaemia, "Use a [i:" + ItemType("ModelMoon") + "] at night");
                AddBoss("Taranys", 5.9999f, ErilipahWorld.downedTaintedSkull, "Use and hold [i:" + ItemType("ModelMoon") + "] in darkness in Erilipah");
            }
        }

        #region Packet handling
        private static List<PacketHandler> handlers = new List<PacketHandler>();
        public static void AddPacketHandler(PacketHandler handler, out int id)
        {
            handlers.Add(handler);
            id = handlers.IndexOf(handler);
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // HANDLE THIS STUFF
            int id = reader.ReadInt32();
            handlers[id].HandlePacket(reader, whoAmI);
        }
        #endregion
    }

    internal class StageIIDust : OverlayParticle
    {
        public StageIIDust(Vector2 pos, Vector2 vel, Vector2 accel, float scale) : base(ModContent.GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Gas"))
        {
            position = pos;
            velocity = vel;
            acceleration = accel;
            this.scale = scale;

            rotation = Main.rand.NextFloat(6.24f);
            color = Color.Lerp(Color.White, Color.Pink, Main.rand.NextFloat());
        }

        public override void Update()
        {
            base.Update();

            scale -= 0.018f;
            color.A -= 6;
            if (color.A <= 5)
                Active = false;
        }
    }

    internal abstract class OverlayParticle
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
        public Color color;
        public Texture2D texture;
        public float rotation;
        public float scale = 1f;

        public bool Active { get; protected set; } = true;

        protected OverlayParticle(Texture2D texture)
        {
            this.texture = texture;
        }

        protected OverlayParticle(Vector2 position, Vector2 velocity, Vector2 acceleration, Color color, Texture2D texture, float rotation, float scale)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.color = color;
            this.texture = texture;
            this.rotation = rotation;
            this.scale = scale;
        }

        public virtual void Update()
        {
            position += velocity += acceleration;
        }

        public void Draw(SpriteBatch sb)
        {
            Vector2 txcnt = texture.Size() / 2;
            sb.Draw(texture, position + txcnt, null, color, rotation, txcnt, scale, 0, 0);
        }
    }
}