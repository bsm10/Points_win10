using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Points
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Game game;
        int boardWidth = 7;
        int boardHeight = 10;
        private int player_move;//переменная хранит значение игрока который делает ход
        //private bool autoplay;

        public MainPage()
        {
            InitializeComponent();
            game = new Game(canvas, boardWidth, boardHeight);
        }

        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            session.Clear(Colors.Azure);
            game.DrawGame(sender,session);
        }

        private void canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            var mpos = e.GetPosition(q);
            game.MousePos = game.TranslateCoordinates(mpos);
            Dot dot = new Dot((int)game.MousePos.X, (int)game.MousePos.Y);
            if (game.MousePos.X > game.startX - 0.5f & game.MousePos.Y > game.startY - 0.5f)
            {
                switch (e.PointerDeviceType)
                {
                    case Windows.Devices.Input.PointerDeviceType.Mouse:
                        break;
                    case Windows.Devices.Input.PointerDeviceType.Pen:
                        break;
                    case Windows.Devices.Input.PointerDeviceType.Touch:
                        break;
                    default:
                        break;
                }

                        #region Ходы игроков
                        if (game.aDots[(int)game.MousePos.X, (int)game.MousePos.Y].Own > 0) return;//предовращение хода если клик был по занятой точке
                        if (player_move == 2 | player_move == 0)
                        {
                            if (MoveGamer(1, new Dot((int)game.MousePos.X, (int)game.MousePos.Y, 1)) > 0) return;
                            player_move = 1;
                            //Application.DoEvents();
                        }
                        //============Ход компьютера=================
                        if (player_move == 1)
                        {
                            game.Redraw = false;
                            if (MoveGamer(2) > 0) return;
                            game.Redraw = true;
                            player_move = 2;
                        }
                        #endregion


            }
        }
        private int MoveGamer(int Player, Dot pl_move = null)
        {
            //toolStripStatusLabel2.ForeColor = Player == 1 ? game.colorGamer1 : game.colorGamer2;
            //toolStripStatusLabel2.Text = "Ход игрока" + Player + "...";
            //Application.DoEvents();
            if (pl_move == null) pl_move = game.PickComputerMove(game.LastMove);
            if (pl_move == null)
            {
                //MessageBox.Show("You win!!! \r\n" + game.Statistic());
                game.NewGame(boardWidth, boardHeight);
                return 1;
            }
            pl_move.Own = Player;
            game.MakeMove(pl_move, Player);
            game.ListMoves.Add(pl_move);
            canvas.Invalidate();
            int pl = Player == 1 ? 2 : 1;
            //toolStripStatusLabel2.ForeColor = pl == 1 ? game.colorGamer1 : game.colorGamer2;
            //toolStripStatusLabel2.Text = "Ход игрока" + pl + "...";
            if (game.GameOver())
            {
                //MessageBox.Show("Game over! \r\n" + game.Statistic());
                return 1;
            }

            return 0;
        }
        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            PointerPoint ptrPt = e.GetCurrentPoint(q);
            txtCoordinate.Text = game.TranslateCoordinates(ptrPt.Position).ToString();
        }

    }
}
