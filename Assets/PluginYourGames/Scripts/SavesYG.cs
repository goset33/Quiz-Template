using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        // Ваши сохранения
        public Dictionary<string, int> levelsHardness = new();
        public List<string> favoriteCards = new();
        public string[] otherCards = null;

        public int level = 1;
        public int experience = 0;
        public int requiredExp = 100;
        public int cash = 0;

        public bool isMusicPlaying = true;
    }
}
