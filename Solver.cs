using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LastPiecePuzzle
{
    public class Solver
    {

        public List<PositionAnalysis> AnalysesDepth = new List<PositionAnalysis>();


        public Action<Board> OnAnalysing;

        public Action<Board> OnDeadEnd;

        public Action<Board> OnSolved;

        public Action OnFailed;

        public void Solve()
        {

            var rootAnalysis = new PositionAnalysis
            {
                Board = Real.Board.Clone(),
            };

            AnalysesDepth.Add(rootAnalysis);
            
            // We can cheat by placing the first piece.
            var cheatPiece = Real.Pieces.Last();
            rootAnalysis.Board.PlacePiece(cheatPiece, 6, 6, 0);
            rootAnalysis.FocusX = 7;
            rootAnalysis.FocusY = 6;

            AnalyseNextPlacement(rootAnalysis);

            if (IsSolved)
            {
                //OnSolved(AnalysesDepth.Last().Board);
            }
            else
            {
                //OnFailed();
            }
        }

        public bool IsSolved;

        public void AnalyseNextPlacement(PositionAnalysis analysis)
        {
            OnAnalysing(analysis.Board);

            analysis.NextPossiblePlacements = FindPossibleNextPlacements(analysis);
            if (!analysis.NextPossiblePlacements.Any())
            {
                OnDeadEnd(analysis.Board);
                return; // Dead end
            }
         
            foreach (var possiblePlacement in analysis.NextPossiblePlacements)
            {
                var nextAnalysis = new PositionAnalysis()
                {
                    Board = analysis.Board.Clone(),
                };
                AnalysesDepth.Add(nextAnalysis);
                {
                    nextAnalysis.Board.PlacePiece(possiblePlacement.Piece, possiblePlacement.AtX, possiblePlacement.AtY, possiblePlacement.Orientation);

                    if (!nextAnalysis.Board.UnplacedPieces.Any())
                    {
                        //IsSolved = true;
                        OnSolved(nextAnalysis.Board);
                        return;
                    }

                    FindNextFocusPoint(nextAnalysis);

                    AnalyseNextPlacement(nextAnalysis);

                    //if (IsSolved) return;
                }
                AnalysesDepth.Remove(nextAnalysis);
            }
        }

        public List<PossiblePlacement> FindPossibleNextPlacements(PositionAnalysis positionAnalysis)
        {
            var list = new List<PossiblePlacement>();

            foreach (var unplacedPiece in positionAnalysis.Board.UnplacedPieces)
            for (var orientationIndex = 0; orientationIndex < 4; orientationIndex++)
            {
                var orientation = unplacedPiece.Orientations[orientationIndex];
                for (var atBoardX = positionAnalysis.FocusX - (orientation.Width - 1); atBoardX <= positionAnalysis.FocusX; atBoardX++)
                for (var atBoardY = positionAnalysis.FocusY - (orientation.Height - 1); atBoardY <= positionAnalysis.FocusY; atBoardY++)
                {
                    // Check each cell on the piece to make sure it is...
                    for (var pieceX = 0; pieceX < orientation.Width; pieceX++)
                    for (var pieceY = 0; pieceY < orientation.Height; pieceY++)
                    {
                        //... still on the board in X direction
                        var checkBoardX = atBoardX + pieceX;
                        if (checkBoardX < 0 || checkBoardX >= positionAnalysis.Board.Width) 
                            goto SkipThisLocation;

                        //... still on the board in Y direction
                        var checkBoardY = atBoardY + pieceY;
                        if (checkBoardY < 0 || checkBoardY >= positionAnalysis.Board.Height)
                            goto SkipThisLocation;

                        //...  and is the correcct outy/inny
                        if (orientation.IsOuty[pieceY, pieceX] != positionAnalysis.Board.IsOuty[checkBoardY, checkBoardX])
                            goto SkipThisLocation;

                        //... aaaaannd..... isn't overlapping an existing piece.
                        if (orientation.IsFilledIn[pieceY, pieceX] && positionAnalysis.Board.CellPieces[checkBoardY, checkBoardX] != null)
                            goto SkipThisLocation;

                        //... aaand is definitely covering the focus point
                        if (!orientation.IsFilledIn[positionAnalysis.FocusY - atBoardY, positionAnalysis.FocusX - atBoardX])
                            goto SkipThisLocation;
                    }
                     
                    // No objections to placing unplacedPiece on teh board at AtX-AtY with specified orientationIndex.
                    list.Add(new PossiblePlacement
                    {
                        AtX = atBoardX,
                        AtY = atBoardY,
                        Orientation = orientationIndex,
                        Piece = unplacedPiece,
                    });

                SkipThisLocation:
                    continue;
                }
                
            SkipThisOrientation:
                continue;
            }

            return list;
        }

        public List<Point> FocusPoints = GetSpiralPointList();

        public static List<Point> GetSpiralPointList()
        {

            var list = new List<Point>();
            var directions = new List<Point>
            {
                new Point(0, -1),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(1, 0),
            };

            var point = new Point(7, 8);

            var vectorLength = 8;

            var vectorCount = 0;

            for (var ring = 0; ring < 4; ring++)
            {
                foreach (var direction in directions)
                {
                    for (var vectorIndex = 0; vectorIndex < vectorLength; vectorIndex++)
                    {
                        point = new Point(point.X + direction.X, point.Y + direction.Y);
                        list.Add(point);
                    }
                    vectorCount++;

                    if (vectorCount % 2 == 1) vectorLength--; // Reduce vector length every second vector for make benefit great spiral
                }
            }

            return list;
        }

        public void FindNextFocusPoint(PositionAnalysis positionAnalysis)
        {
            var nextPoint = FocusPoints.First(point => positionAnalysis.Board.CellPieces[point.Y, point.X] == null);
            positionAnalysis.FocusX = nextPoint.X;
            positionAnalysis.FocusY = nextPoint.Y;
        }

        public class PositionAnalysis
        {
            public Board Board;

            public int FocusX;

            public int FocusY;

            public List<PossiblePlacement> NextPossiblePlacements;
        }

        public class PossiblePlacement
        {
            public Piece Piece;

            public int Orientation;

            public int AtX;

            public int AtY;
        }
    }

    public static class Real
    {
        public static List<Piece> Pieces = new List<Piece>
        {
            new Piece(new[,]{
                    {true,  true},
                    {false, true},
                    {false, true},
                    {false, true}},
                    false, "0"),
            new Piece(new[,]{
                    {true, true},
                    {true, false},
                    {true, false},
                    {true, false}},
                    false, "1"),
            new Piece(new[,]{
                    {true, false},
                    {true, true},
                    {true, false},
                    {true, false}},
                    true, "2"),
            new Piece(new[,]{
                    {true, true},
                    {false, true},
                    {false, true}},
                    false,"3"),
            new Piece(new[,]{
                    {false, true},
                    {true, true},
                    {true, false},
                    {true, false}},
                    false, "4"),
            new Piece(new[,]{
                    {false, true},
                    {true, true},
                    {false, true},
                    {false, true}},
                    true, "5"),
            new Piece(new[,]{
                    {true, true, false},
                    {false, true, false},
                    {false, true, true}},
                    false, "6"),
            new Piece(new[,]{
                    {true, true, false},
                    {false, true, true},
                    {false, false, true}},
                    true, "7"),
            new Piece(new[,]{
                    {true, true},
                    {true, false},
                    {true, false}},
                    false, "8"),
            new Piece(new[,]{
                    {true, false},
                    {true, true},
                    {true, false}},
                    false,"9"),
            new Piece(new[,]{
                    {true, false},
                    {true, true},
                    {false, true},
                    {false, true}},
                    false, "A"),
            new Piece(new[,]{
                    {true, false},
                    {true, true},
                    {false, true}},
                    false, "B"),
            new Piece(new[,]{
                    {false, true, true},
                    {false, true, false},
                    {true, true, false}},
                    true, "C"),
            new Piece(new[,]{
                    {true, false},
                    {true, true}},
                    true, "D"),
        };

        public static Board Board = new Board(8, 8)
        {
            IsOuty = new[,]
            {
                {true, false, true, false, true, false, true, false},
                {false, true, false, true, false, true, false, true},
                {true, false, true, false, true, false, true, false},
                {false, true, false, true, false, true, false, true},
                {true, false, true, false, true, false, true, false},
                {false, true, false, true, false, true, false, true},
                {true, false, true, false, true, false, true, false},
                {false, true, false, true, false, true, false, true}
            },
            UnplacedPieces = Pieces
        };
        
    }

    public class Board
    {
        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            CellPieces = new Piece[Width, Height];

            IsOuty = new bool[Width,Height];
            
            Placements = new List<Placement>();
            UnplacedPieces = new List<Piece>();
        }
        public int Width;

        public int Height;

        public Piece[,] CellPieces;

        public bool[,] IsOuty;

        public List<Placement> Placements;

        public List<Piece> UnplacedPieces; 

        public void PlacePiece(Piece piece, int atX, int atY, int orientation)
        {
            var placement = new Placement
            {
                Orientation = orientation,
                X = atX,
                Y = atY,
                Piece = piece,
            };
            Placements.Add(placement);

            var orient = piece.Orientations[orientation];
            for (var pieceY = 0; pieceY < orient.Height; pieceY++)
            for (var pieceX = 0; pieceX < orient.Width; pieceX++)
            {
                if (!orient.IsFilledIn[pieceY, pieceX]) continue;

                var boardY = pieceY + atY;
                var boardX = pieceX + atX;

                CellPieces[boardY, boardX] = piece;
            }

            UnplacedPieces.Remove(piece);
        }

        public override string ToString()
        {
            
            var sb = new StringBuilder();
            for (var y = 0; y < Height; y++)
            {
                sb.Append("#");
                for (var x = 0; x < Width; x++)
                {
                    sb.Append(CellPieces[y, x] != null ? CellPieces[y, x].Code + " " : "  ");
                }
                sb.AppendLine("#");
            }
            return sb.ToString();
        }

        public Board Clone()
        {
            return new Board(Width, Height)
            {
                IsOuty = IsOuty,
                Placements = Placements.ToList(),
                CellPieces = (Piece[,])CellPieces.Clone(),
                UnplacedPieces = UnplacedPieces.ToList(),

            };
        }
    }

    public class Piece
    {
        public Piece(bool[,] isFilledIn, bool isTopLeftOuty, string code)
        {
            var orientation1 = new PieceOrientation
            {
                Width = isFilledIn.GetUpperBound(1) + 1,
                Height = isFilledIn.GetUpperBound(0) + 1,
                IsFilledIn = isFilledIn,
            };

            orientation1.IsOuty = new bool[orientation1.Height, orientation1.Width];
            for(var y = 0; y < orientation1.Height; y++)
                for (var x = 0; x < orientation1.Width; x++)
                    orientation1.IsOuty[y, x] = ((y + x)%2 == 0) ? isTopLeftOuty : !isTopLeftOuty;

            var orientation2 = orientation1.RotateClockwise90Degrees();
            var orientation3 = orientation2.RotateClockwise90Degrees();
            var orientation4 = orientation3.RotateClockwise90Degrees();
            Orientations = new List<PieceOrientation>{
                orientation1,
                orientation2,
                orientation3,
                orientation4};

            Code = code;
        }

        public string Code;

        public List<PieceOrientation> Orientations;

        
    }

    public class PieceOrientation
    {

        public int Width;

        public int Height;

        public bool[,] IsFilledIn;

        public bool[,] IsOuty;

        public PieceOrientation RotateClockwise90Degrees()
        {
            var orientation = new PieceOrientation
            {
                Width = Height,
                Height = Width,
                IsFilledIn = new bool[Width, Height],
                IsOuty = new bool[Width, Height],
            };

            for (var y = 0; y < orientation.Height; y++)
            for (var x = 0; x < orientation.Width; x++)
            {
                var oldY = (orientation.Width - 1) - x;
                var oldX = y;

                orientation.IsFilledIn[y, x] = IsFilledIn[oldY, oldX];
                orientation.IsOuty[y, x] = IsOuty[oldY, oldX];
            }

            return orientation;
        }

    }

    public class Placement
    {
        public Piece Piece;

        public int X;

        public int Y;

        public int Orientation; // 0, 1, 2, 3, rotatins clockwise
    }

}
