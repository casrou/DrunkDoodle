using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Hubs
{
    public class GameHub : Hub
    {
        // GAME
        private static List<string> _words = new List<string>() { "Cykel", "Abe", "Mikroovn", "Paraply", "Øl" };
        private static List<Room> _rooms = new List<Room>();
        private static Random _random = new Random();

        // ARTIST
        public async Task CreateRoom(List<Player> players)
        {
            Room temproom = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            if (temproom == null)
            {
                int roomId = _random.Next(100, 1000);
                Room room = new Room()
                {
                    roomId = roomId,
                    artist = Context.ConnectionId,
                    audience = new List<string>(),
                    drawPoints = new List<DrawPoint>(),
                    players = orderPlayers(players)
                };
                _rooms.Add(room);
                await Clients.Caller.SendAsync("RoomCreated", roomId);
            }
            else
            {
                await Clients.Caller.SendAsync("RoomCreated", temproom.roomId);
            }
        }

        private Queue<Player> orderPlayers(List<Player> players)
        {
            List<int> teams = players.Select(p => p.team).Distinct().ToList()
                .OrderBy(a => Guid.NewGuid()).ToList();
            Queue<Player> orderedPlayers = new Queue<Player>();
            while(players.Count > 0)
            {
                foreach (int team in teams)
                {
                    IEnumerable<Player> playersInTeam = players.Where(pt => pt.team == team);
                    if(playersInTeam.Count() > 0)
                    {
                        Player p = playersInTeam.ElementAt(_random.Next(playersInTeam.Count()));
                        orderedPlayers.Enqueue(p);
                        players.Remove(p);
                    }                
                }
            }
            return orderedPlayers;
        }

        public async Task StartRound()
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            string word = _words[_random.Next(_words.Count)];
            await Clients.Caller.SendAsync("NewRound", word);
            await Clients.Clients(room.audience).SendAsync("NewRound", word);
        }

        public async Task EndRound()
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            if (room == null) return;
            Player nextDrawer = room.players.Dequeue();
            room.players.Enqueue(nextDrawer);
            await Clients.Caller.SendAsync("PrepareRound", nextDrawer.name);
            await Clients.Clients(room.audience).SendAsync("EndRound");
        }

        public async Task ClearCanvas()
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            if (room == null) return;
            room.drawPoints.Clear();
            await Clients.Caller.SendAsync("ClearCanvas");
            await Clients.Clients(room.audience).SendAsync("ClearCanvas");
        }

        public async Task IsDrawing(double x, double y, bool dragging)
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            if(room != null)
            {
                room.drawPoints.Add(new DrawPoint() { x = x, y = y, dragging = dragging });
                await Clients.Clients(room.audience).SendAsync("UpdateDrawing", x, y, dragging);
            }
        }

        // AUDIENCE
        public async Task JoinAudience(int roomId)
        {
            Room room = _rooms.FirstOrDefault(r => r.roomId == roomId);
            if (room != null)
            {
                room.audience.Add(Context.ConnectionId);
                foreach (DrawPoint drawPoint in room.drawPoints)
                {
                    await Clients.Caller.SendAsync("UpdateDrawing", 
                        drawPoint.x, 
                        drawPoint.y, 
                        drawPoint.dragging);
                }
            }
        }
    }

    internal class Room
    {
        public int roomId { get; set; }
        public List<DrawPoint> drawPoints { get; set; }
        public string artist { get; set; }
        public List<string> audience { get; set; }
        public Queue<Player> players { get; set; }
        public int roundNo { get; set; } = 1;
    }

    public class Player
    {
        public int team { get; set; }
        public string name { get; set; }
    }

    internal class DrawPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public bool dragging { get; set; }
    }
}
