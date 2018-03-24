//-----------------------------------------------------------
//File:   ClientCommunicationManager.cs
//Desc:   This class contains all the logic for the processing of
//        request messages on the client end and sending those to
//        the server.
//----------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BattleshipComm;

namespace BattleshipClient
{
    class ClientCommunicationManager
    {
        public string hostname = "localhost";
        int port = 5500;

        public TcpClient tcpClient;
        StreamReader rd;
        StreamWriter wr;

        /// <summary>
        /// Just a construcor which makes the communcation manager. 
        /// It creates a new tcpClient.
        /// </summary>
        public ClientCommunicationManager()
        {
            tcpClient = new TcpClient();
        }

        /// <summary>
        /// Connects to the server async-wise.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToServerAsync()
        {
            await tcpClient.ConnectAsync(hostname, port);
            rd = new StreamReader(tcpClient.GetStream());
            wr = new StreamWriter(tcpClient.GetStream());
        }

        /// <summary>
        /// Sends the Request Message to the server and gets the response back asyncroniously
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<ResponseMessage> SendMessageAsync(RequestMessage msg)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string serializedMessage = JsonConvert.SerializeObject(msg, settings);
            await wr.WriteLineAsync(serializedMessage);
            await wr.FlushAsync();
            string response = await rd.ReadLineAsync();
            ResponseMessage responseMsg = JsonConvert.DeserializeObject(response, settings) as ResponseMessage;
            return responseMsg;
        }
    }
}
