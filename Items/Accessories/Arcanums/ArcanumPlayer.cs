using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class ArcanumPlayer : ModPlayer
    {
        private int monumentis = 0;
        private int MonumentisMax
        {
            get
            {
                bool equip = ModItemEquipped("Subterrania") || ModItemEquipped("Veritas");
                if (equip && (!Main.dayTime || player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight))
                    return 60 * 240;
                if (equip)
                    return 60 * 480;

                return 60 * 360;
            }
        }
        private bool ModItemEquipped(string s) => player.armor.Take(10).Any(x => x.type == mod.ItemType(s));
        private bool MonumentisActive => monumentis >= MonumentisMax;

        private bool veritas = false;
        private int veritasCooldown = -1;

        public override void UpdateDead()
        {
            monumentis = 0;
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (veritasCooldown <= 0 && ModItemEquipped("Veritas") && Erilipah.VeritasAbilityKey.JustPressed)
            {
                CombatText.NewText(player.getRect(), Color.Aquamarine, "\"Veritas!\"", true);
                veritas = !veritas;
                veritasCooldown = 1;
                Main.PlaySound(SoundID.Item29, player.Center);
            }
        }

        public override void PostUpdate()
        {
            int reviveBuffType = mod.BuffType<ReviveCooldown>();
            int reviveBuffIndex = player.FindBuffIndex(reviveBuffType);
            if (ModItemEquipped("Monumentis") || ModItemEquipped("Subterrania") || ModItemEquipped("Veritas"))
            {
                monumentis++;

                if (monumentis < MonumentisMax)
                {
                    player.AddBuff(reviveBuffType, MonumentisMax - monumentis);
                    if (reviveBuffIndex >= 0)
                        player.buffTime[reviveBuffIndex] = MonumentisMax - monumentis;
                }
                else if (reviveBuffIndex >= 0)
                    player.DelBuff(reviveBuffIndex);
            }
            else if (MonumentisActive && reviveBuffIndex >= 0)
            {
                player.DelBuff(reviveBuffIndex);
            }
            else if (reviveBuffIndex >= 0)
            {
                player.DelBuff(reviveBuffIndex);
            }

            if (veritas && ModItemEquipped("Veritas"))
            {
                player.AddBuff(mod.BuffType<VeritasActive>(), 2);

                player.allDamage += 0.20f;
                player.statLifeMax2 = (int)(player.statLifeMax * 0.80f);
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;
            }
            if (veritasCooldown > 0)
            {
                veritasCooldown++;
                if (veritasCooldown > 300)
                {
                    veritasCooldown = -1;
                }
            }
        }

        public void SavedText() => CombatText.NewText(player.getRect(), CombatText.HealLife, "Saved", true);
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
#if DEBUG
            if (player.HasItem(mod.ItemType("ErilipahsUltimatum")))
            {
                playSound = false;
                genGore = false;

                player.statLife = player.statLifeMax2 - ((int)damage - player.statLife);

                player.immune = true;
                player.immuneTime = 30;
                for (int i = 0; i < player.hurtCooldowns.Length; i++)
                {
                    player.hurtCooldowns[i] = 30;
                }

                Main.PlaySound(SoundID.Item4, player.Center);
                SavedText();
                return false;
            }
#endif
            if (MonumentisActive && (ModItemEquipped("Monumentis") || ModItemEquipped("Subterrania") || ModItemEquipped("Veritas")))
            {
                int healLife = 250;
                int buffTime = 3600;

                if (ModItemEquipped("Subterrania") || ModItemEquipped("Veritas"))
                {
                    healLife = 180;
                }
                if ((ModItemEquipped("Subterrania") || ModItemEquipped("Veritas")) && player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
                {
                    healLife = 300;
                    buffTime = 3600 / 2;
                }

                playSound = false;
                genGore = false;

                int heal = Math.Min(healLife, player.statLifeMax2 - player.statLife);
                player.statLife = heal;

                player.immune = true;
                player.immuneTime = 180;
                for (int i = 0; i < player.hurtCooldowns.Length; i++)
                {
                    player.hurtCooldowns[i] = 180;
                }

                Main.PlaySound(SoundID.Item29, player.Center);
                SavedText();

                player.AddBuff(BuffID.BrokenArmor, buffTime);
                player.AddBuff(BuffID.PotionSickness, buffTime);

                monumentis = 0;
                return false;
            }
            return true;
        }
    }

    public class ReviveCooldown : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Saved - Arcanums");
            Description.SetDefault("Arcanums can no longer save you");

            Main.buffNoSave[Type] = false;
            Main.persistentBuff[Type] = true;
            Main.debuff[Type] = true;
            canBeCleared = false;
        }
        public override void ModifyBuffTip(ref string tip, ref int rare) => rare = 10;
    }
    public class VeritasActive : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Veritas");
            Description.SetDefault("Increased damage, decreased life");

            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.persistentBuff[Type] = true;
            canBeCleared = false;
        }
        public override void ModifyBuffTip(ref string tip, ref int rare) => rare = 10;
    }
}
