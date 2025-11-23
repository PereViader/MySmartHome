using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using MySmartHome.Web.Hubs;

namespace MySmartHome.Web.Services;

public class GameService
{
    private readonly IHubContext<GameHub> _hubContext;
    
    // Lock object for thread safety
    private readonly object _lock = new();

    // Queue of connection IDs waiting for a game
    private readonly List<string> _waitingQueue = new();
    
    // Active rooms mapped by Room ID
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    
    // Map connection ID to Room ID for quick lookup
    private readonly ConcurrentDictionary<string, string> _playerRoomMap = new();

    public GameService(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<bool> AddToQueueAsync(string connectionId)
    {
        string? player1 = null;
        string? player2 = null;
        bool matched = false;

        lock (_lock)
        {
            if (_waitingQueue.Contains(connectionId) || _playerRoomMap.ContainsKey(connectionId))
            {
                return false;
            }

            _waitingQueue.Add(connectionId);
            
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
            await NotifyGameStarted(player1, player2);
            return true;
        }

        return false;
    }

    private async Task NotifyGameStarted(string player1, string player2)
    {
        await _hubContext.Clients.Client(player1).SendAsync("GameStarted", "X", true); // symbol, isMyTurn
        await _hubContext.Clients.Client(player2).SendAsync("GameStarted", "O", false);
    }

    public GameRoom? GetRoomByPlayerId(string connectionId)
    {
        if (_playerRoomMap.TryGetValue(connectionId, out var roomId))
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                return room;
            }
        }
        return null;
    }

    public async Task RemovePlayer(string connectionId)
    {
        lock (_lock)
        {
            if (_waitingQueue.Contains(connectionId))
            {
                _waitingQueue.Remove(connectionId);
                return;
            }
        }

        if (_playerRoomMap.TryRemove(connectionId, out var roomId))
        {
            if (_rooms.TryRemove(roomId, out var room))
            {
                // Notify the other player
                var otherPlayer = room.Player1 == connectionId ? room.Player2 : room.Player1;
                _playerRoomMap.TryRemove(otherPlayer, out _);
                
                await _hubContext.Clients.Client(otherPlayer).SendAsync("OpponentLeft");
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
    public string CurrentTurn { get; set; } // ConnectionId of current player

    public GameRoom(string roomId, string player1, string player2)
    {
        RoomId = roomId;
        Player1 = player1;
        Player2 = player2;
        CurrentTurn = player1; // Player 1 starts
    }
}
