using Erilipah.UI.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Erilipah
{
    public class NoteHandler : ModPlayer
    {
        private bool[] collectedNotes = new bool[NoteUIPages.pageCount];

        public bool[] GetCollectedNotes()
        {
            return (bool[])collectedNotes.Clone();
        }

        public void CollectNote(int noteType)
        {
            if (noteType < 0 || noteType >= NoteUIPages.pageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(noteType));
            }

            if (collectedNotes[noteType] == true)
            {
                return;
            }

            collectedNotes[noteType] = true;
            Erilipah.noteUI.Refresh();
        }

        public override void Initialize()
        {
            collectedNotes = new bool[NoteUIPages.pageCount];
        }

        public override TagCompound Save()
        {
            TagCompound compound = new TagCompound();
            for (int i = 0; i < NoteUIPages.pageCount; i++)
            {
                compound.Add($"Note{i}", collectedNotes[i]);
            }
            return compound;
        }

        public override void Load(TagCompound tag)
        {
            for (int i = 0; i < NoteUIPages.pageCount; i++)
            {
                collectedNotes[i] = tag.GetBool($"Note{i}");
                Erilipah.noteUI.Refresh(false);
            }
        }
    }
}
