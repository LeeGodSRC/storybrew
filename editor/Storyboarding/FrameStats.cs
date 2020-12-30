namespace StorybrewEditor.Storyboarding
{
    public class FrameStats
    {
        public int SpriteCount;
        public int CommandCount;
        public int EffectiveCommandCount;
        public bool IncompatibleCommands;
        public string OverlappedCommands; // LeeGod - Show which sprite is overlapping
        public float ScreenFill;
    }
}
