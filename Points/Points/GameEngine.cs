using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;


namespace Points
{
    public static class GameMessages
    {
        public static string Message {get;set;}

    }
    public partial class GameEngine
    {

        //-------------------------------------------------
        //==========================================================================================================
        private GameDots _gameDots;//Основной массив, где хранятся все поставленные точки. С єтого массива рисуются все точки
        //===========================================================================================================
        public GameDots gameDots 
        {
            get 
              {
                return _gameDots;
              }
        }

        public Dot DOT(int x, int y)
        {
            return _gameDots[x,y];
        }
        public Dot DOT(Dot d)
        {
            return _gameDots[d.x, d.y];
        }

        private string status=string.Empty;
        public string  Status
        {
            get{return status;}
            set{status=value;}
        }
        //private Form2 f;
        //public DebugForm DebugWindow = new DebugForm();

        //public bool Autoplay
        //{
        //    get { return DebugWindow.rbtnHand.Checked; }
        //}


        //=========== цвета, шрифты ===================================================
        public Color colorGamer1 = Colors.Tomato; //Properties.Settings.Default.Color_Gamer1,
        public Color colorGamer2 = Colors.MediumSlateBlue;//Properties.Settings.Default.Color_Gamer2,
        public Color colorCursor = Color.FromArgb(50, 50, 200, 50);// Properties.Settings.Default.Color_Cursor;
        private float PointWidth = 0.20f;
        public Color colorBoard = Color.FromArgb(255, 150, 200, 200);//(Color.DarkSlateBlue, 0.08f);
        public Color colorDrawBrush = Colors.MediumPurple;
        //public Color drB = Colors.MediumSeaGreen;


        //public ICanvasBrush SolidBrush;
        ////private SolidBrush drawBrush = new SolidBrush(Colors.MediumPurple);
        //public Font drawFont = new Font("Arial", 0.22f);
        public bool Redraw { get; set; }
        public int iScaleCoef = 1;//-коэффициент масштаба
        public float startX = -0.5f, startY = -0.5f;

        public Point MousePos;

        private int _pause = 10;

        private CanvasControl pbxBoard;

        public Dot LastMove
        {
            get
            {
                return gameDots.LastMove;
            }
        }


        public GameEngine(CanvasControl CanvasGame, int boardWidth, int boardHeight)
        {
            pbxBoard = CanvasGame;
            NewGame(boardWidth, boardHeight);
        }

        //  ************************************************

        public int pause
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
        public async Task Pause(double sec)
        {
            pbxBoard.Invalidate();
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }

        public Dot PickComputerMove(Dot LastMove)
        {
            return gameDots.PickComputerMove(LastMove);
        }
        public int MakeMove(Dot MoveDot)
        {
            return gameDots.MakeMove(MoveDot);
        }
        public bool GameOver()
        {
            return gameDots.Board_ValidMoves.Count == 0;
        }

        public string Statistic()
        {
            var q5 = from Dot d in _gameDots where d.Own == 1 select d;
            var q6 = from Dot d in _gameDots where d.Own == 2 select d;
            var q7 = from Dot d in _gameDots where d.Own== 1 & d.Blocked select d;
            var q8 = from Dot d in _gameDots where d.Own == 2 & d.Blocked select d;
            return q8.Count().ToString() + ":" + q7.Count().ToString();
        }
        public void Statistic(int x, int y)
        {
                #if DEBUG
            DebugWindow.txtDotStatus.Text = "Blocked: " + _gameDots[x, y].Blocked + "\r\n" +
                              "BlokingDots.Count: " + _gameDots[x, y].BlokingDots.Count + "\r\n" +
                              "NeiborDots.Count: " + _gameDots[x, y].NeiborDots.Count + "\r\n" +
                              "Rating: " + _gameDots[x, y].Rating + "\r\n" +
                              "IndexDot: " + _gameDots[x, y].IndexDot + "\r\n" +
                              "IndexRelation: " + _gameDots[x, y].IndexRelation + "\r\n" +
                              "Own: " + _gameDots[x, y].Own + "\r\n" +
                              "X: " + _gameDots[x, y].x + "; Y: " + _gameDots[x, y].y;
               #endif
        }
        public void NewGame(int boardWidth, int boardHeigth)
        {
            //if (gameDots!=null && DebugWindow!=0)

            _gameDots = new GameDots(boardWidth,boardHeigth); 
            //startX = -0.5f;
            //startY = -0.5f;
            Redraw=true;
            pbxBoard.Invalidate();
        }
        //------------------------------------------------------------------------------------

        public void ResizeBoard(int newSizeWidth, int newSizeHeight)//изменение размера доски
        {
            if (newSizeWidth < 5) newSizeWidth = 5;
            else if (newSizeWidth > 40) newSizeWidth = 40;
            if (newSizeHeight < 5) newSizeHeight = 5;
            else if (newSizeHeight > 40) newSizeHeight = 40;

            gameDots.BoardHeight = newSizeHeight;
            gameDots.BoardWidth = newSizeWidth;
            NewGame(newSizeWidth,newSizeHeight);
            pbxBoard.Invalidate();
        }


        private List<List<Dot>> ListRotatePatterns(List<Dot> listPat)
        {
            List<List<Dot>> lstlstPat = new List<List<Dot>>();
            
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate90(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate_Mirror_Horizontal(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate90(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate_Mirror_Horizontal(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate90(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate_Mirror_Horizontal(listPat);
            lstlstPat.Add(listPat);
            listPat = _gameDots.Rotate90(listPat);
            lstlstPat.Add(listPat);

            return lstlstPat;
        }
      
        public Dot this[int i, int j]//Индексатор возвращает элемент из массива по его индексу
        {
            get
            {
                return gameDots[i, j];
            }
        }
        public Dot this[Dot dot]//Индексатор возвращает элемент из массива по его индексу
        {
            get
            {
                return gameDots[dot.x, dot.y];
            }
        }
        #region SAVE_LOAD Game
        public async void SaveGame()
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

        public async void LoadGame()
        {
            _gameDots.Clear();
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
                    d = new Dot((int)reader.ReadByte(), (int)reader.ReadByte(), (int)reader.ReadByte());
                    _gameDots.MakeMove(d);
                }
                reader.Dispose();
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

        public void UndoDot (Dot dot_move)
        {
            gameDots.UndoMove(dot_move);
        }
        //==========================================================================

    }
}
