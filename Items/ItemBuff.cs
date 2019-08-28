using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Erilipah.Items
{
    public static class ItemBuffID
    {
        public const string DuctTape = "Duct Tape";
        public const string Geode = "Enormous Geode";
    }
    public class ItemBuff : GlobalItem
    {
        private void UpdateBuffInventory(Item item, Player owner, Tuple<string, int> buff)
        {

        }

        private void UpdateBuffEquip(Item item, Player owner, Tuple<string, int> buff)
        {

        }

        private void UpdateBuffAccessory(Item item, Player owner, Tuple<string, int> buff)
        {

        }

        private void ItemBuffStats(Player player, out float dmgMult, out int critInc, out float kbMult, out float useSpeedMult, out string description)
        {
            useSpeedMult = 1;
            dmgMult = 1;
            critInc = 0;
            kbMult = 1;
            description = "";
            foreach (var buffItemTuple in itemBuffs)
            {
                string buffType = buffItemTuple.Item1;
                int buffTime = BuffTime(buffItemTuple.Item2);

                if (buffType == ItemBuffID.DuctTape)
                {
                    useSpeedMult -= 0.2f;
                    dmgMult += 0.2f;
                    kbMult += 0.2f;
                    description = "Increased damage and knockback, decreased use speed";
                }
                if (buffType == ItemBuffID.Geode)
                    description = "Decreased mana cost and inaccuracy";
            }
        }

        /// <summary>
        /// Returns buffTimes index.
        /// </summary>
        /// <returns>The bufftime ID.</returns>
        public int NewBuff(string buffType, int buffTime, bool display = true)
        {
            int openTime = FindOpenTime(buffType, buffTime);
            if (openTime <= -1)
            {
                return -1;
            }
            // else
            noDisplayTimes[openTime] = !display;
            buffTimes[openTime] = buffTime;
            itemBuffs.Add(Tuple.Create(buffType, openTime));
            return openTime;
        }
        public bool HasBuff(string buffType)
        {
            foreach (var buff in itemBuffs)
            {
                if (buff.Item1 == buffType)
                    return true;
            }
            return false;
        }
        public int FindBuff(string buffType)
        {
            foreach (var buff in itemBuffs)
            {
                if (buff.Item1 == buffType)
                    return buff.Item2;
            }
            return -1;
        }
        private int FindOpenTime(string type, int time)
        {
            foreach (var i in itemBuffs)
            {
                if (i.Item1 == type && BuffTime(i.Item2) <= time)
                {
                    buffTimes[i.Item2] = time;
                    return -1;
                }
            }
            for (int i = 0; i < 255; i++)
            {
                if (buffTimes[i] <= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        //public bool RemoveBuff(string type)
        //{
        //    bool success = false;
        //    foreach (var tuple in itemBuffs)
        //    {
        //        if (tuple.Item1 == type)
        //        {

        //        }
        //    }
        //    return success;
        //}

        //void KillBuff(List<Tuple<string, int>> tuples)
        //{

        //}
        public bool CanBeBuffed = true;
        private int[] buffTimes = new int[255];
        private bool[] noDisplayTimes = new bool[255];
        public List<Tuple<string, int>> itemBuffs = new List<Tuple<string, int>>();

        public int BuffTime(int index) => buffTimes[index];
        public bool DisplayTime(int index) => !noDisplayTimes[index];

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            itemBuffs.RemoveAll(x => buffTimes[x.Item2] <= 0);

            for (int i = 0; i < buffTimes.Length; i++)
            {
                if (buffTimes[i] > 0)
                    buffTimes[i]--;
            }
        }
        public override void UpdateInventory(Item item, Player player)
        {
            itemBuffs.RemoveAll(x => buffTimes[x.Item2] <= 0);

            for (int i = 0; i < buffTimes.Length; i++)
            {
                if (buffTimes[i] > 0)
                    buffTimes[i]--;
            }

            foreach (var tuple in itemBuffs)
            {
                if (buffTimes[tuple.Item2] > 0)
                {
                    UpdateBuffInventory(item, player, tuple);
                }
            }
        }
        public override void UpdateEquip(Item item, Player player)
        {
            foreach (var tuple in itemBuffs)
            {
                if (buffTimes[tuple.Item2] > 0)
                {
                    UpdateBuffEquip(item, player, tuple);
                }
            }
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            foreach (var tuple in itemBuffs)
            {
                if (buffTimes[tuple.Item2] > 0)
                {
                    UpdateBuffAccessory(item, player, tuple);
                }
            }
        }
        public override void HoldItem(Item item, Player player)
        {
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i] == item)
                    return;
            }
            UpdateInventory(item, player);
        }

        public override void GetWeaponCrit(Item item, Player player, ref int crit)
        {
            float d;
            int c;
            float kb;
            float utm;
            string s;
            ItemBuffStats(player, out d, out c, out kb, out utm, out s);
            crit += c;
        }
        public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
        {
            float d;
            int c;
            float kb;
            float utm;
            string s;
            ItemBuffStats(player, out d, out c, out kb, out utm, out s);
            flat += d;
        }
        public override void GetWeaponKnockback(Item item, Player player, ref float knockback)
        {
            float d;
            int c;
            float kb;
            float utm;
            string s;
            ItemBuffStats(player, out d, out c, out kb, out utm, out s);
            knockback *= kb;
        }
        public override float UseTimeMultiplier(Item item, Player player)
        {
            float d;
            int c;
            float kb;
            float utm;
            string s;
            ItemBuffStats(player, out d, out c, out kb, out utm, out s);
            return utm;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            foreach (var buffItemTuple in itemBuffs)
            {
                TooltipLine tooltipLine = new TooltipLine(mod, "Item Buff", "")
                {
                    overrideColor = Color.Aquamarine
                };
                string buffType = buffItemTuple.Item1;
                int buffTime = BuffTime(buffItemTuple.Item2);

                bool inMinutes = buffTime >= 3600;
                string minutes = (buffTime / 3600).ToString() + 'm';
                string seconds = ((buffTime % 3600) / 60).ToString() + 's';
                string time = inMinutes ?
                    minutes + ' ' + seconds :
                    seconds;

                float a;
                int b;
                string desc = "";
                if (noDisplayTimes[buffItemTuple.Item2] != true && noDisplayTimes[buffItemTuple.Item2] != false)
                    noDisplayTimes[buffItemTuple.Item2] = true;
                string timeQ = !noDisplayTimes[buffItemTuple.Item2] ? ": " + time : "";
                ItemBuffStats(Main.LocalPlayer, out a, out b, out a, out a, out desc);

                tooltipLine.text =
                    buffType +
                    timeQ +
                    "\n" + desc;

                tooltips.Add(tooltipLine);
            }
        }

        public override bool InstancePerEntity => true;
        public ItemBuff()
        {
            buffTimes = new int[255];
            noDisplayTimes = new bool[255];
            itemBuffs = new List<Tuple<string, int>>();
        }
        public override GlobalItem Clone(Item item, Item itemClone)
        {
            ItemBuff myClone = (ItemBuff)base.Clone(item, itemClone);
            myClone.buffTimes = (int[])buffTimes.Clone();
            myClone.noDisplayTimes = (bool[])noDisplayTimes.Clone();
            myClone.itemBuffs = itemBuffs.Select(s => new Tuple<string, int>(s.Item1, s.Item2)).ToList();
            return myClone;
        }

        public override TagCompound Save(Item item)
        {
            var list = new List<TagCompound>();
            foreach (var i in itemBuffs)
            {
                list.Add(new TagCompound() {
                    { "type", i.Item1 },
                    { "timeIndex", i.Item2 },
                });
            }
            return new TagCompound {
                { "buffTimes", buffTimes },
                { "displayTimes", noDisplayTimes.ToList() },
                { "itemBuffs", list }
            };
        }
        public override void Load(Item item, TagCompound tag)
        {
            buffTimes = tag.GetIntArray("buffTimes");
            noDisplayTimes = tag.GetList<bool>("displayTimes").ToArray();

            itemBuffs = new List<Tuple<string, int>>();
            var list = tag.GetList<TagCompound>("itemBuffs");
            foreach (var i in list)
            {
                string type = i.GetString("type");
                int index = i.GetInt("timeIndex");
                itemBuffs.Add(new Tuple<string, int>(type, index));
            }
        }
        public override bool NeedsSaving(Item item)
        {
            for (int i = 0; i < buffTimes.Length; i++)
            {
                if (buffTimes[i] > 0)
                    return true;
            }
            return false;
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            for (int i = 0; i < buffTimes.Length; i++)
            {
                writer.Write(buffTimes[i]);
            }
        }
        public override void NetReceive(Item item, BinaryReader reader)
        {
            for (int i = 0; i < buffTimes.Length; i++)
            {
                buffTimes[i] = reader.ReadInt32();
            }
        }
    }
}
