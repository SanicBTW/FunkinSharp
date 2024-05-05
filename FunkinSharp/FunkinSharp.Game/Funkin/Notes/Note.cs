using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using osu.Framework.Allocation;
using osuTK;

namespace FunkinSharp.Game.Funkin.Notes
{
    // Scroll Note, shouldn't have any other animation aside from the <color>Scroll one, so its going to be just like AlphaCharacter
    // This will have the Sustain attached to it but won't hold any logic for drawing or loading the frames
    // Some stuff is based of Forever Engine : Rewrite and my shi https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/Note.hx
    public partial class Note : FrameAnimatedSprite
    {
        public static Dictionary<string, FEReceptorData> DataCache = [];
        public FEReceptorData ReceptorData { get; private set; }

        public readonly float StrumTime;
        public readonly int NoteData;
        public readonly string NoteType;
        public readonly int StrumLine;

        public bool BoundToSustain = false;

        public bool GoodHit = false;

        public Note(float strumTime, int noteData, string noteType = "default", int strumLine = 0)
        {
            StrumTime = strumTime;
            NoteData = noteData;
            NoteType = noteType;
            StrumLine = strumLine;

            Loop = true;
            Anchor = Origin = osu.Framework.Graphics.Anchor.Centre;
            Y = -2000;
        }

        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore, SparrowAtlasStore sparrowStore)
        {
            if (NoteData < -1)
                return;

            // these bad boys should be already cached but we ball anyways
            if (DataCache.ContainsKey(NoteType))
                ReceptorData = DataCache[NoteType];
            else
                ReceptorData = DataCache[NoteType] = jsonStore.Get<FEReceptorData>($"NoteTypes/{NoteType}/{NoteType}");

            Atlas = sparrowStore.GetSparrow($"NoteTypes/{NoteType}/{ReceptorData.Texture}");

            // AlphaCharacter stuff, basically add only the frames inside the range
            if (Animations.TryGetValue(GetNoteColor(), out AnimationFrame anim))
            {
                AddFrameRange(anim.StartFrame, anim.EndFrame);
                CurAnim = anim;
                CurAnimName = GetNoteColor();
                Scale = new Vector2(ReceptorData.Size);
            }
        }

        public string GetNoteDirection()
        {
            return ReceptorData.Actions[NoteData];
        }

        public string GetNoteColor()
        {
            return ReceptorData.Colors[NoteData];
        }
    }
}
