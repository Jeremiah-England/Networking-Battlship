//-----------------------------------------------------------
//File:   RequestMessage.cs
//Desc:   This is a set of classes which follows the command
//        pattern where a 'command' is a request from the client.
//----------------------------------------------------------

using System;
using BattleshipModel;

namespace BattleshipComm
{
    /// <summary>
    /// This is corrosponds to the 'Command' class in the command pattern
    /// </summary>
    public abstract class RequestMessage
    {
        // Executes in server program to process the request
        public abstract ResponseMessage Execute(GameController ctrl);
    }

    /// <summary>
    /// This is a request from the client for a player to guess a certain location on the computer's board.
    /// </summary>
    public class PlayerMoveRequestMessage : RequestMessage
    {
        public Location Loc { get; set; }

        public override ResponseMessage Execute(GameController ctrl)
        {
            PlayerMoveResponseMessage responseMsg = new PlayerMoveResponseMessage();
            if (ctrl.computerBoard.GetSquareStatus(Loc) == SquareStatus.Occupied || 
                ctrl.computerBoard.GetSquareStatus(Loc) == SquareStatus.Empty)
            {
                ctrl.UpdatePlayerGuess(Loc);
                responseMsg.PlayerLocation = this.Loc;
                if (!ctrl.IsGameOver())
                {
                    responseMsg.ComputerLocation = ctrl.CalculateComputerGuess();
                }
            }
            return responseMsg; // returns null for both locaions if the reqest is invalid. 
        }

    }

    /// <summary>
    /// This requests the gamestate. 
    /// </summary>
    public class GameStatusRequestMessage : RequestMessage
    {
        public override ResponseMessage Execute(GameController ctrl)
        {
            return new GameStatusResponseMessage() { Controller = ctrl };
        }
    }
}