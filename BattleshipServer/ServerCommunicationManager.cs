//-----------------------------------------------------------
//File:   ServerCommunictaionManager.cs
//Desc:   This class contianes all the logic for communication
//        on the server end.
//----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BattleshipModel;
using Newtonsoft.Json;
using BattleshipComm;

namespace BattleshipServer
{
    class ServerCommunicationManager
    {
        public const int Port = 5500;
        private MainWindow window;
        private TcpListener listener;

        GameController ctrl = new GameController();

        /// <summary>
        /// This is the constructor for the communication manager.
        /// It connected the server to the window, creates a TctListener,
        /// and starts it.
        /// </summary>
        /// <param name="window"></param>
        public ServerCommunicationManager(MainWindow window)
        {
            this.window = window;
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
        }

        /// <summary>
        /// Connects to the client asyncronously using the listener instance varible.
        /// </summary>
        /// <returns></returns>
        public async Task<TcpClient> WaitForClientAsync()
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            return client;
        }

        /// <summary>
        /// Takes a tcpClient starts the process on a thread for handling such a client.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public async Task HandleClientAsync(TcpClient tcpClient)
        {
            try
            {
                using (tcpClient)
                {
                    string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
                    window.Log("Received connection request from " + clientEndPoint);

                    NetworkStream networkStream = tcpClient.GetStream();
                    StreamReader reader = new StreamReader(networkStream);
                    StreamWriter writer = new StreamWriter(networkStream);

                    string request = await reader.ReadLineAsync();
                    while (request != null)
                    {
                        window.Log("Received data: " + request);
                        
                        string response = ProcessMessage(request);

                        window.Log("Transmitting data: " + response);
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                        request = await reader.ReadLineAsync();
                    }
                }

                // Client closed connection
                window.Log("Client closed connection.");
            }
            catch (Exception ex)
            {
                window.Log(ex.Message);
            }
        }

        /// <summary>
        /// Processes the message which comes from the client. That is, it takes the string json and
        /// makes the RequestMessage object which the string represents. Then it executes this method
        /// (and such an execution updates the ctrl and creates a response) then it returns a json version
        /// of the ReponseMessge.
        /// </summary>
        /// <param name="requestMsgStr"></param>
        /// <returns></returns>
        private string ProcessMessage(String requestMsgStr)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            RequestMessage requestMsg = JsonConvert.DeserializeObject(requestMsgStr, settings) as RequestMessage;
            ResponseMessage responseMsg = requestMsg.Execute(ctrl);
            return JsonConvert.SerializeObject(responseMsg, settings);
        }
    }
}
