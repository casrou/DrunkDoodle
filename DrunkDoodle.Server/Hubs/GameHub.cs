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
        public async Task CreateRoom(List<Team> teams)
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
                    teams = teams
                };
                _rooms.Add(room);
                await Clients.Caller.SendAsync("RoomCreated", roomId);
            }
            else
            {
                await Clients.Caller.SendAsync("RoomCreated", temproom.roomId);
            }
        }

        public async Task StartRound()
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            await Clients.Caller.SendAsync("NewRound", _words[_random.Next(_words.Count)]);
            await Clients.Clients(room.audience).SendAsync("NewRound");
            //DateTime thirtySeconds = DateTime.Now.AddSeconds(30).ToUniversalTime();
            //long countdown = (long)(thirtySeconds - new DateTime(1970, 1, 1)).TotalMilliseconds;
            //await Clients.Caller.SendAsync("NewRound",
            //    _words[_random.Next(_words.Count)],
            //    countdown);
            //await Task.Delay(30000).ContinueWith(t => EndRound());
        }

        public async Task EndRound()
        {
            Room room = _rooms.FirstOrDefault(r => r.artist == Context.ConnectionId);
            if (room == null) return;
            await Clients.Caller.SendAsync("PrepareRound", "Casper_Server");
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
        public List<Team> teams { get; set; }
    }

    public class Team
    {
        public int teamNo { get; set; }
        public List<string> members { get; set; }
    }

    internal class DrawPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public bool dragging { get; set; }
    }
}
