using System;
using System.Diagnostics;
using System.IO;
using ChessChallenge.API;

public class Stockfish : IChessBot {
    private Process stockfishProcess;
    private StreamWriter Ins() => stockfishProcess.StandardInput;
    private StreamReader Outs() => stockfishProcess.StandardOutput;

    /// <summary>
    /// The skill level of stockfish. Max is 20, min is 0.
    /// </summary>
    private const int SKILL_LEVEL = 3;
    
    public Stockfish() {
        var stockfishExe = Environment.GetEnvironmentVariable("STOCKFISH_EXE", EnvironmentVariableTarget.User);
        if (stockfishExe == null) {
            throw new Exception("Missing environment variable: 'STOCKFISH_EXE'");
        }

        stockfishProcess = new();
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.FileName = stockfishExe;
        stockfishProcess.Start();

        Ins().WriteLine("uci");
        string? line;
        var isOk = false;

        while ((line = Outs().ReadLine()) != null) {
            if (line == "uciok") {
                isOk = true;
                break;
            }
        }

        if (!isOk) {
            throw new Exception("Failed to communicate with stockfish");
        }

        Ins().WriteLine($"setoption name Skill Level value {SKILL_LEVEL}");
    }

    public Move Think(Board board, Timer timer) {
        Ins().WriteLine("ucinewgame");
        Ins().WriteLine($"position fen {board.GetFenString()}");
        var timeString = board.IsWhiteToMove ? "wtime" : "btime";
        Ins().WriteLine($"go {timeString} {timer.MillisecondsRemaining}");

        string? line;
        Move? move = null;

        while ((line = Outs().ReadLine()) != null) {
            if (line.StartsWith("bestmove")) {
                var moveStr = line.Split()[1];
                move = new Move(moveStr, board);
                
                break;
            }
        }

        if (move == null) {
            throw new Exception("Engine crashed");
        }

        return (Move)move;
    }
}