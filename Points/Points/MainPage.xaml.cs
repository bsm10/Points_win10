using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Points
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //int boardWidth;// = 12;
        //int boardHeight; //= 15;
        private int player_move;//переменная хранит значение игрока который делает ход
        bool autoPlay;
        //int game_result;
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();

        private DateTimeOffset startTime;
        private DateTimeOffset lastTime;

        //private bool autoplay;
        ApplicationView appView;
        ApplicationViewTitleBar titleBar;
        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            // получаем ссылку на внешний вид приложения
            appView = ApplicationView.GetForCurrentView();
            // минимальный размер 
            appView.SetPreferredMinSize(new Size(480, 800));
            // минимальные границы
            appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            // установка заголовка
            appView.Title = "Points!";
            // получаем ссылку на TitleBar
            titleBar = appView.TitleBar;
            // установка цвета панели
            titleBar.BackgroundColor = Colors.LightSteelBlue;

            //if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //    HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        //private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        //{
        //    if (Frame.CanGoBack)
        //        Frame.GoBack();
        //    else
        //    {
        //        game.SaveGame();
        //        Application.Current.Exit(); // выход из приложения       
        //    }

        //}

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            canvas.Height = ActualHeight - cmdBar.ActualHeight;
            canvas.Width = ActualWidth;
            double Xres = canvas.Width;
            double Yres = canvas.Height;
            DrawSession.CanvasCtrl = canvas;
            GameEngine.BoardWidth = 10;
            double scl_coef = Yres / Xres;
            GameEngine.BoardHeight = (int)Math.Round(GameEngine.BoardWidth * scl_coef);
            GameEngine.NewGame();
            await GameEngine.LoadGame();

            //player_move = 2;
            player_move = GameEngine.LastMove.Own == 1 ? 2 : 1;

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
        public IAsyncOperation<int> MoveAsync2(int Player)
        {
            // Use a lock to prevent the ResetAsync method from modifying the game 
            // state at the same time that a different thread is in this method.
            //lock (_lockObject)
            //{
            return AsyncInfo.Run(cancellationToken => Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return -1;

                Dot pl_move = GameEngine.PickComputerMove(GameEngine.LastMove, ct);

                if (pl_move == null)
                {
                    //MessageBox.Show("You win!!! \r\n" + game.Statistic());
                    GameEngine.NewGame();
                    return 1;
                }
                pl_move.Own = Player;

                if (GameEngine.MakeMove(pl_move) != -1)
                {
                    //canvas.Invalidate();
                    player_move = Player == 1 ? 2 : 1;
                }
                else return -1;


                if (GameEngine.GameOver())
                {
                        //StatusMsg.DrawMsg("Game over! \r\n" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen);
                        //await game.Pause(5);
                        GameEngine.NewGame();
                        //StatusMsg.DrawMsg("New game started!" + game.Statistic(), 0, boardHeight + GameEngine.startY, Colors.DarkOliveGreen); 
                        // await game.Pause(1);

                        return 1;
                }
                StatusMsg.ColorMsg = player_move == 1 ? GameEngine.colorGamer1 : GameEngine.colorGamer2;
                StatusMsg.textMsg = "Move player" + player_move + "...";

                return 0;

            }, cancellationToken));
            //}
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken ct; 
        public IAsyncAction MoveAsync(int player)
        {
                ct = tokenSource.Token;
                var x = Task.Run(async () =>
                {
                    try
                    {
                        if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                        int result = await MoveGamer(player, ct);
                        return result;
                    }
                    catch (OperationCanceledException)
                    {
                        StatusMsg.textMsg = "Move canceled!";
                        tokenSource.Dispose();
                        tokenSource = new CancellationTokenSource();
                        return 0;
                    }
                }, ct).AsAsyncAction();

            return x;



    //return AsyncInfo.Run(cancellationToken => Task.Run(async () =>
    //{
    //    if (cancellationToken.IsCancellationRequested) return;
    //    await MoveGamer(player);
    //}, cancellationToken));

}

        private async void Timer2_Tick(object sender, object e)
        {
            if (autoPlay)
            {
                //============Ход компьютера вместо игрока=================
                if (player_move == 1)
                {
                    player_move = 3;
                    await MoveAsync(1);
                    player_move = 2;
                    DrawSession.CanvasCtrl.Invalidate();
                }
                //============Ход компьютера=================
                if (player_move == 2)
                {
                    player_move = 3;
                    await MoveAsync(2);
                    player_move = 1;
                    DrawSession.CanvasCtrl.Invalidate();
                }

            }
        }

        private void Timer_Tick(object sender, object e)
        {
            ////============Ход компьютера=================
            //if (player_move == 2)
            //{
            //    //if (await MoveGamer(2) > 0) return;
            //    await MoveAsync(2);

            //}

        }

        private async void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            //var session = args.DrawingSession;
            args.DrawingSession.Clear(Colors.White);
            //session.Clear(Colors.White);
            DrawSession.CanvasCtrl = sender;
            DrawSession.CanvasDrawingSession = args.DrawingSession;
            try
            {
                GameEngine.DrawGame(sender, args.DrawingSession);
            }
            catch (Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message + "r/n/canvas_Draw");
                await dlg.ShowAsync();
            }
        }

        private async void canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            var mpos = e.GetPosition(q);
            GameEngine.MousePos = GameEngine.TranslateCoordinates(mpos);
            Dot dot = new Dot((int)GameEngine.MousePos.X, (int)GameEngine.MousePos.Y);
            if (GameEngine.MousePos.X > GameEngine.startX - 0.5f & GameEngine.MousePos.Y > GameEngine.startY - 0.5f)
            {
                //if (e.PointerDeviceType == PointerDeviceType.Touch | e.PointerDeviceType == PointerDeviceType.Pen)
                #region Ходы игроков
                if (player_move == 1 | player_move == 0)
                {
                    player_move = 1;
                    if (await MoveGamer(1, ct, new Dot((int)GameEngine.MousePos.X, (int)GameEngine.MousePos.Y, 1)) == 0)
                    {
                        player_move = 2;
                        DrawSession.CanvasCtrl.Invalidate();
                    }
                }
                //============Ход компьютера=================
                if (player_move == 2)
                {
                    player_move = 3;//для того чтобы лишнюю точку не поставил человек
                    await MoveAsync(2);
                    player_move = 1;
                    DrawSession.CanvasCtrl.Invalidate();
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
        private async Task<int> MoveGamer(int Player, CancellationToken? cancellationToken, Dot pl_move = null)
        {
            if (pl_move == null) pl_move = GameEngine.PickComputerMove(GameEngine.LastMove, cancellationToken);
            if (pl_move == null)
            {
                //MessageBox.Show("You win!!! \r\n" + game.Statistic());
                GameEngine.NewGame();
                return 1;
            }
            pl_move.Own = Player;

            if (GameEngine.MakeMove(pl_move) == -1) return -1;

            if (GameEngine.GameOver())
            {
                StatusMsg.textMsg = "Game over! \r\n" + GameEngine.Statistic();
                await GameEngine.Pause(5);
                GameEngine.NewGame();
                StatusMsg.textMsg = "New game started!" + GameEngine.Statistic(); 
                await GameEngine.Pause(1);

                return 1;
            }
            StatusMsg.ColorMsg = player_move == 1 ? GameEngine.colorGamer2 : GameEngine.colorGamer1;
            StatusMsg.textMsg = player_move == 1 ? "Move computer..." : "Your move!";
            appView.Title = StatusMsg.textMsg;
            titleBar.BackgroundColor = StatusMsg.ColorMsg;
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
            if(!tokenSource.IsCancellationRequested)tokenSource.Cancel();
            GameEngine.NewGame();
            tokenSource = new CancellationTokenSource();
        }

        private void SaveGame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GameEngine.SaveGame();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        private async void LoadGame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (tokenSource.IsCancellationRequested) tokenSource.Cancel();
            await GameEngine.LoadGame();
            //tokenSource = new CancellationTokenSource();
        }
        private async Task DelayAsync(int milliseconds, CancellationToken? cancellationToken = null)
        {
            // Delay only in the running app (not in unit tests). 
            if (Window.Current != null)
            {
                if (!cancellationToken.HasValue) await Task.Delay(milliseconds);
                else await Task.Delay(milliseconds, cancellationToken.Value);
            }
        }
        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement q = sender as CanvasControl;
            var mpos = e.GetCurrentPoint(q);
            GameEngine.MousePos = GameEngine.TranslateCoordinates(mpos.Position);
            Dot dot = new Dot((int)GameEngine.MousePos.X, (int)GameEngine.MousePos.Y);

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
                    GameEngine.UndoDot(dot);
#endif
                }

            }
        }

        private void Autoplay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            autoPlay = autoPlay == true ? false : true;
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }
    }
}
