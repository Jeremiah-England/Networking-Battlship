//-----------------------------------------------------------
//File:   MainWindow.xaml.cs
//Desc:   This the gui for a Battleship server.
//----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace BattleshipServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ServerCommunicationManager commManager;

        /// <summary>
        /// Just the initalizer for the window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method listens for clients and connects the to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            commManager = new ServerCommunicationManager(this);
            Log("Listening on port " + ServerCommunicationManager.Port);

            while (true)
            {
                TcpClient client = await commManager.WaitForClientAsync();
                Log("Received incoming connection.");
                commManager.HandleClientAsync(client);
            }
        }

        /// <summary>
        /// Just logs a message on the server window.
        /// </summary>
        /// <param name="msg"></param>
        public void Log(string msg)
        {
            txtLog.Text += msg + "\n";
        }
    }
}
