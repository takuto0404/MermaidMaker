using System;
using System.Collections.Generic;

namespace Test
{
    class Hoge : Fuga,IInterface
    {
        // private static readonly int DiceHist;
        // private static readonly Stack<int> DiceHist;
        private static readonly Stack<(int number, string text, int beforeOperatorMark)> DiceHist;
        // private static readonly Stack<int[]> DiceHist;

        // private static readonly Stack<(int number, string text, int beforeOperatorMark)[]> DiceHist;
        private int _a;
        public int A(int a,string b)
        {
            return 0;
        }
    }

    enum Values
    {
        
    }
    interface IInterface
    {
        int A(int a,string b);
    }

    abstract class Fuga
    {
        public Fuga()
        {
            
        }
    }

    static class B
    {
        
    }
}