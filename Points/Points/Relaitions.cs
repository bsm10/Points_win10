using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Points
{
    class Relaitions
    {
        public Dot d1, d2;
        public int Own;
        public enum Owner : int { None = 0, Player1 = 1, Player2 = 2 }
        public Relaitions(Dot d1,Dot d2)//конструктор класса
        {
            this.d1  = d1;
            this.d2  = d2;
        }

    }
}
