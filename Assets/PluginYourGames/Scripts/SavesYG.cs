using R3;
using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        public List<QuizCardSaveData> quizCards = new();
        public List<string> favoriteCards = new();
        public string[] otherCards = null;

        public int cash = 0;

        public ReactiveProperty<int> musicVolume = new ReactiveProperty<int>(100);
        public ReactiveProperty<int> vfxVolume = new ReactiveProperty<int>(100);

        public bool isFirstQuiz = true;       
    }
}
