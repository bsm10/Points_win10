﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Points
{
    public partial class GameDots : IEnumerator, IEnumerable
    {
        private const int PLAYER_DRAW = -1;
        private const int PLAYER_NONE = 0;
        private const int PLAYER_HUMAN = 1;
        private const int PLAYER_COMPUTER = 2;

        public int SkillLevel = 5;
        public int SkillDepth = 5;
        public int SkillNumSq = 3;

        //statistic
        public float square1;//площадь занятая игроком1
        public float square2;
        public int count_blocked;//счетчик количества окруженных точек
        public int count_blocked1, count_blocked2;
        public int count_dot1, count_dot2;//количество поставленных точек


        private List<Dot> list_moves; //список ходов

        public List<Dot> ListMoves // список ходов
        {
            get { return list_moves; }
            set { list_moves = value; }
        }
        public Dot LastMove
        {
            get
            {
                if (ListMoves.Count == 0)//когда выбирается первая точка для хода
                {
                    var random = new Random(DateTime.Now.Millisecond);
                    var q = from Dot d in Dots
                            where d.x <= BoardWidth / 2 & d.x > BoardWidth / 3
                                                    & d.y <= BoardHeight / 2 & d.y > BoardHeight / 3
                            orderby (random.Next())
                            select d;
                    return q.First();

                }
                else
                {
                    return ListMoves.Last();
                }
            }
        }
        private Dot best_move; //ход который должен сделать комп

        public List<Dot> dots_in_region;//записывает сюда точки, которые окружают точку противника

        private List<Dot> _Dots; // главная коллекция
        public List<Dot> Dots
        {
            get
            {
                return _Dots;
            }
            set
            {
                _Dots = value;
            }
        }


        //private List<Dot> _DotsForDrawing = new List<Dot>();

        public List<Dot> ListDotsForDrawing
        {// главная коллекция для отрисовки партии
            get
            {
                return ListMoves.ToList();
            }
        }

        /// <summary>
        /// Возвращает список не занятых точек
        /// </summary>
        public List<Dot> Board_ValidMoves
        {
            get
            {
                return (from Dot d in _Dots where d.ValidMove select d).ToList();
            }
        }
        public List<Dot> Board_NotEmptyNonBlockedDots
        {
            get
            {
                return (from Dot d in _Dots where d.Own != 0 && d.Blocked == false select d).ToList();
            }
        }
        public List<Dot> Board_AllNotBlockedDots
        {
            get
            {
                return _Dots.Where(d=>!d.Blocked).ToList();
            }
        }


        public GameDots CopyDots
        {
            get
            {
                GameDots ad = new GameDots(BoardWidth,BoardHeight);
                for (int i = 0; i < Dots.Count; i++)
                {
                    ad._Dots[i].Blocked = Dots[i].Blocked;
                    ad._Dots[i].Fixed = Dots[i].Fixed;
                    ad._Dots[i].IndexDot = Dots[i].IndexDot;
                    ad._Dots[i].Own = Dots[i].Own;
                    ad._Dots[i].x = Dots[i].x;
                    ad._Dots[i].y = Dots[i].y;
                }
                return ad;
            }
        }
#if DEBUG
        Stopwatch stopWatch = new Stopwatch();//для диагностики времени выполнения
        Stopwatch sW_BM = new Stopwatch();
        Stopwatch sW2 = new Stopwatch();
        List<string> lstDbgMoves = new List<string>();
        List<string> lstDbg1 = new List<string>();
#endif

        public int win_player;//переменная получает номер игрока, котрый окружил точки
        int position = -1;
        private int nBoardWidth;//размер поля
        private int nBoardHeight;//размер поля


        public int BoardWidth
        {
            get { return nBoardWidth; }
            set { nBoardWidth = value; }
        }
        public int BoardHeight
        {
            get { return nBoardHeight; }
            set { nBoardHeight = value; }
        }
        public int BoardSize
        {
            get
            {
                return nBoardWidth * nBoardHeight;
            }
        }

        public GameDots(int boardwidth, int boardheight)
        {
            int counter = 0;
            int ind;
            BoardHeight = boardheight;
            BoardWidth = boardwidth;
            _Dots = new List<Dot>(boardwidth * boardheight); // главная коллекция точек
            for (int i = 0; i < boardwidth; i++)
            {
                for (int j = 0; j < boardheight; j++)
                {
                    ind = IndexDot(i, j);
                    _Dots.Add(new Dot(i, j));
                    _Dots[ind].IndexDot = ind;
                    if (i == 0 | i == (boardwidth - 1) | j == 0 | j == (boardheight - 1)) _Dots[ind].Fixed = true;
                    counter += 1;
                }
            }

            lnks = new List<Links>();
            list_moves = new List<Dot>();
            dots_in_region = new List<Dot>();
            square1 = 0; square2 = 0;
            count_blocked1 = 0; count_blocked2 = 0;
            count_blocked = 0;
            count_dot1 = 0; count_dot2 = 0;
        }
        public class DotEq : EqualityComparer<Dot>
        {
            public override int GetHashCode(Dot dot)
            {
                int hCode = dot.x ^ dot.y;
                return hCode.GetHashCode();
            }

            public override bool Equals(Dot d1, Dot d2)
            {
                //return Default.Equals(d1, d2);
                return (d1.x == d2.x & d1.y == d2.y); 
            }
        }
        public int Count
        {
            get
            {
                return _Dots.Count;
            }
        }
        public Dot this[int i, int j]//Индексатор возвращает элемент из массива по его индексу
        {
            get
            {
                if (i >= BoardWidth) i = BoardWidth - 1;
                if (j >= BoardHeight) j = BoardHeight - 1;
                if (i < 0) i = 0;
                if (j < 0) j = 0;
                //return DotIndexCheck(i,j) ? null : _Dots[IndexDot(i, j)];
                return _Dots[IndexDot(i, j)];
            }
        }
        private void Add(Dot dot)//добавляет точку в массив
        {
            int ind = IndexDot(dot.x, dot.y);
            if (DotIndexCheck(dot.x, dot.y))
            {
                _Dots[ind].Own = dot.Own;
                if (dot.Own != 0) _Dots[ind].IndexRelation = _Dots[ind].IndexDot;
                _Dots[ind].Blocked = false;
                if (dot.x == 0 | dot.x == (BoardWidth - 1) | dot.y == 0 | 
                    dot.y == (BoardHeight - 1)) _Dots[ind].Fixed = true;
                AddNeibor(_Dots[ind]);

                //ListMoves.Add(_Dots[ind]);
            }
        }

        private void AddNeibor(Dot dot)
        {
            //выбрать соседние точки, если такие есть
            var q = from Dot d in _Dots where d.Own == dot.Own & Distance(dot, d) < 2 select d;

            foreach (Dot d in q)
            {
                if (d != dot)
                {
                    if (dot.Rating > d.Rating) dot.Rating = d.Rating;
                    if (dot.NeiborDots.Contains(d) == false) dot.NeiborDots.Add(d);
                    if (d.NeiborDots.Contains(dot) == false) d.NeiborDots.Add(dot);
                }
            }
            MakeIndexRelation(dot);
        }
        private void RemoveNeibor(Dot dot)
        {
            foreach (Dot d in Dots)
            {
                if (d.NeiborDots.Contains(dot)) d.NeiborDots.Remove(dot);
            }
        }
        private void Remove(Dot dot)//удаляет точку из массива
        {
            int ind = IndexDot(dot.x, dot.y);
            if (DotIndexCheck(dot.x, dot.y))
            {
                int i = _Dots[ind].IndexDot;
                RemoveNeibor(dot);
                _Dots[ind] = new Dot(dot.x, dot.y);
                _Dots[ind].IndexDot = i;
                _Dots[ind].IndexRelation = i;
                ListMoves.Remove(dot);
            }
        }
        public float Distance(Dot dot1, Dot dot2)//расстояние между точками
        {
            return (float)Math.Sqrt(Math.Pow((dot1.x -dot2.x), 2) + Math.Pow((dot1.y -dot2.y), 2));
        }
        /// <summary>
        /// возвращает список соседних точек заданной точки
        /// </summary>
        /// <param name="dot"> точка Dot из массива точек типа ArrayDots </param>
        /// <returns> список точек </returns>
        public List<Dot> NeiborDotsSNWE(Dot dot)//SNWE -S -South, N -North, W -West, E -East
        {
            Dot[] dts = new Dot[4] {
                                    this[dot.x + 1, dot.y],
                                    this[dot.x - 1, dot.y],
                                    this[dot.x, dot.y + 1],
                                    this[dot.x, dot.y - 1]
                                    };
            return dts.ToList();
        }
        public List<Dot> NeiborDotsAll(Dot dot)
        {
            Dot[] dts = new Dot[8] {
                                    this[dot.x + 1, dot.y],
                                    this[dot.x - 1, dot.y],
                                    this[dot.x, dot.y + 1],
                                    this[dot.x, dot.y - 1],
                                    this[dot.x + 1, dot.y + 1],
                                    this[dot.x - 1, dot.y - 1],
                                    this[dot.x - 1, dot.y + 1],
                                    this[dot.x + 1, dot.y - 1]
                                    };
            return dts.ToList();
        }
        public void UnmarkAllDots()
        {
            foreach (Dot d in _Dots) d.UnmarkDot();
        }
        public int MinX()
        {
            var q = from Dot d in _Dots where d.Own != 0 & d.Blocked == false select d;
            int minX = BoardWidth;
            foreach (Dot d in q)
            {
                if (minX > d.x) minX = d.x;
            }
            return minX;
        }
        public int MaxX()
        {
            var q = from Dot d in _Dots where d.Own != 0 & d.Blocked == false select d;
            int maxX = 0;
            foreach (Dot d in q)
            {
                if (maxX < d.x) maxX = d.x;
            }
            return maxX;
        }
        public int MaxY()
        {
            var q = from Dot d in _Dots where d.Own != 0 & d.Blocked == false select d;
            int maxY = 0;
            foreach (Dot d in q)
            {
                if (maxY < d.y) maxY = d.y;
            }
            return maxY;
        }
        public int MinY()
        {
            var q = from Dot d in _Dots where d.Own != 0 & d.Blocked == false select d;
            int minY = BoardHeight;
            foreach (Dot d in q)
            {
                if (minY > d.y) minY = d.y;
            }
            return minY;
        }
        public int CountNeibourDots(int Owner)//количество точек определенного цвета возле пустой точки
        {
            var q = from Dot d in _Dots
                    where d.Blocked == false & d.Own == 0 &
                    _Dots[IndexDot(d.x + 1, d.y -1)].Blocked == false & _Dots[IndexDot(d.x + 1, d.y -1)].Own == Owner & 
                    _Dots[IndexDot(d.x + 1, d.y + 1)].Blocked == false & _Dots[IndexDot(d.x + 1, d.y + 1)].Own == Owner
                    | d.Own == 0 & _Dots[IndexDot(d.x, d.y -1)].Blocked == false & _Dots[IndexDot(d.x, d.y -1)].Own == Owner & _Dots[IndexDot(d.x, d.y + 1)].Blocked == false & _Dots[IndexDot(d.x, d.y + 1)].Own == Owner
                    | d.Own == 0 & _Dots[IndexDot(d.x -1, d.y -1)].Blocked == false & _Dots[IndexDot(d.x -1, d.y -1)].Own == Owner & _Dots[IndexDot(d.x -1, d.y + 1)].Blocked == false & _Dots[IndexDot(d.x -1, d.y + 1)].Own == Owner
                    | d.Own == 0 & _Dots[IndexDot(d.x -1, d.y -1)].Blocked == false & _Dots[IndexDot(d.x -1, d.y -1)].Own == Owner & _Dots[IndexDot(d.x + 1, d.y + 1)].Blocked == false & _Dots[IndexDot(d.x + 1, d.y + 1)].Own == Owner
                    | d.Own == 0 & _Dots[IndexDot(d.x -1, d.y + 1)].Blocked == false & _Dots[IndexDot(d.x -1, d.y + 1)].Own == Owner & _Dots[IndexDot(d.x + 1, d.y -1)].Blocked == false & _Dots[IndexDot(d.x + 1, d.y -1)].Own == Owner
                    select d;
            return q.Count();
        }
        /// <summary>
        /// Вычисляет индекс точки в списке по ее координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int IndexDot(int x, int y)
        {

            int index = x * BoardHeight + y;

            return index;
        }
        /// <summary>
        /// Проверка, находится ли точка на игровой доске
        /// </summary>
        /// <returns></returns>
        public bool DotIndexCheck(int x, int y)
        {
            return (x >= 0 && x < BoardWidth &&
                    y >= 0 && y < BoardHeight);
        }
        /// <summary>
        /// Проверка, находится ли точка на игровой доске
        /// </summary>
        /// <returns></returns>
        public bool DotIndexCheck(Dot dot)
        {
            return dot==null? false : (dot.x >= 0 && dot.x < BoardWidth &&
                    dot.y >= 0 && dot.y < BoardHeight);
        }

        public List<Dot> Rotate90(List<Dot> DotsForRotation)
        {
            int x;
            int y;
            List<Dot> newList = new List<Dot>(DotsForRotation.Count);
            foreach (Dot d in DotsForRotation)
            {
                x = d.x; y = d.y;
                Dot dot = d.DotCopy; 
                dot.x = y;
                dot.y = x;
                newList.Add(dot);
            }
            RebuildDots();
            return newList;
        }
        public List<Dot> Rotate_Mirror_Horizontal(List<Dot> DotsForRotation)
        {
            int x;
            //int y;
            List<Dot> newList = new List<Dot>(DotsForRotation.Count);
            foreach (Dot d in DotsForRotation)
            {
                x = BoardWidth - d.x; 
                //d.x = x;
                Dot dot = d.DotCopy;
                dot.x = x;
                newList.Add(dot);
            }
            RebuildDots();
            return newList;
        }
        public List<Dot> EmptyNeibourDots(int Owner)//список не занятых точек возле определенной точки
        {
            List<Dot> ld = new List<Dot>();
            foreach (Dot d in _Dots)
            {
                if (d.Own == Owner)
                {
                    var q = from Dot dot in _Dots
                            where dot.Blocked == false & dot.Own == 0 & Distance(dot, d) < 2
                            select dot;
                    foreach (Dot empty_d in q)
                    {
                        if (ld.Contains(empty_d) == false) ld.Add(empty_d);
                    }
                }
            }
            return ld;
        }
        public int MakeIndexRelation(Dot dot)
        {
            if (dot.NeiborDots.Count > 0)
            {
                foreach (Dot d in dot.NeiborDots)
                {
                    if (dot.Blocked == false & dot.Own == d.Own) d.IndexRelation = dot.IndexRelation;
                }
            }
            else
            {
            }
            return dot.IndexRelation;
        }
        public void Clear()//Не очищает список, а заменяет точки на Own == 0
        {
            foreach (Dot d in _Dots)
            {
                d.Own = 0;
                d.Marked = false;
                d.Blocked = false;
                d.BlokingDots.Clear();
                d.Rating = 0;
            }
            ListLinks.Clear();
            ListMoves.Clear();

        }
        /// <summary>
        /// Пересканирует блокированные точки и устанавливает статус Blocked
        /// </summary>
        public void RescanBlockedDots()
        {
            //-------------------Rescan Blocked Dots------------------
            var dots_bl = from Dot d in Dots
                          where d.BlokingDots.Count > 0
                          select d.Blocked = true;

            //-------------------Rescan Blocked EmptyDots---------------------

            while ((from Dot d in Dots
                where d.Own == 0 && d.Blocked == false &&
                      d.x + 1 < BoardWidth && d.x - 1 > - 1 &&
                      d.y + 1 < BoardHeight && d.y - 1 > - 1 &&
                      this[d.x + 1, d.y].Blocked |
                      this[d.x - 1, d.y].Blocked |
                      this[d.x, d.y + 1].Blocked |
                      this[d.x, d.y - 1].Blocked
                select d.Blocked = true).Count()!=0);
        }
        /// <summary>
        /// проверяет заблокирована ли точка. Перед использованием функции надо установить flg_own
        /// </summary>
        /// <param name="dot">поверяемая точка</param>
        /// <param name="flg_own">владелец проверяемой точки, этот параметр нужен для рекурсии</param>
        /// <returns></returns>
        public bool DotIsFree(Dot dot, int flg_own) 
        {
            dot.Marked = true;
            if (dot.x == 0 | dot.y == 0 | dot.x == BoardWidth - 1 | dot.y == BoardHeight - 1)
            {
                return true;
            }
            Dot[] d = new Dot[4] { this[dot.x + 1, dot.y], this[dot.x - 1, dot.y], this[dot.x, dot.y + 1], this[dot.x, dot.y - 1] };
            //--------------------------------------------------------------------------------
            if (flg_own == 0)// если точка не принадлежит никому и рядом есть незаблокированные точки -эта точка считается свободной(незаблокированной)
            {
                var q = from Dot fd in d where fd.Blocked == false select fd;
                if (q.Count() > 0) return true;
            }
            //----------------------------------------------------------------------------------
            for (int i = 0; i < 4; i++)
            {
                if (d[i].Marked == false)
                {
                    if (d[i].Own == 0 | d[i].Own == flg_own | d[i].Own != flg_own 
                      & d[i].Blocked & d[i].BlokingDots.Contains(dot) == false)
                    {
                        if (DotIsFree(d[i], flg_own))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void RebuildDots()
        {
            GameDots _Dots = new GameDots(BoardWidth,BoardHeight);

            foreach (Dot dot in ListMoves)
            {
                _Dots.MakeMove(dot, addForDraw: true);
            }
            _Dots.RescanBlockedDots();
            Dots = _Dots.Dots;
            ListMoves = _Dots.ListMoves;
            //LinkDots();
            //DrawSession.DotsForDrawing = ListMoves.ToList();
        }
        public void UndoMove(Dot dot)//поле отмена хода
        {
            Remove(dot);
            RebuildDots();
        }
        private int count_in_region;
        private int count_blocked_dots;
        //=================================================================================================
        /// <summary>
        /// Функция делает ход игрока 
        /// </summary>
        /// <param name="dot">точка куда делается ход</param>
        /// <param name="Owner">владелец точки -целое 1-Игрок или 2 -Компьютер</param>
        /// <returns>количество окруженных точек или -1 если недопустимый ход</returns>
        public int MakeMove(Dot dot, int Owner = 0, bool addForDraw = false)//
        {
            
            if (this[dot.x, dot.y].ValidMove)
            {
                if (Owner != 0) dot.Own = Owner;
                Add(dot); //если точка не занята
            }
            else return -1;//в случае невозможного хода
            //--------------------------------
            int res = CheckBlocked(dot.Own);
            //--------------------------------
            count_blocked_dots = (from Dot d in Dots where d.Blocked select d).Count();
            if (addForDraw)
            {
                ListMoves.Add(_Dots[IndexDot(dot.x, dot.y)]);
                LinkDots();//перестроить связи точек
            }
            return res;
        }
        /// <summary>
        /// проверяет блокировку точек, маркирует точки которые блокируют, возвращает количество окруженных точек
        /// </summary>
        /// <param name="arrDots"></param>
        /// <param name="last_moveOwner"></param>
        /// <returns>количество окруженных точек</returns>
        private int CheckBlocked(int last_moveOwner = 0)
        {
            int counter = 0;
            var checkdots = from Dot dots in this
                            where dots.Own != 0 | dots.Own == 0 & dots.Blocked
                            orderby dots.Own == last_moveOwner
                            select dots;
            lst_blocked_dots.Clear(); lst_in_region_dots.Clear();
            foreach (Dot d in checkdots)
            {
                UnmarkAllDots();
                if (DotIsFree(d, d.Own) == false)
                //if (DotIsFree(d, arrDots) == false)
                {
                    if (d.Own != 0) d.Blocked = true;
                    d.IndexRelation = 0;
                    var q1 = from Dot dots in this where dots.BlokingDots.Contains(d) select dots;
                    if (q1.Count() == 0)
                    {
                        UnmarkAllDots();
                        MarkDotsInRegion(d, d.Own);

                        foreach (Dot dr in lst_in_region_dots)
                        {
                            win_player = dr.Own;
                            count_in_region++;
                            foreach (Dot bd in lst_blocked_dots)
                            {
                                if (bd.Own != 0) counter += 1;
                                if (dr.BlokingDots.Contains(bd) == false & bd.Own != 0 & dr.Own != bd.Own)
                                {
                                    dr.BlokingDots.Add(bd);
                                }
                            }
                        }
                    }
                }
                else
                {
                    d.Blocked = false;
                }
            }
            RescanBlockedDots();

            if (lst_blocked_dots.Count == 0) win_player = 0;
            return lst_blocked_dots.Count;
        }
        private List<Dot> lst_blocked_dots = new List<Dot>();//список блокированных точек
        private List<Dot> lst_in_region_dots = new List<Dot>();//список блокирующих точек
        /// <summary>
        /// Определяет блокирующие точки и устанавливает этим точкам поле InRegion=true 
        /// </summary>
        /// <param name="blocked_dot">точка, которая блокируется</param>
        /// <param name="flg_own">владелец точки</param>
        private void MarkDotsInRegion(Dot blocked_dot, int flg_own)
        {
            blocked_dot.Marked = true;
            Dot[] dts = new Dot[4] {
                                    this[blocked_dot.x + 1, blocked_dot.y],
                                    this[blocked_dot.x -1, blocked_dot.y],
                                    this[blocked_dot.x, blocked_dot.y + 1],
                                    this[blocked_dot.x, blocked_dot.y -1],
                                    };
            //добавим точки которые попали в окружение
            if (lst_blocked_dots.Contains(blocked_dot) == false)
            {
                lst_blocked_dots.Add(blocked_dot);
            }
            foreach (Dot _d in dts)
            //foreach (Dot _d in this.SetNeiborDotsSNWE(blocked_dot))
            {
                if (_d.Own != 0 & _d.Blocked == false & _d.Own != flg_own)//_d-точка которая окружает
                {
                    //добавим в коллекцию точки которые окружают
                    if (lst_in_region_dots.Contains(_d) == false) lst_in_region_dots.Add(_d);
                }
                else
                {
                    if (_d.Marked == false & _d.Fixed == false)
                    {
                        _d.Blocked = true;
                        MarkDotsInRegion(_d, flg_own);
                    }
                }
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void MakeRating()//возвращает массив вражеских точек вокруг заданной точки
        {
            int res;
            var qd = from Dot dt in this where dt.Own != 0 & dt.Blocked == false select dt;
            foreach (Dot dot in qd)
            {
                //if (dot.x > 0 & dot.y > 0 & dot.x < iMapSize -1 & dot.y < iMapSize -1)
                if (dot.x > 0 & dot.y > 0 & dot.x < BoardWidth - 1 & dot.y < BoardHeight - 1)
                {
                    Dot[] dts = new Dot[4] {this[dot.x + 1, dot.y], 
                                            this[dot.x -1, dot.y],
                                            this[dot.x, dot.y + 1],
                                            this[dot.x, dot.y -1]};
                    res = 0;
                    foreach (Dot item in dts)
                    {
                        if (item.Own != 0 & item.Own != dot.Own) res++;
                        else if (item.Own == dot.Own & item.Rating == 0)
                        {
                            res = -1;
                            break;
                        }
                    }
                    dot.Rating = res + 1;//точка без связей получает рейт 1
                }
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private List<Links> lnks;
        public List<Links> ListLinks 
        { 
            get {return lnks;}
        }
        public void LinkDots()//устанавливает связь между двумя точками и возвращает массив связей 
        {
            lnks.Clear();
            var qry = from Dot d1 in this
                      where d1.BlokingDots.Count > 0 
                      from Dot d2 in this
                      where d2.Own == d1.Own && d1.Blocked == d2.Blocked && d2.BlokingDots.Count > 0 
                      & Distance(d1, d2) < 2 & Distance(d1, d2) > 0
                      select new Links(d1, d2);

            var temp = qry.Distinct(new LinksComparer());

            lnks = temp.ToList(); //обновляем основной массив связей - lnks              
        }
        public List<Links> LinkDots(List<Dot> lstDots)//устанавливает связь между двумя точками и возвращает массив связей 
        {
            var qry = from Dot d1 in lstDots
                      where d1.BlokingDots.Count > 0
                      from Dot d2 in lstDots
                      where d2.Own == d1.Own && d1.Blocked == d2.Blocked && d2.BlokingDots.Count > 0
                      & Distance(d1, d2) < 2 & Distance(d1, d2) > 0
                      select new Links(d1, d2);

            var temp = qry.Distinct(new LinksComparer());
            List < Links > ll = temp.ToList();
            return ll;
        }

        /// <summary>
        /// функция проверяет не делается ли ход в точку, которая на следующем ходу будет окружена
        /// </summary>
        /// <param name="dot"></param>
        /// <param name="arrDots"></param>
        /// <param name="Player"></param>
        /// <returns></returns>
        public bool CheckDot(Dot dot, int Player)
        {
            int res = MakeMove(dot, Player);
            int pl = Player == 2 ? 1 : 2;
            if (win_player == pl)// первое условие -ход в уже оеруженный регион, 
            {
                UndoMove(dot);
                return true; // да будет окружена
            }
            //будет ли окружена на следующем ходу
            Dot dotEnemy = CheckMove(pl);
            if (dotEnemy != null)
            {
                res = MakeMove(dotEnemy, pl);
                UndoMove(dotEnemy);
                UndoMove(dot);
                return true; // да будет окружена
            }
            //нет не будет
            UndoMove(dot);
            return false;
        }
        //===============================================================================================================
        public List<Dot> CheckRelation(int index)
        {
            List<Dot> lstDots = new List<Dot>();
            Dot d1, d2;
            var q = from Dot dot in this
                    where dot.IndexDot == index & dot.NeiborDots.Count == 1
                    select dot;

            if (q.Count() == 2)
            {
                d1 = q.First();
                d2 = q.Last();
                var qry = from Dot dot in this
                          where dot.Own == 0 & this.Distance(dot, d1) < 2 & this.Distance(dot, d2) < 2
                          select dot;
                return qry.ToList();
            }
            return null;
            //return lstDots;
        }
        //==============================================================================================
        //проверяет ход в результате которого окружение.Возвращает ход который завершает окружение
        public Dot CheckMove(int Owner)
        {
            List<Dot> happy_dots = new List<Dot>();
            var qry = Board_ValidMoves.Where(
#region Query patterns Check

                         d =>
                         //     d.x + 1 < BoardWidth && d.x - 1 > -1 &&
                         //     d.y + 1 < BoardHeight && d.y - 1 > -1 &&
                                //       + 
                                //    d  
                                //       +
                          this[d.x + 1, d.y - 1].Blocked == false & this[d.x + 1, d.y + 1].Blocked == false &
                          this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y + 1].Own == Owner &
                          this[d.x + 1, d.y].Own != Owner
                                //+        
                                //   d     
                                //+       
                          | this[d.x - 1, d.y - 1].Blocked == false & this[d.x - 1, d.y + 1].Blocked == false &
                          this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y + 1].Own == Owner &
                          this[d.x - 1, d.y].Own != Owner
                                //      
                                //   d          
                                //+     +
                        | this[d.x + 1, d.y + 1].Blocked == false & this[d.x - 1, d.y + 1].Blocked == false &
                        this[d.x + 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Own == Owner &
                        this[d.x, d.y + 1].Own != Owner
                                //+     + 
                                //   d          
                                //     
                        | this[d.x - 1, d.y - 1].Blocked == false & this[d.x + 1, d.y - 1].Blocked == false &
                         this[d.x - 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Own == Owner &
                         this[d.x, d.y - 1].Own != Owner

                         //    +    
                                //    d   
                                //    +   
                          | this[d.x, d.y + 1].Blocked == false & this[d.x, d.y - 1].Blocked == false &
                              this[d.x, d.y - 1].Own == Owner & this[d.x, d.y + 1].Own == Owner &
                              this[d.x + 1, d.y].Own != Owner &
                              this[d.x - 1, d.y].Own != Owner
                                //        
                                //+   d   +  
                                //       
                          | this[d.x - 1, d.y].Blocked == false & this[d.x + 1, d.y].Blocked == false &
                            this[d.x - 1, d.y].Own == Owner & this[d.x + 1, d.y].Own == Owner &
                            this[d.x, d.y + 1].Own != Owner &
                            this[d.x, d.y - 1].Own != Owner

                              //+        
                                //   d     
                                //      +   
                          | this[d.x - 1, d.y - 1].Blocked == false & this[d.x + 1, d.y + 1].Blocked == false &
                          this[d.x - 1, d.y - 1].Own == Owner & this[d.x + 1, d.y + 1].Own == Owner &
                          this[d.x, d.y - 1].Own != Owner &
                          this[d.x, d.y + 1].Own != Owner &
                          this[d.x + 1, d.y].Own != Owner &
                          this[d.x - 1, d.y].Own != Owner
                                //      +  
                                //   d     
                                //+        
                          | this[d.x - 1, d.y + 1].Blocked == false & this[d.x + 1, d.y - 1].Blocked == false &
                          this[d.x - 1, d.y + 1].Own == Owner & this[d.x + 1, d.y - 1].Own == Owner &
                          this[d.x, d.y - 1].Own != Owner &
                          this[d.x, d.y + 1].Own != Owner &
                          this[d.x + 1, d.y].Own != Owner &
                          this[d.x - 1, d.y].Own != Owner

                               //      +
                                //+  d
                        | this[d.x - 1, d.y].Blocked == false & this[d.x + 1, d.y - 1].Blocked == false &
                          this[d.x - 1, d.y].Own == Owner & this[d.x + 1, d.y - 1].Own == Owner &
                         this[d.x, d.y - 1].Own != Owner &
                         this[d.x + 1, d.y].Own != Owner &
                         this[d.x - 1, d.y - 1].Own != Owner &
                         this[d.x + 1, d.y + 1].Own != Owner &
                          this[d.x, d.y + 1].Own != Owner

                                //+  d
                                //      +
                        | this[d.x - 1, d.y].Blocked == false & this[d.x + 1, d.y + 1].Blocked == false &
                          this[d.x - 1, d.y].Own == Owner & this[d.x + 1, d.y + 1].Own == Owner &
                          this[d.x, d.y + 1].Own != Owner &
                         this[d.x + 1, d.y].Own != Owner &
                         this[d.x - 1, d.y + 1].Own != Owner &
                          this[d.x + 1, d.y - 1].Own != Owner &
                          this[d.x, d.y - 1].Own != Owner

                              //+
                                //   d  +       
                        | this[d.x + 1, d.y].Blocked == false & this[d.x - 1, d.y - 1].Blocked == false &
                         this[d.x + 1, d.y].Own == Owner & this[d.x - 1, d.y - 1].Own == Owner &
                         this[d.x, d.y - 1].Own != Owner &
                         this[d.x - 1, d.y + 1].Own != Owner &
                         this[d.x - 1, d.y].Own != Owner &
                         this[d.x + 1, d.y - 1].Own != Owner &
                          this[d.x, d.y + 1].Own != Owner

                               //   d  +       
                                //+
                        | this[d.x + 1, d.y].Blocked == false & this[d.x - 1, d.y + 1].Blocked == false &
                         this[d.x + 1, d.y].Own == Owner & this[d.x - 1, d.y + 1].Own == Owner &
                         this[d.x, d.y + 1].Own != Owner &
                         this[d.x + 1, d.y + 1].Own != Owner &
                         this[d.x - 1, d.y].Own != Owner &
                         this[d.x - 1, d.y - 1].Own != Owner &
                          this[d.x, d.y - 1].Own != Owner

                               //+   
                                //   d          
                                //   +
                        | this[d.x, d.y + 1].Blocked == false & this[d.x - 1, d.y - 1].Blocked == false &
                         this[d.x, d.y + 1].Own == Owner & this[d.x - 1, d.y - 1].Own == Owner &
                         this[d.x - 1, d.y].Own != Owner &
                         this[d.x - 1, d.y + 1].Own != Owner &
                         this[d.x, d.y - 1].Own != Owner &
                         this[d.x + 1, d.y - 1].Own != Owner &
                          this[d.x + 1, d.y].Own != Owner

                               //   +
                                //   d          
                                //+   
                        | this[d.x, d.y - 1].Blocked == false & this[d.x - 1, d.y + 1].Blocked == false &
                        this[d.x, d.y - 1].Own == Owner & this[d.x - 1, d.y + 1].Own == Owner &
                        this[d.x - 1, d.y].Own != Owner &
                        this[d.x - 1, d.y - 1].Own != Owner &
                        this[d.x + 1, d.y].Own != Owner &
                        this[d.x + 1, d.y + 1].Own != Owner &
                        this[d.x, d.y + 1].Own != Owner

                               //   +
                                //   d          
                                //      +
                        | this[d.x, d.y - 1].Blocked == false & this[d.x + 1, d.y + 1].Blocked == false &
                        this[d.x, d.y - 1].Own == Owner & this[d.x + 1, d.y + 1].Own == Owner &
                        this[d.x + 1, d.y].Own != Owner &
                        this[d.x, d.y + 1].Own != Owner &
                        this[d.x - 1, d.y + 1].Own != Owner &
                        this[d.x - 1, d.y].Own != Owner &
                        this[d.x + 1, d.y - 1].Own != Owner

                                //      +
                                //   d          
                                //   +   
                        | this[d.x, d.y + 1].Blocked == false & this[d.x + 1, d.y - 1].Blocked == false &
                        this[d.x, d.y + 1].Own == Owner & this[d.x + 1, d.y - 1].Own == Owner &
                        this[d.x - 1, d.y - 1].Own != Owner &
                        this[d.x, d.y - 1].Own != Owner &
                        this[d.x + 1, d.y].Own != Owner &
                        this[d.x + 1, d.y + 1].Own != Owner &
                        this[d.x - 1, d.y].Own != Owner
#endregion
);
#if DEBUG
            Dot[] ad = qry.ToArray();
#endif
            foreach (Dot d in qry)
            {
                //делаем ход
                int result_last_move = MakeMove(d, Owner);
#if DEBUG
                //if (f.chkMove.Checked) Pause();
#endif
                //-----------------------------------
                if (result_last_move != 0 & this[d.x, d.y].Blocked == false)
                {
                    UndoMove(d);
                    d.CountBlockedDots = result_last_move;
                    happy_dots.Add(d);
                    //return d;
                }
                UndoMove(d);
            }

            //выбрать точку, которая максимально окружит
            var x = happy_dots.Where(dd => 
                    dd.CountBlockedDots == happy_dots.Max(dt => dt.CountBlockedDots));

            return x.Count() > 0 ? x.First() : null;

        }
        public Dot CheckPatternVilkaNextMove(int Owner)
        {
            var qry = Board_NotEmptyNonBlockedDots.Where(dt => dt.Own == Owner);
            Dot dot_ptn;
            if (qry.Count() != 0)
            {
                foreach (Dot d in qry)
                {
                    foreach (Dot dot_move in NeiborDotsSNWE(d))
                    {
                        if (dot_move.ValidMove)
                        {
                            //делаем ход
                            int result_last_move = MakeMove(dot_move, Owner);
                            int pl = Owner == 2 ? 1 : 2;
                            Dot dt = CheckMove(pl); // проверка чтобы не попасть в капкан
                            if (dt != null)
                            {
                                UndoMove(dot_move);
                                continue;
                            }
                            dot_ptn = CheckPattern_vilochka(d.Own);
#if DEBUG
                            //if (f.chkMove.Checked) Pause();
#endif
                            //-----------------------------------
                            if (dot_ptn != null & result_last_move == 0)
                            {
                                UndoMove(dot_move);
                                return dot_move;
                                //return dot_ptn;
                            }
                            UndoMove(dot_move);
                        }
                    }
                }
            }
            return null;
        }
        public int iNumberPattern;
        public Dot CheckPattern_vilochka(int Owner)
        {
            int enemy_own = Owner == 1 ? 2 : 1;
            //ArrayDots this = Dots.CopyArrayDots;

            var get_non_blocked = from Dot d in this where d.Blocked == false select d; //получить коллекцию незаблокированных точек

            //паттерн на диагональное расположение точек         *d   
            //                                                *  +  
            //                                             *  +  *               
            iNumberPattern = 1;

            var pat1 = from Dot d in get_non_blocked
                       where d.Own == Owner &&
                           this[d.x - 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                           & this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                           & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                           & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                           & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                           & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                           & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                           & this[d.x + 2, d.y + 1].Own != enemy_own & this[d.x + 2, d.y + 1].Blocked == false
                           & this[d.x + 1, d.y + 2].Own != enemy_own & this[d.x + 1, d.y + 2].Blocked == false
                       select d;
            if (pat1.Count() > 0) return new Dot(pat1.First().x + 1, pat1.First().y + 1);
            //паттерн на диагональное расположение точек    *  +  *d   
            //                                              +  *    
            //                                              *       

            var pat1_2 = from Dot d in get_non_blocked
                         where d.Own == Owner &&
                             this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                             & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                             & this[d.x - 2, d.y - 1].Own != enemy_own & this[d.x - 2, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y - 2].Own != enemy_own & this[d.x - 1, d.y - 2].Blocked == false
                         select d;
            if (pat1_2.Count() > 0) return new Dot(pat1_2.First().x - 1, pat1_2.First().y - 1);

            //============================================================================================================== 
            //паттерн на диагональное расположение точек    *                   
            //                                              +  d    
            //                                              m  +  * 
            var pat1_3 = from Dot d in get_non_blocked
                         where d.Own == Owner &&
                             this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                             //& this[d.x + 2, d.y + 1].Own != enemy_own & this[d.x + 2, d.y + 1].Blocked == false
                             //& this[d.x + 1, d.y].Own != enemy_own & this[d.x + 1, d.y].Blocked == false
                             //& this[d.x, d.y -1].Own != enemy_own & this[d.x, d.y -1].Blocked == false
                             //& this[d.x -1, d.y -2].Own != enemy_own & this[d.x -1, d.y -2].Blocked == false
                             & this[d.x - 1, d.y + 2].Own != enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                             & this[d.x - 2, d.y + 1].Own != enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                         select d;
            if (pat1_3.Count() > 0) return new Dot(pat1_3.First().x - 1, pat1_3.First().y + 1);
            //180 Rotate=========================================================================================================== 
            //паттерн на диагональное расположение точек    *  +  m   
            //                                                 d  +  
            //                                                    * 
            var pat1_4 = from Dot d in get_non_blocked
                         where d.Own == Owner &&
                             this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                             & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             //& this[d.x -2, d.y -1].Own != enemy_own & this[d.x -2, d.y -1].Blocked == false
                             //& this[d.x -1, d.y].Own != enemy_own & this[d.x -1, d.y].Blocked == false
                             //& this[d.x, d.y + 1].Own != enemy_own & this[d.x, d.y + 1].Blocked == false
                             //& this[d.x + 1, d.y + 2].Own != enemy_own & this[d.x + 1, d.y + 2].Blocked == false
                             & this[d.x + 1, d.y - 2].Own != enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                             & this[d.x + 2, d.y - 1].Own != enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                         select d;
            if (pat1_4.Count() > 0) return new Dot(pat1_4.First().x + 1, pat1_4.First().y - 1);
            //**********************************************************************************        

            //     *     *
            //  *  +  *  +   
            //  *        m
            //     * 
            iNumberPattern = 938;
            var pat938 = from Dot d in get_non_blocked
                         where d.Own == Owner &&
                             this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                             & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y].Own == Owner & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 2, d.y + 1].Own == Owner & this[d.x - 2, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 2].Own == Owner & this[d.x - 1, d.y + 2].Blocked == false
                             & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y + 2].Own == 0 & this[d.x + 1, d.y + 2].Blocked == false
                             & this[d.x + 2, d.y + 1].Own == 0 & this[d.x + 2, d.y + 1].Blocked == false
                         select d;
            if (pat938.Count() > 0) return new Dot(pat938.First().x + 1, pat938.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat938_2 = from Dot d in get_non_blocked
                           where d.Own == Owner &&
                               this[d.x - 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                               & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                               & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                               & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y].Own == Owner & this[d.x + 1, d.y].Blocked == false
                               & this[d.x + 2, d.y - 1].Own == Owner & this[d.x + 2, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y - 2].Own == Owner & this[d.x + 1, d.y - 2].Blocked == false
                               & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                               & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                           select d;
            if (pat938_2.Count() > 0) return new Dot(pat938_2.First().x - 1, pat938_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat938_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner &&
                                 d.x + 1 < BoardWidth && d.x - 1 > -1 &&
                                 d.y + 1 < BoardHeight && d.y - 1 > -1 &&
                                 this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                 & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                 & this[d.x, d.y + 1].Own == Owner & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y + 2].Own == Owner & this[d.x - 1, d.y + 2].Blocked == false
                                 & this[d.x - 2, d.y + 1].Own == Owner & this[d.x - 2, d.y + 1].Blocked == false
                                 & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                             select d;
            if (pat938_2_3.Count() > 0) return new Dot(pat938_2_3.First().x - 1, pat938_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat938_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x - 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                                   & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                   & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x, d.y - 1].Own == Owner & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y - 2].Own == Owner & this[d.x + 1, d.y - 2].Blocked == false
                                   & this[d.x + 2, d.y - 1].Own == Owner & this[d.x + 2, d.y - 1].Blocked == false
                                   & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x + 2, d.y + 1].Own == 0 & this[d.x + 2, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 2].Own == 0 & this[d.x + 1, d.y + 2].Blocked == false
                               select d;
            if (pat938_2_3_4.Count() > 0) return new Dot(pat938_2_3_4.First().x + 1, pat938_2_3_4.First().y + 1);
            //============================================================================================================== 

            //===========ВИЛОЧКА=================================================================================================== 
            //     *   
            //     +  
            //  +  *
            //  *
            iNumberPattern = 670;
            var pat670 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x, d.y + 2].Own == Owner
                             & this[d.x, d.y + 1].Own == enemy_own
                             & this[d.x + 1, d.y - 1].Own == Owner
                             & this[d.x + 1, d.y].Own == enemy_own
                             & this[d.x + 1, d.y + 1].Own == 0
                             & this[d.x + 2, d.y].Own == 0
                             & this[d.x - 1, d.y + 1].Own == 0
                             & this[d.x + 2, d.y - 1].Own != enemy_own
                             & this[d.x, d.y - 1].Own != enemy_own
                             & this[d.x + 2, d.y + 1].Own != enemy_own
                             & this[d.x + 1, d.y + 2].Own != enemy_own
                             & this[d.x - 1, d.y].Own != enemy_own
                         select d;
            if (pat670.Count() > 0) return new Dot(pat670.First().x + 1, pat670.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat670_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x, d.y - 2].Own == Owner
                               & this[d.x, d.y - 1].Own == enemy_own
                               & this[d.x - 1, d.y + 1].Own == Owner
                               & this[d.x - 1, d.y].Own == enemy_own
                               & this[d.x - 1, d.y - 1].Own == 0
                               & this[d.x - 2, d.y].Own == 0
                               & this[d.x + 1, d.y - 1].Own == 0
                               & this[d.x - 2, d.y + 1].Own != enemy_own
                               & this[d.x, d.y + 1].Own != enemy_own
                               & this[d.x - 2, d.y - 1].Own != enemy_own
                               & this[d.x - 1, d.y - 2].Own != enemy_own
                               & this[d.x + 1, d.y].Own != enemy_own
                           select d;
            if (pat670_2.Count() > 0) return new Dot(pat670_2.First().x - 1, pat670_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat670_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x - 2, d.y].Own == Owner
                                 & this[d.x - 1, d.y].Own == enemy_own
                                 & this[d.x + 1, d.y - 1].Own == Owner
                                 & this[d.x, d.y - 1].Own == enemy_own
                                 & this[d.x - 1, d.y - 1].Own == 0
                                 & this[d.x, d.y - 2].Own == 0
                                 & this[d.x - 1, d.y + 1].Own == 0
                                 & this[d.x + 1, d.y - 2].Own != enemy_own
                                 & this[d.x + 1, d.y].Own != enemy_own
                                 & this[d.x - 1, d.y - 2].Own != enemy_own
                                 & this[d.x - 2, d.y - 1].Own != enemy_own
                                 & this[d.x, d.y + 1].Own != enemy_own
                             select d;
            if (pat670_2_3.Count() > 0) return new Dot(pat670_2_3.First().x - 1, pat670_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat670_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x + 2, d.y].Own == Owner
                                   & this[d.x + 1, d.y].Own == enemy_own
                                   & this[d.x - 1, d.y + 1].Own == Owner
                                   & this[d.x, d.y + 1].Own == enemy_own
                                   & this[d.x + 1, d.y + 1].Own == 0
                                   & this[d.x, d.y + 2].Own == 0
                                   & this[d.x + 1, d.y - 1].Own == 0
                                   & this[d.x - 1, d.y + 2].Own != enemy_own
                                   & this[d.x - 1, d.y].Own != enemy_own
                                   & this[d.x + 1, d.y + 2].Own != enemy_own
                                   & this[d.x + 2, d.y + 1].Own != enemy_own
                                   & this[d.x, d.y - 1].Own != enemy_own
                               select d;
            if (pat670_2_3_4.Count() > 0) return new Dot(pat670_2_3_4.First().x + 1, pat670_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 30;
            var pat30 = from Dot d in get_non_blocked
                        where d.Own == Owner
                            & this[d.x - 1, d.y + 1].Own == Owner
                            & this[d.x - 1, d.y].Own == enemy_own
                            & this[d.x, d.y - 1].Own == enemy_own
                            & this[d.x, d.y - 2].Own == Owner
                            & this[d.x + 1, d.y - 1].Own == 0
                            & this[d.x - 1, d.y - 1].Own == 0
                            & this[d.x - 1, d.y - 2].Own == 0
                            & this[d.x - 2, d.y - 1].Own == 0
                            & this[d.x - 2, d.y].Own == 0
                        select d;
            if (pat30.Count() > 0) return new Dot(pat30.First().x - 1, pat30.First().y - 1);
            //180 Rotate=========================================================================================================== 
            var pat30_2 = from Dot d in get_non_blocked
                          where d.Own == Owner
                              & this[d.x + 1, d.y - 1].Own == Owner
                              & this[d.x + 1, d.y].Own == enemy_own
                              & this[d.x, d.y + 1].Own == enemy_own
                              & this[d.x, d.y + 2].Own == Owner
                              & this[d.x - 1, d.y + 1].Own == 0
                              & this[d.x + 1, d.y + 1].Own == 0
                              & this[d.x + 1, d.y + 2].Own == 0
                              & this[d.x + 2, d.y + 1].Own == 0
                              & this[d.x + 2, d.y].Own == 0
                          select d;
            if (pat30_2.Count() > 0) return new Dot(pat30_2.First().x + 1, pat30_2.First().y + 1);
            //--------------Rotate on 90-----------------------------------
            var pat30_2_3 = from Dot d in get_non_blocked
                            where d.Own == Owner
                                & this[d.x - 1, d.y + 1].Own == Owner
                                & this[d.x, d.y + 1].Own == enemy_own
                                & this[d.x + 1, d.y].Own == enemy_own
                                & this[d.x + 2, d.y].Own == Owner
                                & this[d.x + 1, d.y - 1].Own == 0
                                & this[d.x + 1, d.y + 1].Own == 0
                                & this[d.x + 2, d.y + 1].Own == 0
                                & this[d.x + 1, d.y + 2].Own == 0
                                & this[d.x, d.y + 2].Own == 0
                            select d;
            if (pat30_2_3.Count() > 0) return new Dot(pat30_2_3.First().x + 1, pat30_2_3.First().y + 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat30_2_3_4 = from Dot d in get_non_blocked
                              where d.Own == Owner
                                  & this[d.x + 1, d.y - 1].Own == Owner
                                  & this[d.x, d.y - 1].Own == enemy_own
                                  & this[d.x - 1, d.y].Own == enemy_own
                                  & this[d.x - 2, d.y].Own == Owner
                                  & this[d.x - 1, d.y + 1].Own == 0
                                  & this[d.x - 1, d.y - 1].Own == 0
                                  & this[d.x - 2, d.y - 1].Own == 0
                                  & this[d.x - 1, d.y - 2].Own == 0
                                  & this[d.x, d.y - 2].Own == 0
                              select d;
            if (pat30_2_3_4.Count() > 0) return new Dot(pat30_2_3_4.First().x - 1, pat30_2_3_4.First().y - 1);
            //============================================================================================================== 
            iNumberPattern = 295;
            var pat295 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x - 1, d.y].Own == enemy_own
                             & this[d.x, d.y + 1].Own == enemy_own
                             & this[d.x, d.y + 2].Own == enemy_own
                             & this[d.x + 1, d.y + 3].Own == enemy_own
                             & this[d.x + 2, d.y + 2].Own == enemy_own
                             & this[d.x, d.y - 1].Own == 0
                             & this[d.x + 1, d.y - 1].Own == 0
                             & this[d.x + 1, d.y].Own == 0
                             & this[d.x + 2, d.y].Own == 0
                             & this[d.x + 2, d.y + 1].Own == 0
                             & this[d.x + 1, d.y + 1].Own != enemy_own
                             & this[d.x + 1, d.y + 2].Own != enemy_own
                         select d;
            if (pat295.Count() > 0) return new Dot(pat295.First().x + 1, pat295.First().y);
            //180 Rotate=========================================================================================================== 
            var pat295_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x + 1, d.y].Own == enemy_own
                               & this[d.x, d.y - 1].Own == enemy_own
                               & this[d.x, d.y - 2].Own == enemy_own
                               & this[d.x - 1, d.y - 3].Own == enemy_own
                               & this[d.x - 2, d.y - 2].Own == enemy_own
                               & this[d.x, d.y + 1].Own == 0
                               & this[d.x - 1, d.y + 1].Own == 0
                               & this[d.x - 1, d.y].Own == 0
                               & this[d.x - 2, d.y].Own == 0
                               & this[d.x - 2, d.y - 1].Own == 0
                               & this[d.x - 1, d.y - 1].Own != enemy_own
                               & this[d.x - 1, d.y - 2].Own != enemy_own
                           select d;
            if (pat295_2.Count() > 0) return new Dot(pat295_2.First().x - 1, pat295_2.First().y);
            //--------------Rotate on 90-----------------------------------
            var pat295_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x, d.y + 1].Own == enemy_own
                                 & this[d.x - 1, d.y].Own == enemy_own
                                 & this[d.x - 2, d.y].Own == enemy_own
                                 & this[d.x - 3, d.y - 1].Own == enemy_own
                                 & this[d.x - 2, d.y - 2].Own == enemy_own
                                 & this[d.x + 1, d.y].Own == 0
                                 & this[d.x + 1, d.y - 1].Own == 0
                                 & this[d.x, d.y - 1].Own == 0
                                 & this[d.x, d.y - 2].Own == 0
                                 & this[d.x - 1, d.y - 2].Own == 0
                                 & this[d.x - 1, d.y - 1].Own != enemy_own
                                 & this[d.x - 2, d.y - 1].Own != enemy_own
                             select d;
            if (pat295_2_3.Count() > 0) return new Dot(pat295_2_3.First().x, pat295_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat295_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x, d.y - 1].Own == enemy_own
                                   & this[d.x + 1, d.y].Own == enemy_own
                                   & this[d.x + 2, d.y].Own == enemy_own
                                   & this[d.x + 3, d.y + 1].Own == enemy_own
                                   & this[d.x + 2, d.y + 2].Own == enemy_own
                                   & this[d.x - 1, d.y].Own == 0
                                   & this[d.x - 1, d.y + 1].Own == 0
                                   & this[d.x, d.y + 1].Own == 0
                                   & this[d.x, d.y + 2].Own == 0
                                   & this[d.x + 1, d.y + 2].Own == 0
                                   & this[d.x + 1, d.y + 1].Own != enemy_own
                                   & this[d.x + 2, d.y + 1].Own != enemy_own
                               select d;
            if (pat295_2_3_4.Count() > 0) return new Dot(pat295_2_3_4.First().x, pat295_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 968;
            var pat968 = from Dot d in get_non_blocked
                         where d.Own == Owner
& this[d.x + 1, d.y - 2].Own == Owner
& this[d.x + 2, d.y - 1].Own == 0
& this[d.x + 2, d.y].Own == 0
& this[d.x + 2, d.y + 1].Own == 0
& this[d.x + 1, d.y + 2].Own == Owner
& this[d.x + 1, d.y + 1].Own == enemy_own
& this[d.x, d.y + 1].Own == Owner
& this[d.x, d.y - 1].Own == Owner
& this[d.x + 1, d.y - 1].Own == enemy_own
& this[d.x + 1, d.y].Own == 0
                         select d;
            if (pat968.Count() > 0) return new Dot(pat968.First().x + 1, pat968.First().y);
            //180 Rotate=========================================================================================================== 
            var pat968_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
& this[d.x - 1, d.y + 2].Own == Owner
& this[d.x - 2, d.y + 1].Own == 0
& this[d.x - 2, d.y].Own == 0
& this[d.x - 2, d.y - 1].Own == 0
& this[d.x - 1, d.y - 2].Own == Owner
& this[d.x - 1, d.y - 1].Own == enemy_own
& this[d.x, d.y - 1].Own == Owner
& this[d.x, d.y + 1].Own == Owner
& this[d.x - 1, d.y + 1].Own == enemy_own
& this[d.x - 1, d.y].Own == 0
                           select d;
            if (pat968_2.Count() > 0) return new Dot(pat968_2.First().x - 1, pat968_2.First().y);
            //--------------Rotate on 90-----------------------------------
            var pat968_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
& this[d.x + 2, d.y - 1].Own == Owner
& this[d.x + 1, d.y - 2].Own == 0
& this[d.x, d.y - 2].Own == 0
& this[d.x - 1, d.y - 2].Own == 0
& this[d.x - 2, d.y - 1].Own == Owner
& this[d.x - 1, d.y - 1].Own == enemy_own
& this[d.x - 1, d.y].Own == Owner
& this[d.x + 1, d.y].Own == Owner
& this[d.x + 1, d.y - 1].Own == enemy_own
& this[d.x, d.y - 1].Own == 0
                             select d;
            if (pat968_2_3.Count() > 0) return new Dot(pat968_2_3.First().x, pat968_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat968_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
& this[d.x - 2, d.y + 1].Own == Owner
& this[d.x - 1, d.y + 2].Own == 0
& this[d.x, d.y + 2].Own == 0
& this[d.x + 1, d.y + 2].Own == 0
& this[d.x + 2, d.y + 1].Own == Owner
& this[d.x + 1, d.y + 1].Own == enemy_own
& this[d.x + 1, d.y].Own == Owner
& this[d.x - 1, d.y].Own == Owner
& this[d.x - 1, d.y + 1].Own == enemy_own
& this[d.x, d.y + 1].Own == 0
                               select d;
            if (pat968_2_3_4.Count() > 0) return new Dot(pat968_2_3_4.First().x, pat968_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 880;
            var pat880 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x + 1, d.y + 2].Own == enemy_own & this[d.x + 1, d.y + 2].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == enemy_own & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                             & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 2, d.y + 1].Own == 0 & this[d.x + 2, d.y + 1].Blocked == false
                             & this[d.x - 2, d.y].Own == enemy_own & this[d.x - 2, d.y].Blocked == false
                             & this[d.x - 2, d.y + 1].Own == enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 2].Own == enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                             & this[d.x - 1, d.y].Own != enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                         select d;
            if (pat880.Count() > 0) return new Dot(pat880.First().x + 1, pat880.First().y - 1);
            //180 Rotate=========================================================================================================== 
            var pat880_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x - 1, d.y - 2].Own == enemy_own & this[d.x - 1, d.y - 2].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y + 1].Own == enemy_own & this[d.x + 1, d.y + 1].Blocked == false
                               & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                               & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                               & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                               & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                               & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                               & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                               & this[d.x + 2, d.y].Own == enemy_own & this[d.x + 2, d.y].Blocked == false
                               & this[d.x + 2, d.y - 1].Own == enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y - 2].Own == enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                               & this[d.x + 1, d.y].Own != enemy_own & this[d.x + 1, d.y].Blocked == false
                               & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                           select d;
            if (pat880_2.Count() > 0) return new Dot(pat880_2.First().x - 1, pat880_2.First().y + 1);
            //--------------Rotate on 90-----------------------------------
            var pat880_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x - 2, d.y - 1].Own == enemy_own & this[d.x - 2, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x + 1, d.y + 1].Own == enemy_own & this[d.x + 1, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                 & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                 & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                                 & this[d.x, d.y + 2].Own == enemy_own & this[d.x, d.y + 2].Blocked == false
                                 & this[d.x - 1, d.y + 2].Own == enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                                 & this[d.x - 2, d.y + 1].Own == enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                                 & this[d.x, d.y + 1].Own != enemy_own & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                             select d;
            if (pat880_2_3.Count() > 0) return new Dot(pat880_2_3.First().x + 1, pat880_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat880_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x + 2, d.y + 1].Own == enemy_own & this[d.x + 2, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x - 1, d.y - 1].Own == enemy_own & this[d.x - 1, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                                   & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                   & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                   & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 2].Own == 0 & this[d.x + 1, d.y + 2].Blocked == false
                                   & this[d.x, d.y - 2].Own == enemy_own & this[d.x, d.y - 2].Blocked == false
                                   & this[d.x + 1, d.y - 2].Own == enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                                   & this[d.x + 2, d.y - 1].Own == enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 1].Own != enemy_own & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                               select d;
            if (pat880_2_3_4.Count() > 0) return new Dot(pat880_2_3_4.First().x - 1, pat880_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 775;
            var pat775 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x - 2, d.y].Own == Owner & this[d.x - 2, d.y].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                             & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                             & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y].Own != enemy_own & this[d.x + 1, d.y].Blocked == false
                         select d;
            if (pat775.Count() > 0) return new Dot(pat775.First().x - 1, pat775.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat775_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x + 2, d.y].Own == Owner & this[d.x + 2, d.y].Blocked == false
                               & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                               & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                               & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                               & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                               & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                               & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                               & this[d.x - 1, d.y].Own != enemy_own & this[d.x - 1, d.y].Blocked == false
                           select d;
            if (pat775_2.Count() > 0) return new Dot(pat775_2.First().x + 1, pat775_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat775_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x, d.y + 2].Own == Owner & this[d.x, d.y + 2].Blocked == false
                                 & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                                 & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                 & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                 & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                 & this[d.x, d.y - 1].Own != enemy_own & this[d.x, d.y - 1].Blocked == false
                             select d;
            if (pat775_2_3.Count() > 0) return new Dot(pat775_2_3.First().x - 1, pat775_2_3.First().y + 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat775_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x, d.y - 2].Own == Owner & this[d.x, d.y - 2].Blocked == false
                                   & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                   & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                   & this[d.x, d.y + 1].Own != enemy_own & this[d.x, d.y + 1].Blocked == false
                               select d;
            if (pat775_2_3_4.Count() > 0) return new Dot(pat775_2_3_4.First().x + 1, pat775_2_3_4.First().y - 1);
            //============================================================================================================== 
            iNumberPattern = 685;
            var pat685 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x - 2, d.y + 3].Own == Owner & this[d.x - 2, d.y + 3].Blocked == false
                             & this[d.x - 1, d.y + 2].Own == Owner & this[d.x - 1, d.y + 2].Blocked == false
                             & this[d.x, d.y + 3].Own == Owner & this[d.x, d.y + 3].Blocked == false
                             & this[d.x + 1, d.y + 2].Own == Owner & this[d.x + 1, d.y + 2].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                             & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                             & this[d.x - 3, d.y + 2].Own == 0 & this[d.x - 3, d.y + 2].Blocked == false
                             & this[d.x - 3, d.y + 1].Own == 0 & this[d.x - 3, d.y + 1].Blocked == false
                             & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                             & this[d.x - 1, d.y + 1].Own == enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                         select d;
            if (pat685.Count() > 0) return new Dot(pat685.First().x - 2, pat685.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat685_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x + 2, d.y - 3].Own == Owner & this[d.x + 2, d.y - 3].Blocked == false
                               & this[d.x + 1, d.y - 2].Own == Owner & this[d.x + 1, d.y - 2].Blocked == false
                               & this[d.x, d.y - 3].Own == Owner & this[d.x, d.y - 3].Blocked == false
                               & this[d.x - 1, d.y - 2].Own == Owner & this[d.x - 1, d.y - 2].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                               & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                               & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                               & this[d.x + 3, d.y - 2].Own == 0 & this[d.x + 3, d.y - 2].Blocked == false
                               & this[d.x + 3, d.y - 1].Own == 0 & this[d.x + 3, d.y - 1].Blocked == false
                               & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                               & this[d.x + 1, d.y - 1].Own == enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                           select d;
            if (pat685_2.Count() > 0) return new Dot(pat685_2.First().x + 2, pat685_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat685_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x - 3, d.y + 2].Own == Owner & this[d.x - 3, d.y + 2].Blocked == false
                                 & this[d.x - 2, d.y + 1].Own == Owner & this[d.x - 2, d.y + 1].Blocked == false
                                 & this[d.x - 3, d.y].Own == Owner & this[d.x - 3, d.y].Blocked == false
                                 & this[d.x - 2, d.y - 1].Own == Owner & this[d.x - 2, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                 & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                 & this[d.x - 2, d.y + 3].Own == 0 & this[d.x - 2, d.y + 3].Blocked == false
                                 & this[d.x - 1, d.y + 3].Own == 0 & this[d.x - 1, d.y + 3].Blocked == false
                                 & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                                 & this[d.x - 1, d.y + 1].Own == enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                                 & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                             select d;
            if (pat685_2_3.Count() > 0) return new Dot(pat685_2_3.First().x - 1, pat685_2_3.First().y + 2);
            //--------------Rotate on 90 -2-----------------------------------
            var pat685_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x + 3, d.y - 2].Own == Owner & this[d.x + 3, d.y - 2].Blocked == false
                                   & this[d.x + 2, d.y - 1].Own == Owner & this[d.x + 2, d.y - 1].Blocked == false
                                   & this[d.x + 3, d.y].Own == Owner & this[d.x + 3, d.y].Blocked == false
                                   & this[d.x + 2, d.y + 1].Own == Owner & this[d.x + 2, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                   & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                   & this[d.x + 2, d.y - 3].Own == 0 & this[d.x + 2, d.y - 3].Blocked == false
                                   & this[d.x + 1, d.y - 3].Own == 0 & this[d.x + 1, d.y - 3].Blocked == false
                                   & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own == enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                                   & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                               select d;
            if (pat685_2_3_4.Count() > 0) return new Dot(pat685_2_3_4.First().x + 1, pat685_2_3_4.First().y - 2);
            //============================================================================================================== 
            iNumberPattern = 632;
            var pat632 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 2, d.y + 1].Own == enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                             & this[d.x - 1, d.y + 3].Own == enemy_own & this[d.x - 1, d.y + 3].Blocked == false
                             & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y + 2].Own != enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                         select d;
            if (pat632.Count() > 0) return new Dot(pat632.First().x, pat632.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat632_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                               & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                               & this[d.x + 2, d.y - 1].Own == enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                               & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                               & this[d.x + 1, d.y - 3].Own == enemy_own & this[d.x + 1, d.y - 3].Blocked == false
                               & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                               & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                               & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y - 2].Own != enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                           select d;
            if (pat632_2.Count() > 0) return new Dot(pat632_2.First().x, pat632_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat632_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                 & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y + 2].Own == enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                                 & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                                 & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                                 & this[d.x - 3, d.y + 1].Own == enemy_own & this[d.x - 3, d.y + 1].Blocked == false
                                 & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                 & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x - 2, d.y + 1].Own != enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                             select d;
            if (pat632_2_3.Count() > 0) return new Dot(pat632_2_3.First().x - 1, pat632_2_3.First().y);
            //--------------Rotate on 90 -2-----------------------------------
            var pat632_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                   & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y - 2].Own == enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                                   & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                                   & this[d.x + 3, d.y - 1].Own == enemy_own & this[d.x + 3, d.y - 1].Blocked == false
                                   & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                   & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x + 2, d.y - 1].Own != enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                               select d;
            if (pat632_2_3_4.Count() > 0) return new Dot(pat632_2_3_4.First().x + 1, pat632_2_3_4.First().y);
            //============================================================================================================== 
            iNumberPattern = 1938;
            var pat1938 = from Dot d in get_non_blocked
                          where d.Own == Owner
                              & this[d.x, d.y - 3].Own == Owner & this[d.x, d.y - 3].Blocked == false
                              & this[d.x - 1, d.y - 2].Own == Owner & this[d.x - 1, d.y - 2].Blocked == false
                              & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                              & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                              & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                              & this[d.x, d.y - 2].Own != Owner & this[d.x, d.y - 2].Blocked == false
                              & this[d.x, d.y - 1].Own != Owner & this[d.x, d.y - 1].Blocked == false
                              & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                              & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                              & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                              & this[d.x + 2, d.y + 1].Own == 0 & this[d.x + 2, d.y + 1].Blocked == false
                              & this[d.x + 1, d.y - 3].Own == 0 & this[d.x + 1, d.y - 3].Blocked == false
                              & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                          select d;
            if (pat1938.Count() > 0) return new Dot(pat1938.First().x + 1, pat1938.First().y - 1);
            //180 Rotate=========================================================================================================== 
            var pat1938_2 = from Dot d in get_non_blocked
                            where d.Own == Owner
                                & this[d.x, d.y + 3].Own == Owner & this[d.x, d.y + 3].Blocked == false
                                & this[d.x + 1, d.y + 2].Own == Owner & this[d.x + 1, d.y + 2].Blocked == false
                                & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                & this[d.x, d.y + 2].Own != Owner & this[d.x, d.y + 2].Blocked == false
                                & this[d.x, d.y + 1].Own != Owner & this[d.x, d.y + 1].Blocked == false
                                & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                                & this[d.x - 1, d.y + 3].Own == 0 & this[d.x - 1, d.y + 3].Blocked == false
                                & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                            select d;
            if (pat1938_2.Count() > 0) return new Dot(pat1938_2.First().x - 1, pat1938_2.First().y + 1);
            //--------------Rotate on 90-----------------------------------
            var pat1938_2_3 = from Dot d in get_non_blocked
                              where d.Own == Owner
                                  & this[d.x + 3, d.y].Own == Owner & this[d.x + 3, d.y].Blocked == false
                                  & this[d.x + 2, d.y + 1].Own == Owner & this[d.x + 2, d.y + 1].Blocked == false
                                  & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                  & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                  & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                  & this[d.x + 2, d.y].Own != Owner & this[d.x + 2, d.y].Blocked == false
                                  & this[d.x + 1, d.y].Own != Owner & this[d.x + 1, d.y].Blocked == false
                                  & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                                  & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                  & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                  & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                                  & this[d.x + 3, d.y - 1].Own == 0 & this[d.x + 3, d.y - 1].Blocked == false
                                  & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                              select d;
            if (pat1938_2_3.Count() > 0) return new Dot(pat1938_2_3.First().x + 1, pat1938_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat1938_2_3_4 = from Dot d in get_non_blocked
                                where d.Own == Owner
                                    & this[d.x - 3, d.y].Own == Owner & this[d.x - 3, d.y].Blocked == false
                                    & this[d.x - 2, d.y - 1].Own == Owner & this[d.x - 2, d.y - 1].Blocked == false
                                    & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                    & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                    & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                    & this[d.x - 2, d.y].Own != Owner & this[d.x - 2, d.y].Blocked == false
                                    & this[d.x - 1, d.y].Own != Owner & this[d.x - 1, d.y].Blocked == false
                                    & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                                    & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                    & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                    & this[d.x + 1, d.y + 2].Own == 0 & this[d.x + 1, d.y + 2].Blocked == false
                                    & this[d.x - 3, d.y + 1].Own == 0 & this[d.x - 3, d.y + 1].Blocked == false
                                    & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                select d;
            if (pat1938_2_3_4.Count() > 0) return new Dot(pat1938_2_3_4.First().x - 1, pat1938_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 1775;
            var pat1775 = from Dot d in get_non_blocked
                          where d.Own == Owner
                              & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                              & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                              & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                              & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                              & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                              & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                              & this[d.x, d.y - 2].Own == Owner & this[d.x, d.y - 2].Blocked == false
                              & this[d.x - 1, d.y - 2].Own == Owner & this[d.x - 1, d.y - 2].Blocked == false
                              & this[d.x - 2, d.y - 1].Own == Owner & this[d.x - 2, d.y - 1].Blocked == false
                              & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                              & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                          select d;
            if (pat1775.Count() > 0) return new Dot(pat1775.First().x + 1, pat1775.First().y - 1);
            //180 Rotate=========================================================================================================== 
            var pat1775_2 = from Dot d in get_non_blocked
                            where d.Own == Owner
                                & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                                & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                & this[d.x, d.y + 2].Own == Owner & this[d.x, d.y + 2].Blocked == false
                                & this[d.x + 1, d.y + 2].Own == Owner & this[d.x + 1, d.y + 2].Blocked == false
                                & this[d.x + 2, d.y + 1].Own == Owner & this[d.x + 2, d.y + 1].Blocked == false
                                & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                            select d;
            if (pat1775_2.Count() > 0) return new Dot(pat1775_2.First().x - 1, pat1775_2.First().y + 1);
            //--------------Rotate on 90-----------------------------------
            var pat1775_2_3 = from Dot d in get_non_blocked
                              where d.Own == Owner
                                  & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                  & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                  & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                  & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                  & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                  & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                                  & this[d.x + 2, d.y].Own == Owner & this[d.x + 2, d.y].Blocked == false
                                  & this[d.x + 2, d.y + 1].Own == Owner & this[d.x + 2, d.y + 1].Blocked == false
                                  & this[d.x + 1, d.y + 2].Own == Owner & this[d.x + 1, d.y + 2].Blocked == false
                                  & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                  & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                              select d;
            if (pat1775_2_3.Count() > 0) return new Dot(pat1775_2_3.First().x + 1, pat1775_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat1775_2_3_4 = from Dot d in get_non_blocked
                                where d.Own == Owner
                                    & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                    & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                    & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                    & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                    & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                    & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                                    & this[d.x - 2, d.y].Own == Owner & this[d.x - 2, d.y].Blocked == false
                                    & this[d.x - 2, d.y - 1].Own == Owner & this[d.x - 2, d.y - 1].Blocked == false
                                    & this[d.x - 1, d.y - 2].Own == Owner & this[d.x - 1, d.y - 2].Blocked == false
                                    & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                    & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                select d;
            if (pat1775_2_3_4.Count() > 0) return new Dot(pat1775_2_3_4.First().x - 1, pat1775_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 274;
            var pat274 = from Dot d in get_non_blocked
                         where d.Own == Owner
                             & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                             & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                             & this[d.x, d.y - 2].Own == enemy_own & this[d.x, d.y - 2].Blocked == false
                             & this[d.x, d.y - 3].Own == enemy_own & this[d.x, d.y - 3].Blocked == false
                             & this[d.x + 1, d.y - 4].Own == enemy_own & this[d.x + 1, d.y - 4].Blocked == false
                             & this[d.x + 2, d.y - 3].Own == enemy_own & this[d.x + 2, d.y - 3].Blocked == false
                             & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                             & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y - 2].Own != enemy_own & this[d.x + 1, d.y - 2].Blocked == false
                             & this[d.x + 1, d.y - 3].Own != enemy_own & this[d.x + 1, d.y - 3].Blocked == false
                         select d;
            if (pat274.Count() > 0) return new Dot(pat274.First().x + 1, pat274.First().y);
            //180 Rotate=========================================================================================================== 
            var pat274_2 = from Dot d in get_non_blocked
                           where d.Own == Owner
                               & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                               & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                               & this[d.x, d.y + 2].Own == enemy_own & this[d.x, d.y + 2].Blocked == false
                               & this[d.x, d.y + 3].Own == enemy_own & this[d.x, d.y + 3].Blocked == false
                               & this[d.x - 1, d.y + 4].Own == enemy_own & this[d.x - 1, d.y + 4].Blocked == false
                               & this[d.x - 2, d.y + 3].Own == enemy_own & this[d.x - 2, d.y + 3].Blocked == false
                               & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                               & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                               & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                               & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                               & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                               & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                               & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                               & this[d.x - 1, d.y + 2].Own != enemy_own & this[d.x - 1, d.y + 2].Blocked == false
                               & this[d.x - 1, d.y + 3].Own != enemy_own & this[d.x - 1, d.y + 3].Blocked == false
                           select d;
            if (pat274_2.Count() > 0) return new Dot(pat274_2.First().x - 1, pat274_2.First().y);
            //--------------Rotate on 90-----------------------------------
            var pat274_2_3 = from Dot d in get_non_blocked
                             where d.Own == Owner
                                 & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                 & this[d.x + 2, d.y].Own == enemy_own & this[d.x + 2, d.y].Blocked == false
                                 & this[d.x + 3, d.y].Own == enemy_own & this[d.x + 3, d.y].Blocked == false
                                 & this[d.x + 4, d.y - 1].Own == enemy_own & this[d.x + 4, d.y - 1].Blocked == false
                                 & this[d.x + 3, d.y - 2].Own == enemy_own & this[d.x + 3, d.y - 2].Blocked == false
                                 & this[d.x + 2, d.y - 2].Own == enemy_own & this[d.x + 2, d.y - 2].Blocked == false
                                 & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                 & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                                 & this[d.x + 1, d.y - 1].Own != enemy_own & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x + 2, d.y - 1].Own != enemy_own & this[d.x + 2, d.y - 1].Blocked == false
                                 & this[d.x + 3, d.y - 1].Own != enemy_own & this[d.x + 3, d.y - 1].Blocked == false
                             select d;
            if (pat274_2_3.Count() > 0) return new Dot(pat274_2_3.First().x, pat274_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat274_2_3_4 = from Dot d in get_non_blocked
                               where d.Own == Owner
                                   & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                   & this[d.x - 2, d.y].Own == enemy_own & this[d.x - 2, d.y].Blocked == false
                                   & this[d.x - 3, d.y].Own == enemy_own & this[d.x - 3, d.y].Blocked == false
                                   & this[d.x - 4, d.y + 1].Own == enemy_own & this[d.x - 4, d.y + 1].Blocked == false
                                   & this[d.x - 3, d.y + 2].Own == enemy_own & this[d.x - 3, d.y + 2].Blocked == false
                                   & this[d.x - 2, d.y + 2].Own == enemy_own & this[d.x - 2, d.y + 2].Blocked == false
                                   & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                   & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                   & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x - 1, d.y + 1].Own != enemy_own & this[d.x - 1, d.y + 1].Blocked == false
                                   & this[d.x - 2, d.y + 1].Own != enemy_own & this[d.x - 2, d.y + 1].Blocked == false
                                   & this[d.x - 3, d.y + 1].Own != enemy_own & this[d.x - 3, d.y + 1].Blocked == false
                               select d;
            if (pat274_2_3_4.Count() > 0) return new Dot(pat274_2_3_4.First().x, pat274_2_3_4.First().y + 1);
            //============================================================================================================== 
            iNumberPattern = 44;
            var pat44 = from Dot d in get_non_blocked
                        where d.Own == Owner
                            & this[d.x - 1, d.y + 2].Own == Owner & this[d.x - 1, d.y + 2].Blocked == false
                            & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                            & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                            & this[d.x + 2, d.y - 1].Own == Owner & this[d.x + 2, d.y - 1].Blocked == false
                            & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                            & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                            & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                            & this[d.x + 2, d.y + 1].Own == 0 & this[d.x + 2, d.y + 1].Blocked == false
                            & this[d.x + 1, d.y + 2].Own == 0 & this[d.x + 1, d.y + 2].Blocked == false
                        select d;
            if (pat44.Count() > 0) return new Dot(pat44.First().x + 1, pat44.First().y + 1);
            //180 Rotate=========================================================================================================== 
            var pat44_2 = from Dot d in get_non_blocked
                          where d.Own == Owner
                              & this[d.x + 1, d.y - 2].Own == Owner & this[d.x + 1, d.y - 2].Blocked == false
                              & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                              & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                              & this[d.x - 2, d.y + 1].Own == Owner & this[d.x - 2, d.y + 1].Blocked == false
                              & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                              & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                              & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                              & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                              & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                          select d;
            if (pat44_2.Count() > 0) return new Dot(pat44_2.First().x - 1, pat44_2.First().y - 1);
            //--------------Rotate on 90-----------------------------------
            var pat44_2_3 = from Dot d in get_non_blocked
                            where d.Own == Owner
                                & this[d.x + 2, d.y + 1].Own == Owner & this[d.x + 2, d.y + 1].Blocked == false
                                & this[d.x + 1, d.y].Own == enemy_own & this[d.x + 1, d.y].Blocked == false
                                & this[d.x, d.y - 1].Own == enemy_own & this[d.x, d.y - 1].Blocked == false
                                & this[d.x - 1, d.y - 2].Own == Owner & this[d.x - 1, d.y - 2].Blocked == false
                                & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                            select d;
            if (pat44_2_3.Count() > 0) return new Dot(pat44_2_3.First().x + 1, pat44_2_3.First().y - 1);
            //--------------Rotate on 90 -2-----------------------------------
            var pat44_2_3_4 = from Dot d in get_non_blocked
                              where d.Own == Owner
                                  & this[d.x - 2, d.y - 1].Own == Owner & this[d.x - 2, d.y - 1].Blocked == false
                                  & this[d.x - 1, d.y].Own == enemy_own & this[d.x - 1, d.y].Blocked == false
                                  & this[d.x, d.y + 1].Own == enemy_own & this[d.x, d.y + 1].Blocked == false
                                  & this[d.x + 1, d.y + 2].Own == Owner & this[d.x + 1, d.y + 2].Blocked == false
                                  & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                  & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                  & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                  & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                                  & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                              select d;
            if (pat44_2_3_4.Count() > 0) return new Dot(pat44_2_3_4.First().x - 1, pat44_2_3_4.First().y + 1);
            //============================================================================================================== 



            //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
            return null;//если никаких паттернов не найдено возвращаем нуль

        }

        //CancellationToken cancellationToken;

        public Dot PickComputerMove(Dot enemy_move, CancellationToken? cancellationToken)
        {
            #region если первый ход выбираем произвольную соседнюю точку

            //_DotsForDrawing = Dots.Where(d=>d.Own!=0).ToList();

            if (ListMoves.Count < 2)
            {
                var random = new Random(DateTime.Now.Millisecond);
                var fm = from Dot d in Dots
                         where d.Own == 0 & Math.Sqrt(Math.Pow(Math.Abs(d.x - enemy_move.x), 2) + Math.Pow(Math.Abs(d.y - enemy_move.y), 2)) < 2
                         orderby random.Next()
                         select d;
                return new Dot(fm.First().x, fm.First().y); //так надо чтобы best_move не ссылался на точку в Dots;
            }
#endregion
#region  Если ситуация проигрышная -сдаемся
            var q1 = from Dot d in Dots
                     where d.Own == PLAYER_COMPUTER && (d.Blocked == false)
                     select d;
            var q2 = from Dot d in Dots
                     where d.Own == PLAYER_HUMAN && (d.Blocked == false)
                     select d;
            float res1 = q2.Count();
            float res2 = q1.Count();
            if (res1 / res2 > 2.0)
            {
                return null;
            }

#endregion

            best_move = null;
            var t1 = DateTime.Now.Millisecond;
            #region StopWatch
#if DEBUG
            stopWatch.Start();
            lstDbgMoves.Clear();
            lstDbg1.Clear();
#endif
#endregion
            counter_moves = 0;
            counter_moves_all = 0;
            //lst_best_move.Clear();
            lst_branch.Clear();
            lst_moves.Clear();
            //Проигрываем разные комбинации
            recursion_depth = 0;
            object lockObj = new object();

            lock (lockObj)
            {
                 Play(PLAYER_HUMAN, PLAYER_COMPUTER, cancellationToken);
            }
            
            
            Dot move = lst_branch.Where(dt => dt.Rating == lst_branch.Min(d => d.Rating)).ElementAtOrDefault(0);
            if (move!=null) best_move = move;
            #region Если не найдено лучшего хода, берем любую точку
            if (best_move == null)
            {
                var random = new Random(DateTime.Now.Millisecond);
                var q = from Dot d in Dots//любая точка
                        where d.Blocked == false & d.Own == PLAYER_NONE 
                        orderby random.Next()
                        select d;

                if (q.Count() > 0)
                {
                    best_move = q.Where(dt => Distance(dt, LastMove) < 3).FirstOrDefault();
                    if (best_move == null) best_move = q.FirstOrDefault();
#if DEBUG
                    {

                        lstDbgMoves.Add(best_move.ToString() + " - random ");

                    }
#endif
                }
                else return null;
            }
            #endregion

#region Statistic
#if DEBUG
            stopWatch.Stop();
            foreach (string d in lstDbgMoves) Debug.WriteLine(d);
            GameEngine.DbgInfo = "Количество ходов: " + counter_moves +
                                "\r\nХод на " + best_move.ToString() +
                                "\r\nвремя просчета " + stopWatch.ElapsedMilliseconds.ToString() + " мс" +
                                "\r\nСделано ходов: " + counter_moves_all;
            stopWatch.Reset();
#endif
            #endregion

            //_DotsForDrawing = Dots.Where(d => d.Own != 0).ToList();
            //_DotsForDrawing = Dots.ToList();

            return new Dot(best_move.x, best_move.y); //так надо чтобы best_move не ссылался на точку в Dots
        }
        //===============================================================================================
        //-----------------------------------Поиск лучшего хода------------------------------------------
        //===============================================================================================
        private List<Dot> BestMove(int pl1, int pl2)
        {
            string strDebug = string.Empty;
            List<Dot> moves = new List<Dot>();
            Dot bm;
#if DEBUG
            sW2.Start();
            GameEngine.DbgInfo = "CheckMove(pl2,pl1)...";
#endif
            bm = CheckMove(pl2);
            if (DotIndexCheck(bm))
            {
#region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl1 + " - Win Comp! " + iNumberPattern);
                }
#endif
#endregion
                bm.iNumberPattern = 777; //777-ход в результате которого получается окружение -компьютер побеждает
                moves.Add(bm);
                return moves;
            }
            bm = CheckMove(pl1);
            if (DotIndexCheck(bm))
            {
                #region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl1 + " - Win Human! " + iNumberPattern);
                }
#endif
                #endregion
                bm.iNumberPattern = 666; //666-ход в результате которого получается окружение -компьютер проигрывает
                moves.Add(bm);
                return moves;
            }
