using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jamaica
{
    public class DiceModel
    {
        /// <summary>
        /// 計算目標のサイコロ
        /// </summary>
        private static Dice _answerDice;
    
        /// <summary>
        /// 式に使用できるサイコロたち
        /// </summary>
        private static List<Dictionary<Dice,DiceModel>> _dices;


        public static void SetDice(List<Dice> dices,Dice answerDice)
        {
            _answerDice = answerDice;
            _answerDice.IsAnswerBox = true;
        }
    }
}