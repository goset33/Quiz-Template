using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        // Ваши сохранения
        public Dictionary<string, int> levelsHardness = new(); // Сериализуется 😎
        public List<QuizCard> favoriteCards = new(), otherCards = new();

        public int level = 1;
        public int experience = 0;
        public int requiredExp = 100;
        public int cash = 0;

        public bool isMusicPlaying = true;
    }
}
