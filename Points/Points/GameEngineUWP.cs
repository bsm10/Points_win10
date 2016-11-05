using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using System.Threading;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace DotsGame
{
    public static class Toast
    {
        /// <summary>
        /// Shows the specified text in a toast notification if notifications are enabled.
        /// </summary>
        /// <param name="text">The text to show.</param>
        public static void Show(string text)
        {
            const string template =
                "<toast duration='short'><visual><binding template='ToastText01'>" +
                "<text id='1'>{0}</text></binding></visual></toast>";
            var toastXml = new XmlDocument();
            toastXml.LoadXml(string.Format(template, text));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }

    public static partial class GameEngineUWP
    {

        //-------------------------------------------------
        //==========================================================================================================
        private static GameDots _gameDots;//Основной массив, где хранятся все поставленные точки. С єтого массива рисуются все точки
        //===========================================================================================================
        public static GameDots gameDots 
        {
            get 
              {
                return _gameDots;
              }
        }

        public static float startX = -0.5f;
        public static float startY = -0.5f;
        //public static float BoardWidth;
        //public static float BoardHeight;

        public static Dot DOT(int x, int y)
        {
            return _gameDots[x,y];
        }
        public static Dot DOT(Dot d)
        {
            return _gameDots[d.x, d.y];
        }

        private static string dbgInfo=string.Empty;
        public static string DbgInfo
        {
            get{return dbgInfo;}
            set{dbgInfo=value;}
        }

        private static int _pause = 10;


        public static Dot LastMove
        {
            get
            {
                return gameDots.LastMove;
            }
        }

        public static int BoardWidth { get; set; }
        public static int BoardHeight { get; set; }

        public static void NewGame()
        {
            _gameDots = new GameDots(BoardWidth, BoardHeight);
            Redraw = false;
            DrawSession.DotsForDrawing.Clear();

        }

        //  ************************************************

        public static int pause
        {
            get
            {
                return _pause;
                //
            }
            set
            {
                _pause = value;
            }
        }
        public static  async Task Pause(double sec)
        {
            //DrawSession.CanvasCtrl.Invalidate();
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
        public static async Task<int> MoveAsync(int player, CancellationToken? cancellationToken, Dot pl_move = null)
        {
            //Redraw = false;
            int result = await gameDots.Move(player, cancellationToken, pl_move);
           // Redraw = true;
            return result;


        }

        //public static Dot PickComputerMove(Dot LastMove, CancellationToken? cancellationToken)
        //{
        //    Dot result = gameDots.PickComputerMove(LastMove, cancellationToken);
        //    return result;
        //}
        //public static int MakeMove(Dot MoveDot)
        //{
        //    return gameDots.MakeMove(MoveDot, addForDraw: true);
        //}
        public static bool GameOver()
        {
            return gameDots.IsGameOver;
        }

        public static string Statistic()
        {
            var q5 = from Dot d in _gameDots where d.Own == 1 select d;
            var q6 = from Dot d in _gameDots where d.Own == 2 select d;
            var q7 = from Dot d in _gameDots where d.Own== 1 & d.Blocked select d;
            var q8 = from Dot d in _gameDots where d.Own == 2 & d.Blocked select d;
            return q8.Count().ToString() + ":" + q7.Count().ToString();
        }
        public static void Statistic(int x, int y)
        {
#if DEBUG
            DbgInfo = "Blocked: " + _gameDots[x, y].Blocked + "\r\n" +
                              "BlokingDots.Count: " + _gameDots[x, y].BlokingDots.Count + "\r\n" +
                              "NeiborDots.Count: " + _gameDots[x, y].NeiborDots.Count + "\r\n" +
                              "Rating: " + _gameDots[x, y].Rating + "\r\n" +
                              "IndexDot: " + _gameDots[x, y].IndexDot + "\r\n" +
                              "IndexRelation: " + _gameDots[x, y].IndexRelation + "\r\n" +
                              "Own: " + _gameDots[x, y].Own + "\r\n" +
                              "X: " + _gameDots[x, y].x + "; Y: " + _gameDots[x, y].y;
               #endif
        }

        public static void ResizeBoard(int newSizeWidth, int newSizeHeight)//изменение размера доски
        {
            if (newSizeWidth < 5) newSizeWidth = 5;
            else if (newSizeWidth > 40) newSizeWidth = 40;
            if (newSizeHeight < 5) newSizeHeight = 5;
            else if (newSizeHeight > 40) newSizeHeight = 40;

            gameDots.BoardHeight = newSizeHeight;
            gameDots.BoardWidth = newSizeWidth;
            //NewGame(newSizeWidth,newSizeHeight);
            //DrawSession.CanvasCtrl.Invalidate();
        }
     
        #region SAVE_LOAD Game
        public static async void SaveGame()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.CreateFileAsync(@"\dots.dts", CreationCollisionOption.ReplaceExisting);

                // создаем объект BinaryWriter
                using (BinaryWriter writer = new BinaryWriter(File.Open(file.Path, FileMode.Create)))
                {

                    for (int i = 0; i < _gameDots.ListMoves.Count; i++)
                    {
                        writer.Write((byte)_gameDots.ListMoves[i].x);
                        writer.Write((byte)_gameDots.ListMoves[i].y);
                        writer.Write((byte)_gameDots.ListMoves[i].Own);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog d = new MessageDialog(ex.Message + " saveStringToLocalFile");
                await d.ShowAsync();
            }
        }

        public static async Task LoadGame()
        {
            _gameDots.Clear();
            DrawSession.DotsForDrawing.Clear();
            Dot d = null;
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.CreateFileAsync(@"\dots.dts", CreationCollisionOption.OpenIfExists);

                // создаем объект BinaryReader
                BinaryReader reader = new BinaryReader(File.Open(file.Path, FileMode.Open));
                // пока не достигнут конец файла считываем каждое значение из файла
                while (reader.PeekChar() > -1)
                {
                    d = new Dot(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    _gameDots.MakeMove(d, addForDraw: true);
                    
                    //DrawSession.DotsForDrawing.Add(_gameDots[d.x,d.y]);
                }
                reader.Dispose();
                //_gameDots._DotsForDrawing = _gameDots.Dots.ToList();
            }
            catch (Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message + " LoadGame");
                await dlg.ShowAsync();
            }

        }
        #endregion

        //==========================================================================
        #region SAVE_LOAD Game
        //public async void SaveGame()
        //{
        //    byte[] saveData = new byte[list_moves.Count*3];
        //    int i = 0;
        //    foreach(Dot d in list_moves)
        //    {
        //        saveData[i++] = (byte)d.x;
        //        saveData[i++] = (byte)d.y;
        //        saveData[i++] = (byte)d.Own;
        //        //i++;
        //    }
        //    try
        //    {
        //        var folder = ApplicationData.Current.LocalFolder;
        //        var file = await folder.CreateFileAsync(@"\dots.dts", CreationCollisionOption.ReplaceExisting);
        //        await FileIO.WriteBytesAsync(file, saveData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageDialog d = new MessageDialog(ex.Message + " saveStringToLocalFile");
        //        await d.ShowAsync();
        //    }
        //}
        #endregion

        //public static void UndoDot (Dot dot_move)
        //{
        //    gameDots.UndoMove(dot_move);
        //}
        //==========================================================================

    }
}