#region DEBUG
#if DEBUG
            sW2.Stop();
            sW2.Reset();
            //проверяем паттерны
            sW2.Start();
            GameEngine.DbgInfo = "CheckPattern_vilochka проверяем ходы на два вперед...";
            //DrawSession.CanvasCtrl.Invalidate();
#endif
            #endregion
            #region CheckPattern_vilochka
            bm = CheckPattern_vilochka(pl2);
            //if (bm != null)
            if (DotIndexCheck(bm))
            {
#region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl2 + " -CheckPattern_vilochka " + iNumberPattern);
                }
#endif
#endregion
                bm.iNumberPattern = 777; //777-ход в результате которого получается окружение -компьютер побеждает
                moves.Add(bm);//return bm;
            }

            bm = CheckPattern_vilochka(pl1);
            // if (bm != null)
            if (DotIndexCheck(bm))
            {
#region DEBUG
#if DEBUG

                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl1 + " -CheckPattern_vilochka " + iNumberPattern);
                }
#endif
#endregion
                bm.iNumberPattern = 666; //777-ход в результате которого получается окружение -компьютер побеждает
                moves.Add(bm);//return bm;
            }
            #region DEBUG

#if DEBUG
            sW2.Stop();
            strDebug = strDebug + "\r\nCheckPattern_vilochka -" + sW2.Elapsed.Milliseconds.ToString();
            GameEngine.DbgInfo = strDebug;
            sW2.Reset();
            sW2.Start();
            //DrawSession.CanvasCtrl.Invalidate();

