```mermaid
    classDiagram

    class JamaicaSolver{
       -static (int,string)[] ab
       -static Stack<(int,string)> ba
       -static readonly Stack<(int,string,int)[]> DiceHist
       -static readonly Dictionary<int,string> OperatorDic
       -static (int,string,int)[] _dices
       -static List<string> _solutions
       -static int _answer
   }

    class Hoge{
       -static readonly Stack<(int,string,int)> DiceHist
       -int _a
       +int A(int a,string b)
   }

    class Values{
    <<enum>>
   }

    class IInterface{
    <<interface>>
       +int A(int a,string b)
   }

    class Fuga{
   }

    class B{
   }
JamaicaSolver --|> Object
Hoge --|> Fuga
Hoge ..|> IInterface
Fuga --|> Object
B --|> Object
```