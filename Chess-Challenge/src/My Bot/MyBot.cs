using ChessChallenge.API;
using System;

public class MyBot : IChessBot {
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer t) {
        Move[] moves = board.GetLegalMoves();
        
        Console.WriteLine(Search(5,  int.MinValue, int.MaxValue, board).Item1);

        Move moveToPlay = Search(5, int.MinValue, int.MaxValue, board).Item2;
        Console.WriteLine(moveToPlay);
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

    int Evaluate(Board board) {
        int whiteEval = CountMaterial(true, board);
        int blackEval = CountMaterial(false, board);

        // if positive then it's advantage for white, else is advantage for black
        int pieceEval = whiteEval - blackEval;
        //// if black is to move, then it should be black advantage, then it's should be positive if the player is black
        int perspective = (board.IsWhiteToMove) ? 1 : -1;
        int checkValue = board.IsInCheck()? 1000 : 0;


        int finalEval = pieceEval;

        return finalEval;
    }


    Tuple<int, Move> Search(int depth, int alpha, int beta, Board board) {
        if (depth == 0) {
            return Tuple.Create(Evaluate(board), Move.NullMove);
        }

        Move[] moves = board.GetLegalMoves();
        Move bestMove = Move.NullMove;
        if (moves.Length == 0) {
            if (board.IsInCheck()) {
                return Tuple.Create(int.MinValue, bestMove);
            }
            return Tuple.Create(0, bestMove);
        }
        if (board.IsWhiteToMove) {
            int maxEval = int.MinValue;
            foreach (Move move in moves) {
                board.MakeMove(move);
                int currentEval = Search(depth - 1, alpha, beta, board).Item1;
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
            foreach (Move move in moves) {
                board.MakeMove(move);
                int currentEval = Search(depth - 1, alpha, beta, board).Item1;
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


        //Move searchBestMove = Move.NullMove;
        //Move currentBestMove = Move.NullMove;

        //int bestEval = int.MinValue;
        //if (depth == 0) {
        //    return Tuple.Create(Evaluate(board), currentBestMove);
        //}


        //    foreach (Move move in moves) {
        //        board.MakeMove(move);
        //        int eval = -Search(depth - 1, -beta, -alpha, board).Item1;
        //        if (eval > bestEval) {
        //            bestEval = eval;
        //            currentBestMove = move;
        //        }
        //        board.UndoMove(move);
        //    }

        //    if (depth == 4) {
        //        searchBestMove = currentBestMove;
        //    }
        //}
    }
}