#endif
            #endregion
            #endregion
            #region CheckPattern2Move проверяем ходы на два вперед
            List<Dot> empty_dots = EmptyNeibourDots(pl2);
            List<Dot> lst_dots2;

//            foreach (Dot dot in empty_dots)
//            {
//                if (CheckDot(dot, pl2) == false) MakeMove(dot, pl2);
//                lst_dots2 = CheckPattern2Move(pl2);
//                foreach (Dot nd in lst_dots2)
//                {
//                    if (MakeMove(nd, pl2) != 0)
//                    {
//                        UndoMove(nd);
//                        UndoMove(dot);
//                        #region DEBUG
//#if DEBUG
//                        {
//                            //lstDbgMoves.Add(dot.x + ":" + dot.y + " player" + pl2 + " - CheckPattern2Move!");
//                        }
//#endif
//                        #endregion
//                        dot.iNumberPattern = 777;
//                        moves.Add(dot);
//                    }
//                    UndoMove(nd);
//                }
//                UndoMove(dot);
//            }
#if DEBUG
            sW2.Stop();
            strDebug = strDebug + "\r\nCheckPattern2Move(pl2) - " + sW2.Elapsed.Milliseconds.ToString();
            GameEngine.DbgInfo = strDebug;
            //DrawSession.CanvasCtrl.Invalidate();
            sW2.Reset();
            sW2.Start();
            //f.lblBestMove.Text = "CheckPattern_vilochka...";

