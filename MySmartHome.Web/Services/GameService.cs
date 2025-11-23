using System.Collections.Concurrent;

namespace MySmartHome.Web.Services;

public class GameService
{
    // Events for game lifecycle
    public event Action<string>? OnWaiting;
    public event Action<string, string, bool>? OnGameStarted; // playerId, symbol, isTurn
    public event Action<string, string?[], bool, string?, bool>? OnGameUpdated; // playerId, board, isTurn, winner, isDraw
    public event Action<string>? OnOpponentLeft;

    // Lock object for thread safety
    private readonly object _lock = new();

    // Queue of player IDs waiting for a game
    private readonly List<string> _waitingQueue = new();
    
    // Active rooms mapped by Room ID
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    
    // Map player ID to Room ID for quick lookup
    private readonly ConcurrentDictionary<string, string> _playerRoomMap = new();

    public void JoinQueue(string playerId)
    {
        // Ensure any previous game state is cleared
        RemovePlayer(playerId);

        string? player1 = null;
        string? player2 = null;
        bool matched = false;

        lock (_lock)
        {
            if (_waitingQueue.Contains(playerId) || _playerRoomMap.ContainsKey(playerId))
            {
                return;
            }

            _waitingQueue.Add(playerId);
            
            if (_waitingQueue.Count >= 2)
            {
                player1 = _waitingQueue[0];
                player2 = _waitingQueue[1];
                
                _waitingQueue.RemoveAt(0);
                _waitingQueue.RemoveAt(0);

                var roomId = Guid.NewGuid().ToString();
                var room = new GameRoom(roomId, player1, player2);
                
                _rooms.TryAdd(roomId, room);
                _playerRoomMap.TryAdd(player1, roomId);
                _playerRoomMap.TryAdd(player2, roomId);
                
                matched = true;
            }
        }

        if (matched && player1 != null && player2 != null)
        {
            NotifyGameStarted(player1, player2);
        }
        else
        {
            OnWaiting?.Invoke(playerId);
        }
    }

    private void NotifyGameStarted(string player1, string player2)
    {
        OnGameStarted?.Invoke(player1, "X", true);
        OnGameStarted?.Invoke(player2, "O", false);
    }

    public void MakeMove(string playerId, int index)
    {
        if (!_playerRoomMap.TryGetValue(playerId, out var roomId) || !_rooms.TryGetValue(roomId, out var room))
        {
            return;
        }

        if (room.CurrentTurn != playerId)
        {
            return;
        }

        if (room.Board[index] != null)
        {
            return;
        }

        // Apply move
        var symbol = room.Player1 == playerId ? "X" : "O";
        room.Board[index] = symbol;

        // Check win/draw
        var winner = CheckWin(room.Board);
        var isDraw = winner == null && room.Board.All(x => x != null);

        // Switch turn
        room.CurrentTurn = room.Player1 == playerId ? room.Player2 : room.Player1;

        // Broadcast update
        OnGameUpdated?.Invoke(room.Player1, room.Board, room.CurrentTurn == room.Player1, winner, isDraw);
        OnGameUpdated?.Invoke(room.Player2, room.Board, room.CurrentTurn == room.Player2, winner, isDraw);
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

    public void RemovePlayer(string playerId)
    {
        lock (_lock)
        {
            if (_waitingQueue.Contains(playerId))
            {
                _waitingQueue.Remove(playerId);
                return;
            }
        }

        if (_playerRoomMap.TryRemove(playerId, out var roomId))
        {
            if (_rooms.TryRemove(roomId, out var room))
            {
                // Notify the other player
                var otherPlayer = room.Player1 == playerId ? room.Player2 : room.Player1;
                _playerRoomMap.TryRemove(otherPlayer, out _);
                
                OnOpponentLeft?.Invoke(otherPlayer);
            }
        }
    }
}

public class GameRoom
{
    public string RoomId { get; }
    public string Player1 { get; } // X
    public string Player2 { get; } // O
    public string[] Board { get; } = new string[9];
    public string CurrentTurn { get; set; } // PlayerId of current player

    public GameRoom(string roomId, string player1, string player2)
    {
        RoomId = roomId;
        Player1 = player1;
        Player2 = player2;
        CurrentTurn = player1; // Player 1 starts
    }
}
