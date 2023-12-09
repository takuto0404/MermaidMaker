using System;
using System.Threading;

namespace Jamaica
{
    public class Dice
    {

        /// <summary>
        /// このサイコロが答え用のサイコロかどうか
        /// </summary>
        public bool IsAnswerBox;
    
        /// <summary>
        /// このサイコロが活動状態かどうか
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// このサイコロをシャッフルする
        /// </summary>
        /// <param name="shuffleLength">シャッフルする時間</param>
        /// <param name="gameCt"></param>
        public async void ShuffleAsync(float shuffleLength,CancellationToken gameCt)
        {
            if (IsAnswerBox)
            {
                (int number1, int number2) randomAnswer = (0, 0);
                int randomNum1 = randomAnswer.number1;
                int randomNum2 = randomAnswer.number2;
                
                return;
            }
        }
    }
}