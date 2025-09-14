using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        public List<QuizCardSaveData> quizCards = new();
        public List<string> favoriteCards = new();
        public string[] otherCards = null;

        public int cash = 0;

        public float musicVolume = 1f;
        public float vfxVolume = 1f;

        public bool isFirstQuiz = true;
    }
}
