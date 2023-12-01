using System.Collections.Generic;
using System.Linq;

namespace Test2
{
    public static class JamaicaSolver
    {
        private static (int a, string b)[] ab;
        private static Stack<(int,string)> ba;
        /// <summary>
        /// ソルブできるか、どのようにソルブできるかを試す
        /// </summary>
        /// <param name="dices">元々あるサイコロ</param>
        /// <returns>ソルブ判定の結果を返すSolutionクラウs</returns>
        private static readonly Stack<(int number, string text, int beforeOperatorMark)[]> DiceHist =
            new Stack<(int number, string text, int beforeOperatorMark)[]>();

        private static readonly Dictionary<int, string> OperatorDic = new Dictionary<int, string>()
        {
            { 0, "+" },
            { 1, "-" },
            { 2, "*" },
            { 3, "/" }
        };

        private static (int number, string text, int beforeOperatorMark)[] _dices;
        private static List<string> _solutions;
        private static int _answer;


        public static (bool canSolve, List<string> solutions) SolveJamaica(int answer, int[] diceNumbers)
        {
            _solutions = new List<string>();
            _dices = new (int number, string text, int beforeOperatorMark)[diceNumbers.Length];
            for (int i = 0; i < diceNumbers.Length; i++)
            {
                _dices[i] = (diceNumbers[i], diceNumbers[i].ToString(), -1);
            }

            DiceHist.Push(_dices);
            JamaicaSolver._answer = answer;
            Solve();

            return (_solutions.Count > 0, _solutions);
        }

        private static void Solve()
        {
            var last = DiceHist.Peek();
            var subscribed1 = new List<int>();
            for (int i = 0; i < _dices.Length; i++)
            {
                if (last[i].number == -1 || subscribed1.Contains(last[i].number)) continue;
                subscribed1.Add(last[i].number);
                var subscribed2 = new List<int>();
                for (int j = 0; j < _dices.Length; j++)
                {
                    if (i == j || last[j].number == -1 || last[i].number == -1 || subscribed2.Contains(last[i].number) ||
                        last[i].number < last[j].number) continue;
                    subscribed2.Add(last[i].number);
                    for (int k = 0; k < 4; k++)
                    {
                        var newArray = CopyArray(last);
                        var oneBefore = newArray[i];
                        var anotherOneBefore = newArray[j];

                        var one = newArray[i];
                        var anotherOne = newArray[j];
                        var canCalculate = new bool[4]
                        {
                            true, one.number <= anotherOne.number, true,
                            ((float)one.number / (float)anotherOne.number) % 1 == 0 && anotherOne.number != 0 &&
                            anotherOne.number != 1
                        };

                        if (!canCalculate[k]) continue;
                        if ((oneBefore.beforeOperatorMark is 0 or 2 && oneBefore.beforeOperatorMark == k) || (anotherOneBefore.beforeOperatorMark is 0 or 2 &&
                                anotherOneBefore.beforeOperatorMark == k))
                        {
                            if (oneBefore.number < anotherOne.number)
                            {
                                continue;
                            }
                        }
                        if (oneBefore.beforeOperatorMark == 3 && anotherOneBefore.beforeOperatorMark == 2)
                        {
                            continue;
                        }

                        switch (k)
                        {
                            case 0:
                                one.number += anotherOne.number;
                                break;
                            case 1:
                                one.number -= anotherOne.number;
                                break;
                            case 2:
                                one.number *= anotherOne.number;
                                break;
                            case 3:
                                one.number /= anotherOne.number;
                                break;
                        }

                        if (k is 2 or 3)
                        {
                            if (oneBefore.beforeOperatorMark is 0 or 1 && anotherOneBefore.beforeOperatorMark is 0 or 1)
                            {
                                one.text = $"({oneBefore.text}) {OperatorDic[k]} ({anotherOneBefore.text})";
                            }
                            else if (oneBefore.beforeOperatorMark is 0 or 1)
                            {
                                one.text = $"({oneBefore.text}) {OperatorDic[k]} {anotherOneBefore.text}";
                            }
                            else if (anotherOneBefore.beforeOperatorMark is 0 or 1)
                            {
                                one.text = $"{oneBefore.text} {OperatorDic[k]} ({anotherOneBefore.text})";
                            }
                            else
                            {
                                one.text = $"{oneBefore.text} {OperatorDic[k]} {anotherOneBefore.text}";
                            }
                        }
                        else
                        {
                            one.text = $"{oneBefore.text} {OperatorDic[k]} {anotherOneBefore.text}";
                        }


                        one.beforeOperatorMark = k;

                        anotherOne.text = "";
                        anotherOne.number = -1;
                        anotherOne.beforeOperatorMark = -1;

                        newArray[i] = one;
                        newArray[j] = anotherOne;
                        DiceHist.Push(newArray);

                        var activeList = newArray.Where(x => x.number != -1).ToArray();
                        if (activeList.Length == 1)
                        {
                            if (activeList.First().number == _answer)
                            {
                                var text = $"{activeList.First().text} = {_answer}";
                                if (_solutions.Contains(text)) continue;
                                _solutions.Add(text);
                            }
                        }

                        Solve();

                        DiceHist.Pop();
                    }
                }
            }
        }

        static (int number, string text, int beforeOperatorMark)[] CopyArray(
            (int number, string text, int beforeOpeartorMark)[] array)
        {
            (int number, string text, int beforeOperatorMark)[] newArray =
                new (int number, string text, int beforeOperatorMark)[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }
    }
}