#endif

            #endregion
            //-----------------------------------------------------------------
            #region CheckPatternVilkaNextMove
            bm = CheckPatternVilkaNextMove(pl2);
            if (DotIndexCheck(bm))
            {
#region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl2 + "CheckPatternVilkaNextMove " + iNumberPattern);
                }
#endif
#endregion
                moves.Add(bm); //return bm;
            }
            #region DEBUG

#if DEBUG
            sW2.Stop();
            strDebug = strDebug + "\r\nCheckPatternVilkaNextMove -" + sW2.Elapsed.Milliseconds.ToString();
            GameEngine.DbgInfo = strDebug;
           // DrawSession.CanvasCtrl.Invalidate();
            sW2.Reset();
            sW2.Start();
#endif
            #endregion
            #endregion
//-------------------------------------------------
            #region CheckPattern

            bm = CheckPattern(pl2);
            //foreach (Dot dt in CheckPattern(pl2))
            //{
                if (DotIndexCheck(bm))
                {
                    #region DEBUG
#if DEBUG
                            {
                                lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl2 + " - CheckPattern " + bm.iNumberPattern);
                            }
#endif
                    #endregion
                    if (CheckDot(bm, pl2) == false) moves.Add(bm);
                }
            //}
            bm = CheckPattern(pl1);
            //foreach (Dot dt in CheckPattern(pl1))
            //{
                if (DotIndexCheck(bm))
                {
                    #region DEBUG
#if DEBUG
                                {
                                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl1 + " - CheckPattern " + bm.iNumberPattern);
                                }
