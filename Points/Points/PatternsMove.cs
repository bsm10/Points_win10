using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotsGame
{
    public partial class GameDots
    {
        private Dot CheckPatternMove(int Owner)//паттерны без вражеской точки
        {
            iNumberPattern = 1;
            var pat1 = from Dot d in this
                         where d.Own == 0
                             & this[d.x - 1, d.y + 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 1, d.y + 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                             & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                         select d;
            if (pat1.Count() > 0) return new Dot(pat1.First().x, pat1.First().y);
            //--------------Rotate on 90----------------------------------- 
            var pat1_2_3_4 = from Dot d in this
                               where d.Own == 0
                                   & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
                                   & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y + 1].Blocked == false
                                   & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                                   & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                                   & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y + 1].Blocked == false
                                   & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                   & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                   & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                                   & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                   & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                               select d;
            if (pat1_2_3_4.Count() > 0) return new Dot(pat1_2_3_4.First().x, pat1_2_3_4.First().y);
            //============================================================================================================== 
            iNumberPattern = 883;
            var pat883 = from Dot d in this
                         where d.Own == Owner
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             & this[d.x + 3, d.y].Own == 0 & this[d.x + 3, d.y].Blocked == false
                             & this[d.x + 3, d.y - 1].Own == Owner & this[d.x + 3, d.y - 1].Blocked == false
                             & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             & this[d.x + 2, d.y - 2].Own == 0 & this[d.x + 2, d.y - 2].Blocked == false
                             & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                             & this[d.x + 3, d.y - 2].Own == 0 & this[d.x + 3, d.y - 2].Blocked == false
                             & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                         select d;
            if (pat883.Count() > 0) return new Dot(pat883.First().x + 1, pat883.First().y - 1);
            //--------------Rotate on 90----------------------------------- 
            var pat883_2_3 = from Dot d in this
                             where d.Own == Owner
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                                 & this[d.x, d.y - 3].Own == 0 & this[d.x, d.y - 3].Blocked == false
                                 & this[d.x + 1, d.y - 3].Own == Owner & this[d.x + 1, d.y - 3].Blocked == false
                                 & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                                 & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                 & this[d.x + 2, d.y - 2].Own == 0 & this[d.x + 2, d.y - 2].Blocked == false
                                 & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                                 & this[d.x + 2, d.y - 3].Own == 0 & this[d.x + 2, d.y - 3].Blocked == false
                                 & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                             select d;
            if (pat883_2_3.Count() > 0) return new Dot(pat883_2_3.First().x + 1, pat883_2_3.First().y - 1);
            //=================================================================================
            // 0d край доски
            // m   *
            iNumberPattern = 2;
            var pat2 = from Dot d in this
                         where d.Own == Owner & d.y==0 & d.x>0 & d.x< BoardWidth
                             & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                             & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                         select d;
            if (pat2.Count() > 0) return new Dot(pat2.First().x, pat2.First().y + 1);
            var pat2_2 = from Dot d in this
                         where d.Own == Owner & d.y > 1 & d.y < BoardHeight & d.x==0
                               & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                               & this[d.x + 1, d.y ].Own == 0 & this[d.x+1, d.y].Blocked == false
                           select d;
            if (pat2_2.Count() > 0) return new Dot(pat2_2.First().x+1, pat2_2.First().y );
            var pat2_2_3 = from Dot d in this
                           where d.Own == Owner & d.x == BoardWidth - 1 & d.y > 0 & d.y < BoardHeight
                                 & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                 & this[d.x-1, d.y].Own == 0 & this[d.x-1, d.y].Blocked == false
                           select d;
            if (pat2_2_3.Count() > 0) return new Dot(pat2_2_3.First().x - 1, pat2_2_3.First().y);
            var pat2_2_3_4 = from Dot d in this
                             where d.Own == Owner & d.y == BoardHeight - 1 & d.x > 0 & d.x < BoardWidth
                                   & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             select d;
            if (pat2_2_3_4.Count() > 0) return new Dot(pat2_2_3_4.First().x, pat2_2_3_4.First().y - 1);
            ////============================================================================================================== 
            //iNumberPattern = 3;
            //var pat3 = from Dot d in this
            //           where d.Own == Owner & d.Blocked == false
            //                 & this[d.x + 1, d.y - 1].Own == Owner & this[d.x + 1, d.y - 1].Blocked == false
            //                 & this[d.x + 1, d.y].Blocked == false & this[d.x + 1, d.y].Own !=Owner
            //                 & this[d.x, d.y - 1].Blocked == false & this[d.x, d.y - 1].Own !=Owner
            //             select d;
            //Dot[] ad = pat3.ToArray();
            //foreach (Dot d in ad)
            //{
            //    if (this[d.x+1, d.y].Own == 0) return new Dot(d.x+1, d.y);
            //    if (this[d.x, d.y - 1].Own == 0) return new Dot(d.x, d.y - 1);
            //}
            //       *
            //     m
            //  d*
            iNumberPattern = 4;
            var pat4 = from Dot d in this
                       where d.Own == Owner
                           & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                           & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                           & this[d.x + 2, d.y - 2].Own == Owner & this[d.x + 2, d.y - 2].Blocked == false
                           & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                           & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                           & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                       select d;
            if (pat4.Count() > 0) return new Dot(pat4.First().x + 1, pat4.First().y - 1);
            //180 Rotate=========================================================================================================== 
            //  *
            //     m
            //        d* 
            var pat4_2 = from Dot d in this
                         where d.Own == Owner
                             & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y - 2].Own == 0 & this[d.x - 1, d.y - 2].Blocked == false
                             & this[d.x - 2, d.y - 2].Own == Owner & this[d.x - 2, d.y - 2].Blocked == false
                             & this[d.x - 2, d.y - 1].Own == 0 & this[d.x - 2, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 1, d.y - 1].Own == 0 & this[d.x - 1, d.y - 1].Blocked == false
                         select d;
            if (pat4_2.Count() > 0) return new Dot(pat4_2.First().x - 1, pat4_2.First().y - 1);
            //============================================================================================================== 
            //// если точка рядом с бортом - заземляем
            //iNumberPattern = 5;
            //var pat5 = from Dot d in this
            //             where d.Own == Owner
            //                 & this[d.x + 1, d.y].Own == 0 & d.x + 1 == iBoardSize - 1
            //             select d;
            //if (pat5.Count() > 0) return new Dot(pat5.First().x + 1, pat5.First().y);
            //var pat5_2_3_4 = from Dot d in this
            //                 where d.Own == Owner
            //                     & this[d.x - 1, d.y].Own == 0 & d.x - 1 == 0
            //                 select d;
            //if (pat5_2_3_4.Count() > 0) return new Dot(pat5_2_3_4.First().x - 1, pat5_2_3_4.First().y );
            //var pat5_2 = from Dot d in this
            //               where d.Own == Owner
            //                   & this[d.x, d.y + 1].Own == 0 & d.y + 1 == iBoardSize - 1
            //               select d;
            //if (pat5_2.Count() > 0) return new Dot(pat5_2.First().x , pat5_2.First().y+1);
            //var pat5_2_3 = from Dot d in this
            //                 where d.Own == Owner
            //                     & this[d.x, d.y - 1].Own == 0 & (d.y - 1) == 0
            //                 select d;
            //if (pat5_2_3.Count() > 0) return new Dot(pat5_2_3.First().x, pat5_2_3.First().y - 1);
            ////============================================================================================================== 
            ////     m  *
            //// d*      
            //iNumberPattern = 6;
            //var pat6 = from Dot d in this
            //             where d.Own == Owner
            //                 && d.IndexRelation != this[d.x + 2, d.y - 1].IndexRelation
            //                 & this[d.x + 2, d.y - 1].Blocked == false
            //                 & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
            //                 & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
            //             select d;
            //if (pat6.Count() > 0) return new Dot(pat6.First().x + 1, pat6.First().y - 1);
            ////180 Rotate=========================================================================================================== 
            ////         d*
            //// *   m
            //var pat6_2 = from Dot d in this
            //               where d.Own == Owner
            //                   && d.IndexRelation != this[d.x - 2, d.y + 1].IndexRelation
            //                   & this[d.x - 2, d.y + 1].Blocked == false
            //                   & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
            //                   & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
            //               select d;
            //if (pat6_2.Count() > 0) return new Dot(pat6_2.First().x - 1, pat6_2.First().y + 1);
            ////--------------Rotate on 90----------------------------------- 
            ////     *     
            ////     m 
            ////  d*
            //var pat6_2_3 = from Dot d in this
            //                 where d.Own == Owner
            //                    && d.IndexRelation != this[d.x + 1, d.y - 2].IndexRelation
            //                     & this[d.x + 1, d.y - 2].Blocked == false
            //                     & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
            //                     & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
            //                 select d;
            //if (pat6_2_3.Count() > 0) return new Dot(pat6_2_3.First().x + 1, pat6_2_3.First().y - 1);
            ////--------------Rotate on 90 - 2----------------------------------- 
            ////        d*     
            ////     m 
            ////     *
            //var pat6_2_3_4 = from Dot d in this
            //                   where d.Own == Owner
            //                        && d.IndexRelation != this[d.x - 1, d.y + 2].IndexRelation
            //                       & this[d.x - 1, d.y + 2].Blocked == false
            //                       & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
            //                       & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
            //                   select d;
            //if (pat6_2_3_4.Count() > 0) return new Dot(pat6_2_3_4.First().x - 1, pat6_2_3_4.First().y + 1);
            //============================================================================================================== 
            //d*  m  *
            iNumberPattern = 7;
            var pat7 = from Dot d in this
                         where d.Own == Owner
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x + 1, d.y-1].Own == 0 & this[d.x + 1, d.y-1].Blocked == false
                             & this[d.x + 1, d.y+1].Own == 0 & this[d.x + 1, d.y+1].Blocked == false
                             & this[d.x + 2, d.y].Own == Owner & this[d.x + 2, d.y].Blocked == false
                         select d;
            if (pat7.Count() > 0) return new Dot(pat7.First().x + 1, pat7.First().y);
            //--------------Rotate on 90----------------------------------- 
            //   *
            //   m
            //   d*
            var pat7_2 = from Dot d in this
                             where d.Own == Owner
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x-1, d.y - 1].Own == 0 & this[d.x-1, d.y - 1].Blocked == false
                                 & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x, d.y - 2].Own == Owner & this[d.x, d.y - 2].Blocked == false
                             select d;
            if (pat7_2.Count() > 0) return new Dot(pat7_2.First().x, pat7_2.First().y - 1);
            //============================================================================================================== 
            //    *
            // m 
            //
            // d*
            iNumberPattern = 8;
            var pat8 = from Dot d in this
                         where d.Own == Owner
                             & this[d.x + 1, d.y - 3].Own == Owner & this[d.x + 1, d.y - 3].Blocked == false
                             & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                             & this[d.x + 1, d.y - 2].Own == 0 & this[d.x + 1, d.y - 2].Blocked == false
                             & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                             & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                             & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                             & this[d.x, d.y - 3].Own == 0 & this[d.x, d.y - 3].Blocked == false
                         select d;
            if (pat8.Count() > 0) return new Dot(pat8.First().x, pat8.First().y - 2);
            //180 Rotate=========================================================================================================== 
            var pat8_2 = from Dot d in this
                           where d.Own == Owner
                               & this[d.x - 1, d.y + 3].Own == Owner & this[d.x - 1, d.y + 3].Blocked == false
                               & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                               & this[d.x - 1, d.y + 2].Own == 0 & this[d.x - 1, d.y + 2].Blocked == false
                               & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                               & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                               & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                               & this[d.x, d.y + 3].Own == 0 & this[d.x, d.y + 3].Blocked == false
                           select d;
            if (pat8_2.Count() > 0) return new Dot(pat8_2.First().x, pat8_2.First().y + 2);
            //--------------Rotate on 90----------------------------------- 
            var pat8_2_3 = from Dot d in this
                             where d.Own == Owner
                                 & this[d.x + 3, d.y - 1].Own == Owner & this[d.x + 3, d.y - 1].Blocked == false
                                 & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                                 & this[d.x + 2, d.y - 1].Own == 0 & this[d.x + 2, d.y - 1].Blocked == false
                                 & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                 & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                                 & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                 & this[d.x + 3, d.y].Own == 0 & this[d.x + 3, d.y].Blocked == false
                             select d;
            if (pat8_2_3.Count() > 0) return new Dot(pat8_2_3.First().x + 2, pat8_2_3.First().y);
            //--------------Rotate on 90 - 2----------------------------------- 
            var pat8_2_3_4 = from Dot d in this
                               where d.Own == Owner
                                   & this[d.x - 3, d.y + 1].Own == Owner & this[d.x - 3, d.y + 1].Blocked == false
                                   & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                                   & this[d.x - 2, d.y + 1].Own == 0 & this[d.x - 2, d.y + 1].Blocked == false
                                   & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                   & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                                   & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                   & this[d.x - 3, d.y].Own == 0 & this[d.x - 3, d.y].Blocked == false
                               select d;
            if (pat8_2_3_4.Count() > 0) return new Dot(pat8_2_3_4.First().x - 2, pat8_2_3_4.First().y);
                //============================================================================================================== 
                //     *
                //        d*  
                //     m
                //============================================================================================================== 
            iNumberPattern = 9;
            var pat9 = from Dot d in this
                         where d.Own == Owner
                             & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                             & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                             & this[d.x - 1, d.y].Own == 0 & this[d.x - 1, d.y].Blocked == false
                             & this[d.x - 2, d.y].Own == 0 & this[d.x - 2, d.y].Blocked == false
                         select d;
            if (pat9.Count() > 0) return new Dot(pat9.First().x - 1, pat9.First().y + 1);
            //180 Rotate=========================================================================================================== 
            //     m  
            // d*  
            //     *
            var pat9_2 = from Dot d in this
                           where d.Own == Owner
                               & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                               & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                               & this[d.x + 1, d.y].Own == 0 & this[d.x + 1, d.y].Blocked == false
                               & this[d.x + 2, d.y].Own == 0 & this[d.x + 2, d.y].Blocked == false
                           select d;
            if (pat9_2.Count() > 0) return new Dot(pat9_2.First().x + 1, pat9_2.First().y - 1);
            //--------------Rotate on 90----------------------------------- 
            //         
            //     d*
            //  m       *
            var pat9_2_3 = from Dot d in this
                             where d.Own == Owner
                                 & this[d.x + 1, d.y + 1].Own == Owner & this[d.x + 1, d.y + 1].Blocked == false
                                 & this[d.x - 1, d.y + 1].Own == 0 & this[d.x - 1, d.y + 1].Blocked == false
                                 & this[d.x, d.y + 1].Own == 0 & this[d.x, d.y + 1].Blocked == false
                                 & this[d.x, d.y + 2].Own == 0 & this[d.x, d.y + 2].Blocked == false
                             select d;
            if (pat9_2_3.Count() > 0) return new Dot(pat9_2_3.First().x - 1, pat9_2_3.First().y + 1);
            //--------------Rotate on 90 - 2----------------------------------- 
            // *      m
            //    d*   
            //
            var pat9_2_3_4 = from Dot d in this
                               where d.Own == Owner
                                   & this[d.x - 1, d.y - 1].Own == Owner & this[d.x - 1, d.y - 1].Blocked == false
                                   & this[d.x + 1, d.y - 1].Own == 0 & this[d.x + 1, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 1].Own == 0 & this[d.x, d.y - 1].Blocked == false
                                   & this[d.x, d.y - 2].Own == 0 & this[d.x, d.y - 2].Blocked == false
                               select d;
            if (pat9_2_3_4.Count() > 0) return new Dot(pat9_2_3_4.First().x + 1, pat9_2_3_4.First().y - 1);
                //============================================================================================================== 

            //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
            return null;//если никаких паттернов не найдено возвращаем нуль
        }

        public List<Dot> CheckPattern2Move(int Owner, bool IndexRelation)
        {
            IEnumerable<Dot> qry;
            if (IndexRelation)
            {
                qry = from Dot d1 in this
                      where d1.Own == Owner && !d1.Blocked
                      from Dot d2 in this
                      where d2.IndexRelation == d1.IndexRelation && !d2.Blocked && Distance(d1, d2) < 3.5f & Distance(d1, d2) >= 3f
                      from Dot de1 in this
                      where de1.ValidMove & Distance(d1, de1) == 1
                      from Dot de2 in this
                      where de2.ValidMove & Distance(de1, de2) == 1 & Distance(d1, de2) < 2

                      from Dot de3 in this
                      where de3.ValidMove & Distance(d2, de3) < 2 & Distance(de2, de3) == 1
                         || de3.ValidMove & Distance(d2, de3) < 2 & Distance(de1, de3) == 1

                      select new Dot(de3.x, de3.y, NumberPattern: 777, Rating: 1);
            }
            else
            {
                qry = from Dot d1 in this
                      where d1.Own == Owner && !d1.Blocked
                      from Dot d2 in this
                      where d2.Own == Owner && !d2.Blocked && Distance(d1, d2) < 3.5f & Distance(d1, d2) >= 3
                      from Dot de1 in this
                      where de1.ValidMove & Distance(d1, de1) == 1
                      from Dot de2 in this
                      where de2.ValidMove & Distance(de1, de2) == 1 & Distance(d1, de2) < 2
                      from Dot de3 in this
                      where de3.ValidMove & Distance(d2, de3) < 2 & Distance(de2, de3) == 1
                         || de3.ValidMove & Distance(d2, de3) < 2 & Distance(de1, de3) == 1
                      select de3;

            }

            return qry.Distinct(new DotEq()).ToList();
        }
        public List<Dot> CheckPatternVilka2x2(int Owner, bool IndexRelation) //проверка хода на гарантированное окружение(когда точки находятся через 4 клетки) 
        {
            IEnumerable<Dot> qry;
            if (IndexRelation)
            {
                qry = from Dot d1 in this
                      where d1.Own == Owner && !d1.Blocked

                      from Dot d2 in this
                      where d2.IndexRelation == d1.IndexRelation && !d2.Blocked && Distance(d1, d2) < 4.5f & Distance(d1, d2) >= 2.5f

                      from Dot de1 in this
                      where de1.ValidMove & Distance(d1, de1) == 1

                      from Dot de2 in this
                      where de2.ValidMove & Distance(d2, de2) == 1

                      from Dot de1_1 in this
                      where de1_1.ValidMove & Distance(de1_1, de1) == 1 & Distance(de1_1, d1) < 2

                      from Dot de2_1 in this
                      where de2_1.ValidMove & Distance(de2_1, de2) == 1 & Distance(de2_1, d2) < 2


                      from Dot de3 in this
                      where de3.ValidMove & Distance(de1, de3) < 2
                                          & Distance(de2, de3) < 2
                                          & Distance(de1_1, de3) < 2
                                          & Distance(de2_1, de3) < 2
                                          & Distance(d1, de3) >= 2
                                          & Distance(d2, de3) >= 2

                      select new Dot(de3.x, de3.y, NumberPattern: 777, Rating: 1);
            }
            else
            {
                qry = from Dot d1 in this
                      where d1.Own == Owner && !d1.Blocked

                      from Dot d2 in this
                      where d2.Own == d1.Own && !d2.Blocked && Distance(d1, d2) < 4.5f & Distance(d1, d2) >= 2.5f

                      from Dot de1 in this
                      where de1.ValidMove & Distance(d1, de1) == 1

                      from Dot de2 in this
                      where de2.ValidMove & Distance(d2, de2) == 1

                      from Dot de1_1 in this
                      where de1_1.ValidMove & Distance(de1_1, de1) == 1 & Distance(de1_1, d1) < 2

                      from Dot de2_1 in this
                      where de2_1.ValidMove & Distance(de2_1, de2) == 1 & Distance(de2_1, d2) < 2


                      from Dot de3 in this
                      where de3.ValidMove & Distance(de1, de3) < 2
                                          & Distance(de2, de3) < 2
                                          & Distance(de1_1, de3) < 2
                                          & Distance(de2_1, de3) < 2
                                          & Distance(d1, de3) >= 2
                                          & Distance(d2, de3) >= 2

                      select new Dot(de3.x, de3.y, NumberPattern: 777, Rating: 2);
            }
            return qry.Distinct(new DotEq()).ToList();
        }
    }
}
