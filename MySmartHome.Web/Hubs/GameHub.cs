using Microsoft.AspNetCore.SignalR;
using MySmartHome.Web.Services;

namespace MySmartHome.Web.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;

    public GameHub(GameService gameService)
    {
        _gameService = gameService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _gameService.RemovePlayer(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinQueue()
    {
        // Ensure any previous game state is cleared
        await _gameService.RemovePlayer(Context.ConnectionId);

        bool matched = await _gameService.AddToQueueAsync(Context.ConnectionId);
        if (!matched)
        {
            await Clients.Caller.SendAsync("WaitingForOpponent");
        }
    }

    public async Task MakeMove(int index)
    {
        var room = _gameService.GetRoomByPlayerId(Context.ConnectionId);
        if (room == null || room.CurrentTurn != Context.ConnectionId)
        {
            return;
        }

        if (room.Board[index] != null)
        {
            return;
        }

        // Apply move
        var symbol = room.Player1 == Context.ConnectionId ? "X" : "O";
        room.Board[index] = symbol;

        // Check win/draw
        var winner = CheckWin(room.Board);
        var isDraw = winner == null && room.Board.All(x => x != null);

        // Switch turn
        room.CurrentTurn = room.Player1 == Context.ConnectionId ? room.Player2 : room.Player1;

        // Broadcast update
        await Clients.Client(room.Player1).SendAsync("UpdateGame", room.Board, room.CurrentTurn == room.Player1, winner, isDraw);
        await Clients.Client(room.Player2).SendAsync("UpdateGame", room.Board, room.CurrentTurn == room.Player2, winner, isDraw);
    }



    private string? CheckWin(string[] board)
    {
        int[][] combinations = new int[][]
        {
            new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Rows
            new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Cols
            new[] {0, 4, 8}, new[] {2, 4, 6}                    // Diagonals
        };

        foreach (var combo in combinations)
        {
            if (board[combo[0]] != null &&
                board[combo[0]] == board[combo[1]] &&
                board[combo[1]] == board[combo[2]])
            {
                return board[combo[0]];
            }
        }
        return null;
    }
    
    // Dispose subscription to avoid memory leaks if Hub is transient (Hubs are usually transient)
    // However, GameService is Singleton. We need to be careful about event subscription.
    // Actually, Hub instances are created per request/connection. Subscribing to a Singleton event from a Transient object is a memory leak risk.
    // BETTER APPROACH: The GameService should call the HubContext directly or we use a different mechanism.
    // For this simple example, we can inject IHubContext<GameHub> into GameService.
}