#endif
                    #endregion
                    if (CheckDot(bm, pl2) == false) moves.Add(bm);
                }
            //}

            #endregion
//-------------------------------------------------
            #region CheckPatternMove
            bm = CheckPatternMove(pl2);
            if(DotIndexCheck(bm))
            //if (bm != null)
            {
#region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl2 + " -CheckPatternMove " + iNumberPattern);
                }
#endif
#endregion
                if (CheckDot(bm, pl2) == false) moves.Add(bm);//return bm;
            }
            bm = CheckPatternMove(pl1);
            if (DotIndexCheck(bm))
                //if (bm != null)
            {
#region DEBUG
#if DEBUG
                {
                    lstDbgMoves.Add(bm.x + ":" + bm.y + " player" + pl2 + " -CheckPatternMove " + iNumberPattern);
                }
#endif
#endregion
                if (CheckDot(bm, pl1) == false) moves.Add(bm); //return bm;
            }
#if DEBUG
            sW2.Stop();
            strDebug = strDebug + "\r\nCheckPatternMove(pl2) -" + sW2.Elapsed.Milliseconds.ToString();
            GameEngine.DbgInfo = strDebug;
            sW2.Reset();
#endif

#endregion
            var result = moves.Distinct(new DotEq());
            return result.ToList();
        }
        //==================================================================================================================
        
        List<Dot> lst_moves = new List<Dot>();//сюда заносим проверяемые ходы
        List<Dot> lst_branch = new List<Dot>();//сюда заносим начало перспективной ветки для одного игрока
        List<Dot> lst_branch_enemy = new List<Dot>();//сюда заносим начало перспективной ветки для другого игрока
        //
        int counter_moves = 0;
        int counter_moves_all = 0;
        int res_last_move; //хранит результат хода
        //int recursion_depth;
        const int MAX_RECURSION = 3;
        int recursion_depth;
        Dot tempmove;
        //===================================================================================================================
        private int Play(int player1, int player2, CancellationToken? cancellationToken)//возвращает Owner кто побеждает в результате хода
        {
            if (cancellationToken.HasValue)
                cancellationToken.Value.ThrowIfCancellationRequested();

            List<Dot> lst_best_move = new List<Dot>();//сюда заносим лучшие ходы
            if (recursion_depth==1)counter_moves = 1;
            GameEngine.DbgInfo= "check move - " + counter_moves.ToString();
            recursion_depth++;
            StatusMsg.ColorMsg = player1 == 1 ? GameEngine.colorGamer1 : GameEngine.colorGamer2;
            StatusMsg.textMsg = "Play - Player" + player1 + "..." + "\r\n" +
                                "recursion_depth: " + recursion_depth + "\r\n" +
                                "counter_moves: " + counter_moves + "\r\n" +
                                "counter_moves_all: " + counter_moves_all;

            if (recursion_depth > MAX_RECURSION) return PLAYER_NONE;

            lst_best_move = BestMove(player1, player2);
            
            //если есть паттерн на окружение противника тоже устанавливается бест мув
            tempmove = lst_best_move.Where(dt => dt.iNumberPattern == 777).FirstOrDefault();
            if (tempmove != null) 
            {
                if (player2 == PLAYER_COMPUTER)
                {
                    lst_branch.Add(tempmove);
                    lst_branch.Last().Rating = counter_moves;
                }
                else
                {
                    lst_branch_enemy.Add(tempmove);
                    lst_branch_enemy.Last().Rating = counter_moves;
                }
                return player2;

            }
            //если есть паттерн на окружение компа устанавливается бест мув
            tempmove = lst_best_move.Where(dt => dt.iNumberPattern == 666).FirstOrDefault();
            if (tempmove != null) 
            {
                if (player2 == PLAYER_COMPUTER)
                {
                    lst_branch.Add(tempmove);
                    lst_branch.Last().Rating = counter_moves;
                }
                else
                {
                    lst_branch_enemy.Add(tempmove);
                    lst_branch_enemy.Last().Rating = counter_moves;
                }
                return player1;
            }

            if(lst_best_move.Count>0)
            {
#region Cycle
                foreach (Dot move in lst_best_move)
                {
                    if (cancellationToken.HasValue)
                        cancellationToken.Value.ThrowIfCancellationRequested();

                    #region ходит комп в проверяемые точки
                    player2 = player1 == PLAYER_HUMAN ? PLAYER_COMPUTER : PLAYER_HUMAN;
                    //**************делаем ход***********************************
                    res_last_move = MakeMove(move,player2);
                    lst_moves.Add(move);
                    counter_moves_all++;
                    counter_moves++;
#region проверка на окружение

                    if (win_player == PLAYER_COMPUTER)
                    {
                        Dot dt_move = lst_moves.First();
                        dt_move.Rating = counter_moves;
                        lst_branch.Add(dt_move);
                        UndoMove(move);
                        continue;
                        //return PLAYER_COMPUTER;
                    }
                    //если ход в заведомо окруженный регион - пропускаем такой ход
                    if (win_player == PLAYER_HUMAN)
                    {
                        UndoMove(move);
                        continue;
                    }
#endregion
#region Debug statistic
#if DEBUG
                    //if (f.chkMove.Checked) Pause(); //делает паузу если значение поля pause>0
                    lstDbg1.Add(move.ToString());//(move.Own + " -" + move.x + ":" + move.y);
                    
                    GameEngine.DbgInfo = " Ходов проверено: " + counter_moves +
                                       "\r\n проверка вокруг точки " + LastMove +
                                       "\r\n время поиска " + stopWatch.ElapsedMilliseconds;
                    Debug.WriteLine(GameEngine.DbgInfo);
#endif
                    #endregion
                    //теперь ходит другой игрок ===========================================================================

                    int result = Play(player2, player1, cancellationToken);

                    recursion_depth--;

                     if (result == 0)
                    {
                        lst_moves.Remove(move);
#if DEBUG
                        lstDbg1.Remove(move.ToString());
#endif
                        UndoMove(move);
                        continue;
                    }
                    else if (result == PLAYER_COMPUTER)
                    {
                        if (recursion_depth == 1)
                        {
                            lst_moves[0].Rating = counter_moves;
                            lst_branch.Add(lst_moves[0]);
                        }
                        
                        lst_moves.Remove(move);
#if DEBUG
                        lstDbg1.Remove(move.ToString());
#endif
                        UndoMove(move);
                        return result;
                    }
                    else if (result == PLAYER_HUMAN)
                    {
                        if (recursion_depth == 1)
                        {
                            lst_moves[0].Rating = counter_moves;//такой рейтинг устанавливается если выигрывает человек
                            lst_branch.Add(lst_moves[0]);
                        }
                        lst_moves.Remove(move);
#if DEBUG
                        lstDbg1.Remove(move.ToString());
#endif

                        UndoMove(move);
                        return result;
                    }


#region Debug
#if DEBUG
//remove from list
                    if (lstDbg1.Count > 0) lstDbg1.RemoveAt(lstDbg1.Count - 1);


                    //foreach(string d in lstDbg1) Debug.WriteLine(d);

#endif
                    #endregion
                }
                #endregion
            }
            #endregion
            //lst_moves.Remove(lst_moves.Last());
            //recursion_depth--;


            best_move = lst_best_move.Where(dt => dt.Rating == lst_best_move.Min(d => d.Rating)).ElementAtOrDefault(0);
            return PLAYER_NONE;

        }//----------------------------Play-----------------------------------------------------

        private float SquarePolygon(int nBlockedDots, int nRegionDots)
        {
            return nBlockedDots + nRegionDots / 2.0f - 1;//Формула Пика
        }
        //IEnumerator and IEnumerable require these methods.
        public IEnumerator GetEnumerator()
        {
            position = -1;
            //return this;
            return _Dots.GetEnumerator();
        }
        //IEnumerator
        public bool MoveNext()
        {
            position++;
            return (position < _Dots.Count);
        }
        //IEnumerable
        public void Reset()
        { position = 0; }
        //IEnumerable
        public object Current
        {
            get
            {
                return _Dots[position];
            }
        }
    }
}
