//-----------------------------------------------------------
//File:   MainWindow.xaml.cs
//Desc:   This is where all the gui processes take place for 
//        the client. 
//----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BattleshipModel;
using BattleshipComm;
using System.Windows.Threading;

namespace BattleshipClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameController ctrl = new GameController();
        ClientCommunicationManager client = new ClientCommunicationManager();

        /// <summary>
        /// Just the initializing method.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This load method creates three stackpanels within the base panal of the window.
        /// The first one contains some buttons, the other two have arrays of buttons. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri("iowa_1984.jpg", UriKind.Relative));
            ImageBrush imgBrush = new ImageBrush(img.Source);
            imgBrush.Stretch = Stretch.Uniform;
            Background = imgBrush;
            // grid 0 is player; grid 1 is computer
            for (int i = 0; i < 2; ++i)
            {
                StackPanel oceanPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(20, 0, 20, 0)
                };
                panel.Children.Add(oceanPanel);
                for (int row = 0; row < 5; ++row)
                {
                    StackPanel rowPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    oceanPanel.Children.Add(rowPanel);
                    for (int col = 0; col < 5; ++col)
                    {
                        int btnNum = row * 3 + col;
                        Button b = MakeButton(i == 1);
                        b.Tag = new Location() { Row = row, Column = col };
                        rowPanel.Children.Add(b);
                    }
                }
            }
            UpdateBoardAsync();
        }

        /// <summary>
        /// This is the event handler for if any of the buttons are clicked. It is the heart of all
        /// the GUI logic. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            Location loc = (Location)btn.Tag;            

            // stop process if the square has already been selected.
            if (ctrl.computerBoard.GetSquareStatus(loc) == SquareStatus.Hit)
                return;
            if (ctrl.computerBoard.GetSquareStatus(loc) == SquareStatus.Guessed)
                return;

            PlayerMoveRequestMessage msg = new PlayerMoveRequestMessage();
            msg.Loc = loc;
            try
            {
                PlayerMoveResponseMessage response = (PlayerMoveResponseMessage)(await client.SendMessageAsync(msg));
                if (response != null)
                    {
                        ctrl.UpdatePlayerGuess(response.PlayerLocation);
                        ctrl.UpdateComputerGuess(response.ComputerLocation);
                    }

            } catch
            {
                ctrl.CalculateComputerGuess();
                ctrl.UpdatePlayerGuess(loc);
            }
            
            UpdateBoardAsync();
        }

        /// <summary>
        /// This method is used by the Window_Load method when it contructs the board.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        private Button MakeButton(bool enabled)
        {
            Button b = new Button();
            b.BorderBrush = new SolidColorBrush(Colors.Black);
            b.Width = 50;
            b.Height = 50;
            b.Margin = new Thickness(10);
            
            if (enabled) b.Click += btn_Click;
            return b;
        }

        /// <summary>
        /// This method iterates through all the buttons and updates the gui to match the model.
        /// </summary>
        private async void UpdateBoardAsync()
        {
            if(client.tcpClient.Connected)
            {
                GameStatusRequestMessage request = new GameStatusRequestMessage();
                GameStatusResponseMessage response = (GameStatusResponseMessage)(await client.SendMessageAsync(request));
                ctrl = response.Controller;
            }

            foreach (StackPanel stack in ((StackPanel)panel.Children[3]).Children)
            {
                foreach (Button btn in stack.Children)
                        UpdateButtonComputerSide(btn);
                }
            foreach (StackPanel stack in ((StackPanel)panel.Children[2]).Children)
            {
                foreach (Button btn in stack.Children)
                    UpdateButtonPlayerSide(btn);
            }
            if (ctrl.LastComputerGuess != null)
            {
                ((Button)((StackPanel)((StackPanel)panel.Children[2]).Children[ctrl.LastComputerGuess.Row]).Children[ctrl.LastComputerGuess.Column]).Background = Brushes.LightYellow;
            }
            if(ctrl.gameIsOver || ctrl.IsGameOver())
            {
                AnimateEnding();
            }
        }

        //private void UpdateButtonComputerSide(Button btn)
        //{
        //    Location btnLoc = (Location)btn.Tag;
        //    SquareStatus btnSquare = ctrl.computerBoard.GetSquareStatus(btnLoc);
        //    if (btnSquare == SquareStatus.Guessed)
        //        btn.Background = Brushes.DeepSkyBlue;
        //    else if (btnSquare == SquareStatus.Hit)
        //        btn.Background = Brushes.Firebrick;
        //    else
        //        btn.Background = Brushes.LightGray;
        //}

        /// <summary>
        /// This method updates the button if it's on the computer side. 
        /// </summary>
        /// <param name="btn"></param>
        private void UpdateButtonComputerSide(Button btn)
        {
            Location btnLoc = (Location)btn.Tag;
            SquareStatus btnSquare = ctrl.computerBoard.GetSquareStatus(btnLoc);
            if (btnSquare == SquareStatus.Guessed)
            {
                btn.Background = Brushes.DeepSkyBlue;
                btn.Content = "G";
            }
            else if (btnSquare == SquareStatus.Hit)
                btn.Background = Brushes.Firebrick;
            else if (btnSquare == SquareStatus.Occupied)
                btn.Background = Brushes.DarkGray;
            else
                btn.Background = Brushes.DeepSkyBlue;
        }

        /// <summary>
        /// this updates the button color if its on the player's side.
        /// </summary>
        /// <param name="btn"></param>
        private void UpdateButtonPlayerSide(Button btn)
        {
            Location btnLoc = (Location)btn.Tag;
            SquareStatus btnSquare = ctrl.playerBoard.GetSquareStatus(btnLoc);
            if (btnSquare == SquareStatus.Guessed)
            {
                btn.Background = Brushes.DeepSkyBlue;
                btn.Content = "G";
            }
            else if (btnSquare == SquareStatus.Hit)
                btn.Background = Brushes.Firebrick;
            else if (btnSquare == SquareStatus.Occupied)
                btn.Background = Brushes.DarkGray;
            else
                btn.Background = Brushes.DeepSkyBlue;
        }

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if(ctrl.gameIsOver && e.Key == Key.F2)
        //    {
        //        panel.Children.Clear();
        //        ctrl = new GameController();
        //        this.Window_Loaded(sender, e);
        //    }
        //    Console.WriteLine(e.Key.ToString());
        //}

        /// <summary>
        /// This method creates a new window which informs the user who has won the game.
        /// </summary>
        private void AnimateEnding()
        {
            string msg;
            if (ctrl.winner == "player")
                msg = "You Win!";
            else
                msg = "You Loose";

            TextBox gameOver = new TextBox()
            {
                Margin = new Thickness(40, 0, 40, 0),
                Text = msg,
                TextAlignment = TextAlignment.Center,
                FontSize = 70,
                FontWeight = FontWeights.ExtraBold,
                Background = Brushes.Red,
            };

            Window wind = new Window()
            {
                Title = "Game Over",
                Height = 150,
                Width = 500,
                ForceCursor = true,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Visibility = Visibility.Visible,
                Background = Brushes.DarkBlue,
                Name = "wind",
                Content = gameOver
            };
        }

        //private void Wind_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (ctrl.gameIsOver && e.Key == Key.F2)
        //    {
        //        panel.Children.RemoveAt(3);
        //        panel.Children.RemoveAt(2);
        //        ctrl = new GameController();
        //        this.Window_Loaded(sender, e);
        //    }
        //    ((Window)sender).Close();
        //}

        /// <summary>
        ///  This method is the event handler for the 'connect' button. It connects the client to a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (client.tcpClient.Connected)
                return;
            client.hostname = txtServerName.Text;
            await client.ConnectToServerAsync();
            UpdateBoardAsync();
        }

        /// <summary>
        /// This method gets the data from the server and refreshes the gameboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateBoardAsync();
        }
    }
}
