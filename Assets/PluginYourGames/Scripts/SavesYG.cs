using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        // Ваши сохранения
        public List<QuizCardSaveData> quizCards = new();
        public List<string> favoriteCards = new();
        public string[] otherCards = null;

        public int cash = 0;

        public bool isFirstQuiz = true;
        public bool isMusicPlaying = true;
    }
}
