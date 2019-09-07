using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Erilipah.Items
{
    public abstract class Bandoliers : ModItem
    {
        public List<Item> Bullets { get; set; } = new List<Item>();
        public Item Bullet => Bullets.Last();

        public void UpdateStats()
        {
            if (Bullets.Count == 0)
            {
                item.ammo = AmmoID.None;
                item.damage = 0;
                item.knockBack = 0;
                item.crit = 0;
                item.shoot = 0;
                item.shootSpeed = 0;
                item.rare = 3;
                return;
            }
            item.ammo = AmmoID.Bullet;
            item.damage = Bullet.damage;
            item.knockBack = Bullet.knockBack;
            item.crit = Bullet.crit;
            item.shoot = Bullet.shoot;
            item.shootSpeed = Bullet.shootSpeed;
            item.rare = Bullet.rare;
        }
        public override bool CanRightClick() => true;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int insertTo = tooltips.FindIndex(x => x.Name == "Tooltip0");
            Color color1 = new Color(1f, 0.5f, 1f);
            Color color2 = new Color(0.0f, 1f, 1f);

            // Insert a tooltip below the usage toolip listing the item types + their stacks
            foreach (var item in Bullets)
            {
                int index = Bullets.IndexOf(item);
                string num = 1 + index + "  ";
                if (index < 9)
                    num += " ";
                if (index == 0)
                    num += " ";

                string itemName = num + item.Name;
                if (item.stack > 1)
                    itemName += " (" + item.stack + ")";

                TooltipLine tooltipLine = new TooltipLine(mod, "Bullets", itemName);
                if (this is Bandolier)
                    tooltipLine.overrideColor = Color.Lerp(color1, color2, (4 - index) / 7f);
                else
                    tooltipLine.overrideColor = Color.Lerp(color1, color2, (9 - index) / 14f);

                tooltips.Insert(insertTo, tooltipLine);
            }

            if (Bullets.Count > 0)
            {
                var name = tooltips.Find(x => x.Name == "ItemName");
                name.text += ": " + Bullet.Name;
            }
        }

        public override bool CloneNewInstances => true;
        public override ModItem Clone()
        {
            Bandoliers clone = (Bandoliers)base.Clone();
            clone.Bullets = Bullets.ConvertAll(item => item.Clone());
            return clone;
        }

        public override TagCompound Save() => new TagCompound() { { "bullets", Bullets } };
        public override void Load(TagCompound tag)
        {
            Bullets = tag.GetList<Item>("bullets").ToList();
            UpdateStats();
        }

        public class Bandolier : Bandoliers
        {
            public override string Texture => "Erilipah/TempSprite";
            public override void SetStaticDefaults()
            {
                Tooltip.SetDefault("Right click in inventory to add or remove bullets\n" +
                    "Press the Bandolier hotkey to cycle through bullets\n" +
                    "Can hold five types of bullets of any stack count\n" +
                    "When in use, gun speed increases but extra ammo is consumed\n" +
                    "'For all your gun-slingin' needs'");
            }
            public override void SetDefaults()
            {
                item.maxStack = 1;

                item.ranged = true;
                item.width = 32;
                item.height = 32;
                item.consumable = false;

                item.value = Item.buyPrice(0, 35, 0, 0);
                item.rare = ItemRarityID.Green;
                UpdateStats();
            }

            public override void RightClick(Player player)
            {
                item.stack++;
                // Get the item that the player is holding in their mouse.
                Item mouse = Main.mouseItem;

                // If the bandolier already has an item of this type,
                if (Bullets.Any(x => x.type == mouse.type))
                {
                    // just add their stacks together.
                    Item item1 = Bullets.First(x => x.type == mouse.type);
                    item1.stack += mouse.stack;

                    mouse.TurnToAir();
                    player.inventory[58] = mouse;
                }
                else if (Bullets.Count > 0 && (mouse.IsAir || mouse.ammo != AmmoID.Bullet || mouse.notAmmo || !mouse.consumable))
                {
                    if (Bullet.stack > 4995)
                    {
                        Bullet.stack -= 4995;
                        for (int i = 0; i < 5; i++)
                        {
                            player.QuickSpawnItem(Bullet, 999);
                        }
                    }
                    else
                    {
                        int numStacks = Bullet.stack / 999;
                        int remainder = Bullet.stack % 999;

                        for (int i = 0; i < numStacks; i++)
                        {
                            player.QuickSpawnItem(Bullet, 999);
                        }

                        player.QuickSpawnItem(Bullet, remainder);
                        Bullets.Remove(Bullet);
                    }
                }
                // Otherwise, add a new item to the queue.
                else if (Bullets.Count < 5)
                {
                    mouse.maxStack = 999999;
                    Bullets.Add(mouse.Clone());

                    mouse.TurnToAir();
                    player.inventory[58] = mouse;
                }

                // Update stats, kill the item, and update the mouse item.
                Bullets.RemoveAll(x => x.IsAir || x.type == 0);
                UpdateStats();
            }

            public override void OnConsumeAmmo(Player player)
            {
                if (Bullets == null || Bullets.Count == 0)
                    return;
                // If the current bullet is about to run out, dequeue it.
                if (Bullet.stack <= 2)
                {
                    Bullets.Remove(Bullet);
                }
                else // Otherwise just decrease stack.
                {
                    Bullet.stack -= 2;
                }

                UpdateStats();
            }
        }

        public class ThreeTorusBandolier : Bandoliers
        {
            public override string Texture => "Erilipah/TempSprite";
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Three-Torus Bandolier");
                Tooltip.SetDefault("Functions like the Bandolier; can hold ten types of bullets\n" +
                    "A full stack of bullets is needed to add a type of bullet to the Bandolier\n" +
                    "When in use, gun speed increases greatly and no ammo is consumed\n" +
                    "'It's bigger on the inside'");
            }
            public override void SetDefaults()
            {
                item.maxStack = 1;

                item.ranged = true;
                item.width = 32;
                item.height = 32;
                item.consumable = false;

                item.value = Item.buyPrice(0, 20, 0, 0);
                item.rare = ItemRarityID.LightRed;
                UpdateStats();
            }

            public override void AddRecipes()
            {
                ModRecipe r = new ModRecipe(mod);
                r.AddIngredient(mod.ItemType<Bandolier>(), 1);
                r.AddIngredient(TileID.Crystals, 3);
                r.AddIngredient(ItemID.EndlessMusketPouch, 1);
                r.AddTile(TileID.CrystalBall);
                r.SetResult(this);
                r.AddRecipe();
            }

            public override void RightClick(Player player)
            {
                item.stack++;
                // Get the item that the player is holding in their mouse.
                Item mouse = Main.mouseItem;

                // If the bandolier already has an item of this type,
                if (Bullets.Any(x => x.type == mouse.type))
                {
                }
                else if (Bullets.Count > 0 && (mouse.IsAir || mouse.ammo != AmmoID.Bullet || mouse.notAmmo || !mouse.consumable))
                {
                    // Don't give back more than 999
                    player.QuickSpawnItem(Bullet, Math.Min(999, Bullet.maxStack));
                    Bullets.Remove(Bullet);
                }
                // Otherwise, add a new item to the queue.
                else if (Bullets.Count < 10 && mouse.stack >= 999 || mouse.stack == mouse.maxStack)
                {
                    mouse.stack = 1;
                    Bullets.Add(mouse.Clone());

                    mouse.TurnToAir();
                    player.inventory[58] = mouse;
                }

                // Update stats, kill the item, and update the mouse item.
                Bullets.RemoveAll(x => x.IsAir);
                UpdateStats();
            }

            public override void OnConsumeAmmo(Player player)
            {
                UpdateStats();
            }
        }
    }
    public class BandolierGlobal : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;

        public override float UseTimeMultiplier(Item item, Player player)
        {
            bool valid = item.useAmmo == AmmoID.Bullet && item.ranged;
            if (!valid)
                return 1;
            if (Helper.FindAmmo(player).modItem is Bandoliers.Bandolier)
                return 1.15f;
            if (Helper.FindAmmo(player).modItem is Bandoliers.ThreeTorusBandolier)
                return 1.25f;
            return 1;
        }
    }
    public class BandolierPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            Item item = player.FindAmmo(i => i.modItem is Bandoliers);
            if (item == null)
                return;
            Bandoliers band = item.modItem as Bandoliers;

            if (Erilipah.Bandolier.JustPressed && item != null)
            {
                Main.PlaySound(SoundID.Item11, player.Center);
                List<Item> newQueue = new List<Item>
                {
                    band.Bullets.Last() // Add the last element to the front.
                };
                foreach (var curBullet in band.Bullets)
                {
                    // Excepting the last element because it was already added,
                    // add all the elements back to the new list.
                    if (curBullet != band.Bullets.Last())
                        newQueue.Add(curBullet);
                } // This COULD be done with more Linq expressions, but it's cleaner this way.

                // Update the queue, update the stats, and run no more code.
                band.Bullets = newQueue;
                band.UpdateStats();
                CombatText.NewText(player.getRect(), new Color(0, 255, 255), band.Bullet.Name);
            }
        }
    }
}
