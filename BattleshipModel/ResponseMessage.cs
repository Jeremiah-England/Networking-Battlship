//-----------------------------------------------------------
//File:   ResponseMessages.cs
//Desc:   Herein is a set of classes which is set up like the 
//        command pattern, only in this case a 'command' is 
//        a ResponseMessage.
//----------------------------------------------------------

using BattleshipModel;
using System;

namespace BattleshipComm
{
    // the master classe. Corrosponds to 'Command' in the command pattern.
    public class ResponseMessage
    {
      
    }

    /// <summary>
    /// This is a response to a move request by the player. It contains the information
    /// necesary to update the board: What the player's guess was, and what the computer's
    /// guess was.
    /// </summary>
    public class PlayerMoveResponseMessage: ResponseMessage
    {
        public Location ComputerLocation { get; set; }

        public Location PlayerLocation { get; set; }
        
    }

    /// <summary>
    /// This class is for when the player asks for the state of the game. 
    /// I decided just to hand over the entire controler.
    /// </summary>
    public class GameStatusResponseMessage: ResponseMessage
    {
        public GameController Controller { get; set; }
        
    }
}