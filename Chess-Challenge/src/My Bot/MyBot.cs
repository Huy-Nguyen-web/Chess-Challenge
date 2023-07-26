using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot {
    // Bot version 0.3

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer t) {
        // If not endgame then search for 5 moves ahead.
        int countDepth;
        if (CountMaterial(true, board) <= 1000 || CountMaterial(false, board) <= 1000) {
            if (CountMaterial(true, board) <= 600 || CountMaterial(false, board) <= 600) {
                countDepth = 6;
            } else {
                countDepth = 5;
            }
        } else {
            countDepth = 4;
        }
        Move moveToPlay = Search(countDepth, int.MinValue, int.MaxValue, board).Item2;
        return moveToPlay;
    }

    int CountMaterial(bool isWhite, Board board) {
        int material = 0;

        // TODO: Optimize this later
        material += board.GetPieceList(PieceType.Pawn, isWhite).Count * 100;
        material += board.GetPieceList(PieceType.Knight, isWhite).Count * 300;
        material += board.GetPieceList(PieceType.Bishop, isWhite).Count * 300;
        material += board.GetPieceList(PieceType.Rook, isWhite).Count * 500;
        material += board.GetPieceList(PieceType.Queen, isWhite).Count * 900;

        return material;
    }

    int CountMobility(bool isWhite, Board board) {
        int moveCount ;
        if((board.IsWhiteToMove && isWhite) || (!board.IsWhiteToMove && !isWhite)) {
            moveCount = board.GetLegalMoves().Length;
        } else {
            if (board.TrySkipTurn()) {
                moveCount = board.GetLegalMoves().Length;
                board.UndoSkipTurn();
            } else {
                moveCount = 0;
            }
        }
        return moveCount;
    }

    int Evaluate(Board board) {
        int whiteEval = CountMaterial(true, board);
        int blackEval = CountMaterial(false, board);
        
        int whiteMobility = CountMobility(true, board);
        int blackMobility = CountMobility(false, board);

        int pieceEval = whiteEval - blackEval;
        int mobilityEval = whiteMobility - blackMobility;
        

        int finalEval = pieceEval + mobilityEval * 10;

        return finalEval ;
    }


    // Add order move by MVVLVA value
    int GetMVVLVAValue(Move move) {
        if (move.IsCapture) {
            return (pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType]);
        }
        return -pieceValues[(int) move.MovePieceType];
    }

    Move[] OrderMove(Move[] moves, Board board) {
        int mobility = CountMobility(board.IsWhiteToMove, board);
        Array.Sort(moves, (move1, move2) => {
            int mvvLvaValue1 = GetMVVLVAValue(move1) + mobility;
            int mvvLvaValue2 = GetMVVLVAValue(move2) + mobility;
            return mvvLvaValue2.CompareTo(mvvLvaValue1);
        });
        return moves;
    }

    Tuple<int, Move> Search(int depth, int alpha, int beta, Board board) {
        if (depth == 0) {
            return Tuple.Create(Evaluate(board), Move.NullMove); 
        }

        Move[] moves = board.GetLegalMoves();
        Move[] orderMoves = OrderMove(moves, board);
        if(!board.IsWhiteToMove) Array.Reverse(orderMoves);

        Move bestMove;
        if (moves.Length == 0) {
            if (board.IsInCheck()) {
                if(board.IsWhiteToMove)
                    return Tuple.Create(int.MinValue, Move.NullMove);
                else {
                    return Tuple.Create(int.MaxValue, Move.NullMove);
                }
            }
            return Tuple.Create(0, Move.NullMove);
        }
        
        bestMove = orderMoves[0];
        if (board.IsWhiteToMove) {
            int maxEval = int.MinValue;
            foreach (Move move in orderMoves) {
                board.MakeMove(move);
                int currentEval;
                if (board.IsInCheck()) {
                    if (!board.IsDraw()) {
                        currentEval = Search(depth, alpha, beta, board).Item1;
                    } else {
                        currentEval = 0;
                    }
                } else {
                    currentEval = Search(depth - 1, alpha, beta, board).Item1;
                }
                board.UndoMove(move);

                if (currentEval > maxEval) {
                    maxEval = currentEval;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, currentEval);
                if (beta <= alpha) break;
            }
            return Tuple.Create(maxEval, bestMove);
        } else {
            int minEval = int.MaxValue;
            foreach (Move move in orderMoves) {
                board.MakeMove(move);
                int currentEval;
                if (board.IsInCheck()) {
                    if (!board.IsDraw()) {
                        currentEval = Search(depth, alpha, beta, board).Item1;
                    } else {
                        currentEval = 0;
                    }
                } else {
                    currentEval = Search(depth - 1, alpha, beta, board).Item1;
                }
                board.UndoMove(move);

                if (currentEval < minEval) {
                    minEval = currentEval;
                    bestMove = move;
                }
                beta = Math.Min(beta, currentEval);
                if (beta <= alpha) break;
            }
            return Tuple.Create(minEval, bestMove);
        }
    }
}
