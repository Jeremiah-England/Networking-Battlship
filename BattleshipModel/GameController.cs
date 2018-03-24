using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipModel
{
    public enum Direction { Vertical, Horizontal };
    public enum SquareStatus { Empty, Occupied, Guessed, Hit };
    // Empty and Occupied are 'hidden' while Guessed and Hit are their respective 
    // revealed variables. I placed this in the Model so that classes other than Board 
    // could use it. (I had it in the Board class originally, which didn't work.)

    // Tracks the overall state of the game
    // and provides top-level methods invoked by
    // user interface to control the game
    public class GameController
    {
        public bool gameIsOver = false;
        public string winner;
        public Board playerBoard = Board.MakeRandomBoard();
        public Board computerBoard = Board.MakeRandomBoard();
        public Location LastComputerGuess { get; set; }

        public void UpdatePlayerGuess(Location loc)
        {
            SquareStatus square = computerBoard.GetSquareStatus(loc);
            if (square == SquareStatus.Empty)
            {
                computerBoard.squares[loc.Row, loc.Column] = SquareStatus.Guessed;
            }
            else if (square == SquareStatus.Occupied)
            {
                computerBoard.squares[loc.Row, loc.Column] = SquareStatus.Hit;
                square = SquareStatus.Hit;
            }
            computerBoard.SinkShips();
        }

        public void UpdateComputerGuess(Location loc)
        {
            SquareStatus square = playerBoard.GetSquareStatus(loc);
            if (square == SquareStatus.Empty)
            {
                computerBoard.squares[loc.Row, loc.Column] = SquareStatus.Guessed;
                LastComputerGuess = loc;
            }
            else if (square == SquareStatus.Occupied)
            {
                computerBoard.squares[loc.Row, loc.Column] = SquareStatus.Hit;
                square = SquareStatus.Hit;
                LastComputerGuess = loc;

            }
            computerBoard.SinkShips();
        }

        public Location CalculateComputerGuess() // Just do it randomnly for now.
        {
            Random rand = new Random();
            Location loc = new Location();
            while (true)
            { 
                int row = rand.Next(5);
                int colum = rand.Next(5);
                if (playerBoard.squares[row, colum] != SquareStatus.Hit && playerBoard.squares[row, colum] != SquareStatus.Guessed)
                {

                    loc.Row = row;
                    loc.Column = colum;
                    if (playerBoard.squares[row, colum] == SquareStatus.Occupied)
                        playerBoard.squares[row, colum] = SquareStatus.Hit;
                    else
                        playerBoard.squares[row, colum] = SquareStatus.Guessed;
                    playerBoard.SinkShips();
                    LastComputerGuess = loc;
                    return loc;
                }
            }
        }

        public bool IsGameOver()
        {
            if (!computerBoard.Ships.Exists(ship => ship.IsSunk == false))
            { gameIsOver = true; Console.WriteLine("Game Over"); winner = "player";  return true; }
            if (!playerBoard.Ships.Exists(ship => ship.IsSunk == false))
            { gameIsOver = true; Console.WriteLine("GameOver");  winner = "computer"; return true; }
            return false;
        }
    }

    public class Location
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return ("(" + Convert.ToString(Row) + ", " + Convert.ToString(Column) + ")");   
        }
    }

    public class Board
    {
        public SquareStatus[,] squares = new SquareStatus[5, 5];
        public List<Ship> Ships { get; set; }

        public static Board MakeRandomBoard()
        {
            while (true)
            {
                Board brd = new Board();
                brd.Ships = new List<Ship>();
                brd.Ships.Add(new Ship() { Length = 4 });
                brd.Ships.Add(new Ship() { Length = 2 });
                brd.Ships.Add(new Ship() { Length = 1 });
                foreach (Ship ship in brd.Ships)
                {
                    ship.PlaceShipRandomly(brd);
                    ship.AdjacentLocations = ship.MakeAdjacentLocations();
                }
                if (BoardIsValid(brd))
                {
                    return brd;
                }
            }
        }

        static private bool BoardIsValid(Board brd)
        {
            foreach(Ship ship in brd.Ships)
            { 
                foreach (Location loc in ship.AdjacentLocations)
                {
                    if (brd.GetSquareStatus(loc) == SquareStatus.Occupied)
                    {
                        return false;
                    }
                }
            }
            
            // check for ships on top of eachother.
            int numberOfShipSpaces = 0;
            foreach (SquareStatus square in brd.squares)
                if (square == SquareStatus.Occupied) { numberOfShipSpaces++; }
            int totalLengthOfships = 0;
            foreach (Ship ship in brd.Ships)
                totalLengthOfships += ship.Length;
            if (totalLengthOfships != numberOfShipSpaces)
                return false;

            return true; // because if the code gets here, the board is valid.
                         // (Assuming that there are no ships partially or totally
                         // off the board.)
        }

        public SquareStatus GetSquareStatus(Location loc)
        {
            return squares[loc.Row, loc.Column];
        }

        public void SinkShips()
        {
            foreach (Ship ship in Ships)
            {
                foreach (Location loc in ship.Coordinates)
                {
                    if (GetSquareStatus(loc) != SquareStatus.Hit)
                        return;
                }
                ship.IsSunk = true; // the code won't get here is one of the squares is not occupied.
            }
        }

        public void PrintBoard()
        {
            foreach (SquareStatus square in squares)
                Console.WriteLine(square);
        }
    }

    public class Ship
    {
        static Random rand = new Random();
        public Direction direction;
        public bool IsSunk { get; set; }
        public Location[] Coordinates { get; set; }
        public int Length { get; set; }
        public List<Location> AdjacentLocations { get; set; }

        public Ship()
        {
            IsSunk = false;
            AdjacentLocations = MakeAdjacentLocations();
        }
        public void PlaceShip(Direction dir, Location startingCoordinate, Board sea)
        {
            direction = dir;
            Coordinates = new Location[Length];

            // first we make the coordinates for the ship.
            if (dir == Direction.Horizontal)
            {

                for (int i = 0; i < Length; i++)
                {
                    Coordinates[i] = new Location() { Row = startingCoordinate.Row, Column = startingCoordinate.Column + i };
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    Coordinates[i] = new Location() { Row = startingCoordinate.Row + i, Column = startingCoordinate.Column };
                }
            }

            // now that we made the ships coordinates, we need to update that on the board.

            foreach (Location loc in Coordinates)
                sea.squares[loc.Row, loc.Column] = SquareStatus.Occupied;

            // Here I will place the ship in the given <sea> with starting coordinate indicated
            // and in the direction indicated. The logic of making sure that the ship was not placed
            // adjacent to another ship will be left up to the BoardIsValid() method. 
        }

        public void PlaceShipRandomly(Board sea)
        {
            int positionLengthwise = rand.Next(6 - Length); // because smaller ships can go further running into an edge.
            int positionSidewise = rand.Next(5);

            int dirNumber = rand.Next(2);
            Direction dir;
            if (dirNumber == 0)
            {
                dir = Direction.Horizontal;
                Location startingCoordinate = new Location() { Row = positionSidewise, Column = positionLengthwise };
                PlaceShip(dir, startingCoordinate, sea);
            }
            else
            {
                dir = Direction.Vertical;
                Location startingCoordinate = new Location() { Row = positionLengthwise, Column = positionSidewise };
                PlaceShip(dir, startingCoordinate, sea);
            }
        }

        public List<Location> MakeAdjacentLocations()
        {
            List<Location> adjacents = new List<Location>();
            if (direction == Direction.Horizontal)
            {
                for (int i = 0; i < Coordinates.Length; i++)
                {
                    if (i == 0)
                    {
                        if (Coordinates[0].Row == 0)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[0].Row + 1, Column = Coordinates[0].Column });
                        }
                        else if (Coordinates[0].Row == 4)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[0].Row - 1, Column = Coordinates[0].Column });
                        }
                        else
                        {
                            adjacents.Add(new Location() { Row = Coordinates[0].Row - 1, Column = Coordinates[0].Column });
                            adjacents.Add(new Location() { Row = Coordinates[0].Row + 1, Column = Coordinates[0].Column });
                        }

                        if (Coordinates[0].Column != 0)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[0].Row, Column = Coordinates[0].Column - 1 });
                        }
                    }
                    else if (i == Length - 1)
                    {
                        if (Coordinates[i].Row == 0)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row + 1, Column = Coordinates[i].Column });
                        }
                        else if (Coordinates[i].Row == 4)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row - 1, Column = Coordinates[i].Column });
                        }
                        else
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row - 1, Column = Coordinates[i].Column });
                            adjacents.Add(new Location() { Row = Coordinates[i].Row + 1, Column = Coordinates[i].Column });
                        }

                        if (Coordinates[i].Column != 4)
                            adjacents.Add(new Location() { Column = Coordinates[i].Column + 1, Row = Coordinates[i].Row });

                    }
                    else
                    {
                        if (Coordinates[i].Row == 0)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row + 1, Column = Coordinates[i].Column });
                        }
                        else if (Coordinates[i].Row == 4)
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row - 1, Column = Coordinates[i].Column });
                        }
                        else
                        {
                            adjacents.Add(new Location() { Row = Coordinates[i].Row + 1, Column = Coordinates[i].Column });
                            adjacents.Add(new Location() { Row = Coordinates[i].Row - 1, Column = Coordinates[i].Column });
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    if (i == 0)
                    {
                        if (Coordinates[i].Column == 0)
                        {
                            adjacents.Add(new Location() { Column = Coordinates[0].Column + 1, Row = Coordinates[0].Row });
                        }
                        else if (Coordinates[i].Column == 4)
                            adjacents.Add(new Location() { Column = Coordinates[0].Column - 1, Row = Coordinates[0].Row });
                        else
                        {
                            adjacents.Add(new Location() { Column = Coordinates[0].Column + 1, Row = Coordinates[0].Row });
                            adjacents.Add(new Location() { Column = Coordinates[0].Column - 1, Row = Coordinates[0].Row });
                        }
                        if (Coordinates[i].Row != 0)
                            adjacents.Add(new Location() { Column = Coordinates[0].Column, Row = Coordinates[0].Row - 1 });
                    }
                    else if (i == Length - 1)
                    {
                        if (Coordinates[i].Column == 0)
                        {
                            adjacents.Add(new Location() { Column = Coordinates[i].Column + 1, Row = Coordinates[i].Row });
                        }
                        else if (Coordinates[i].Column == 4)
                        {
                            adjacents.Add(new Location() { Column = Coordinates[i].Column - 1, Row = Coordinates[i].Row });
                        }
                        else
                        {
                            adjacents.Add(new Location() { Column = Coordinates[i].Column - 1, Row = Coordinates[i].Row });
                            adjacents.Add(new Location() { Column = Coordinates[i].Column + 1, Row = Coordinates[i].Row });
                        }
                        if (Coordinates[i].Row != 4)
                            adjacents.Add(new Location() { Column = Coordinates[i].Column, Row = Coordinates[i].Row + 1 });
                    }
                    else
                    {
                        if (Coordinates[i].Column == 0)
                            adjacents.Add(new Location() { Column = Coordinates[i].Column + 1, Row = Coordinates[i].Row });
                        else if (Coordinates[i].Column == 4)
                            adjacents.Add(new Location() { Column = Coordinates[i].Column - 1, Row = Coordinates[i].Row });
                        else
                        {
                            adjacents.Add(new Location() { Column = Coordinates[i].Column - 1, Row = Coordinates[i].Row });
                            adjacents.Add(new Location() { Column = Coordinates[i].Column + 1, Row = Coordinates[i].Row });
                        }
                    }
                }
            }
            return adjacents;
        }
    }
}


