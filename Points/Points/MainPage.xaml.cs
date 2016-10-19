using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
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
        GameEngine game;
        int boardWidth;// = 12;
        int boardHeight; //= 15;
        private int player_move;//переменная хранит значение игрока который делает ход
        bool autoPlay;
        //int game_result;
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();

        private DateTimeOffset startTime;
        private DateTimeOffset lastTime;

        //private bool autoplay;

        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            // получаем ссылку на внешний вид приложения
            ApplicationView appView = ApplicationView.GetForCurrentView();
            // минимальный размер 
            appView.SetPreferredMinSize(new Size(480, 800));
            // минимальные границы
            appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            // установка заголовка
            appView.Title = "Points!";
            // получаем ссылку на TitleBar
            ApplicationViewTitleBar titleBar = appView.TitleBar;
            // установка цвета панели
            titleBar.BackgroundColor = Colors.LightSteelBlue;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
            {
                game.SaveGame();
                Application.Current.Exit(); // выход из приложения       
            }
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            double Xres = canvas.ActualWidth;
            double Yres = canvas.ActualHeight;
            double scl_coef = (Yres-100) / Xres;

            boardWidth = 10;
            boardHeight =(int) Math.Round(boardWidth * scl_coef);;

            DrawSession.CanvasCtrl = canvas;
            
            game = new GameEngine(boardWidth, boardHeight);
            game.LoadGame();

            //player_move = 2;
            player_move = game.LastMove.Own == 1 ? 2 : 1 ;

            DispatcherTimerSetup();
        }

        public void DispatcherTimerSetup()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            startTime = DateTimeOffset.Now;
            lastTime = startTime;
            timer.Start();
            timer2 = new DispatcherTimer();
            timer2.Tick += Timer2_Tick;
            timer2.Interval = new TimeSpan(0, 0, 1);
            timer2.Start();

        }
        //public IAsyncOperation<int> MoveAsync(int Player)
        //{
        //    // Use a lock to prevent the ResetAsync method from modifying the game 
        //    // state at the same time that a different thread is in this method.
        //    //lock (_lockObject)
        //    //{
        //        return AsyncInfo.Run(cancellationToken => Task<int>.Run(() =>
        //        {
        //            if (cancellationToken.IsCancellationRequested) return null;

        //            Dot pl_move = game.PickComputerMove(game.LastMove);

        //            if (pl_move == null)
        //            {
        //                //MessageBox.Show("You win!!! \r\n" + game.Statistic());
        //                game.NewGame(boardWidth, boardHeight);
        //                return 1;
        //            }
        //            pl_move.Own = Player;

        //            if (game.MakeMove(pl_move) != -1)
        //            {
        //                canvas.Invalidate();
        //                player_move = Player == 1 ? 2 : 1;
        //            }
        //            else return -1;


        //            if (game.GameOver())
        //            {
        //                //StatusMsg.DrawMsg("Game over! \r\n" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen);
        //                //await game.Pause(5);
        //                game = new GameEngine(boardWidth, boardHeight);
        //                //StatusMsg.DrawMsg("New game started!" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen); 
        //               // await game.Pause(1);

        //                return 1;
        //            }
        //            StatusMsg.ColorMsg = player_move == 1 ? game.colorGamer1 : game.colorGamer2;
        //            StatusMsg.textMsg = "Move player" + player_move + "...";

        //            return 0;

        //        }, cancellationToken));
        //    //}
        //}
        public IAsyncAction MoveAsync(int player)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(1);
                MoveGamer(player);
            }).AsAsyncAction();
        }

        private void Timer2_Tick(object sender, object e)
        {
            if (autoPlay)
            {
                //============Ход компьютера вместо игрока=================
                if (player_move == 1)
                {
                    if (MoveGamer(1) > 0) return;
                }
            }
        }

        private  void Timer_Tick(object sender, object e)
        {
            ////============Ход компьютера=================
            //if (player_move == 2)
            //{
            //    //if (await MoveGamer(2) > 0) return;
            //    await MoveAsync(2);

            //}

        }

        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            //var session = args.DrawingSession;
            args.DrawingSession.Clear(Colors.White);
            //session.Clear(Colors.White);
            DrawSession.CanvasCtrl = sender;
            //DrawSession.CanvasDrawingSession = args.DrawingSession;
            game.DrawGame(sender, args.DrawingSession);
        }

        private async void canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            var mpos = e.GetPosition(q);
            game.MousePos = game.TranslateCoordinates(mpos);
            Dot dot = new Dot((int)game.MousePos.X, (int)game.MousePos.Y);
            if (game.MousePos.X > GameEngine.startX - 0.5f & game.MousePos.Y > GameEngine.startY - 0.5f)
            {
                //if (e.PointerDeviceType == PointerDeviceType.Touch | e.PointerDeviceType == PointerDeviceType.Pen)
                #region Ходы игроков
                if (player_move == 1 | player_move == 0)
                {
                    player_move = 1;
                    if (MoveGamer(1, new Dot((int)game.MousePos.X, (int)game.MousePos.Y, 1)) == 0)
                    {
                        //canvas.Invalidate();
                        player_move = 2;
                    }
                }
                //============Ход компьютера=================
                if (player_move == 2)
                {
                    //if (await MoveGamer(2) > 0) return;
                    player_move = 3;//для того чтобы лишнюю точку не поставил человек
                    await MoveAsync(2);
                    //canvas.Invalidate();
                    player_move = 1;
                }
                #endregion


            }
        }
        /// <summary>
        /// Ходы игроков
        /// </summary>
        /// <param name="Player">кто ходит 1-человек, 2-компьютер</param>
        /// <param name="pl_move">Устанавливается, исходя из координат тапа, если ходит человек, если ходит компьютер - null</param>
        /// <returns>-1 ошибка, недопустимый ход</returns>
        private int MoveGamer(int Player, Dot pl_move = null)
        {
            if (pl_move == null) pl_move = game.PickComputerMove(game.LastMove);
            if (pl_move == null)
            {
                //MessageBox.Show("You win!!! \r\n" + game.Statistic());
                game.NewGame(boardWidth, boardHeight);
                return 1;
            }
            pl_move.Own = Player;

            if (game.MakeMove(pl_move) != -1)
            {
                //canvas.Invalidate();
                //player_move = Player == 1 ? 2 : 1;
            }
            else return -1;


            if (game.GameOver())
            {
                //StatusMsg.DrawMsg("Game over! \r\n" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen);
                //await game.Pause(5);
                game = new GameEngine(boardWidth, boardHeight);
                //StatusMsg.DrawMsg("New game started!" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen); 
                //await game.Pause(1);

                return 1;
            }
            StatusMsg.ColorMsg = player_move == 1 ? game.colorGamer1 : game.colorGamer2;
            StatusMsg.textMsg = "Move player" + player_move + "...";

            return 0;
        }
        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            PointerPoint ptrPt = e.GetCurrentPoint(q);
            //txtCoordinate.Text = game.TranslateCoordinates(ptrPt.Position).ToString();
        }

        private void NewGame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            game = new GameEngine(boardWidth, boardHeight);
        }

        private void SaveGame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            game.SaveGame();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        private void LoadGame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            game.LoadGame();
            canvas.Invalidate();
        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            var mpos = e.GetCurrentPoint(q);
            game.MousePos = game.TranslateCoordinates(mpos.Position);
            Dot dot = new Dot((int)game.MousePos.X, (int)game.MousePos.Y);

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    // Left button pressed
                    //if (player_move == 1 | player_move == 0)
                    //{
                    //    player_move = 1;
                    //    if (MoveGamer(1, new Dot((int)game.MousePos.X, (int)game.MousePos.Y, 1)) > 0) return;
                    //}

                }
                else if (properties.IsRightButtonPressed)
                {
                    // Right button pressed
                }
                else if (properties.IsMiddleButtonPressed)
                {
#if DEBUG
                    game.UndoDot(dot);
#endif
                }

            }
        }

        private void Autoplay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            autoPlay = autoPlay == true ? false : true;
        }
    }
}
