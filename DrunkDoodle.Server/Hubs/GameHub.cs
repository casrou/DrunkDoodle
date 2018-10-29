using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DrunkDoodle.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private IEnumerable<Word> _words;
        private static List<Room> _rooms;
        private static Random _random;

        public GameHub(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _words = InitializeWords();
            _rooms = new List<Room>();
            _random = new Random();
        }      

        private List<Word> InitializeWords()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string json = File.ReadAllText(webRootPath + "/words/words.json");
            List<Word> words = JsonConvert.DeserializeObject<List<Word>>(json);
            return words;
        }
        
        public async Task CreateRoom(List<Player> players, RoomRules roomRules)
        {
            Room temproom = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (temproom == null)
            {
                int roomId = _random.Next(100, 1000);
                Room room = new Room()
                {
                    roomId = roomId,
                    artistDevice = Context.ConnectionId,
                    players = shufflePlayers(players),
                    roomRules = roomRules
                };
                _rooms.Add(room);
                await Clients.Caller.SendAsync("RoomCreated", roomId);
            }
            else
            {
                await Clients.Caller.SendAsync("RoomCreated", temproom.roomId);
            }
        }

        private Queue<Player> shufflePlayers(List<Player> players)
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
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            List<Word> wordsInLanguage = _words.Where(w => w.language == room.wordLanguage.ToLower()).ToList();
            Word word = wordsInLanguage[_random.Next(wordsInLanguage.Count)];
            room.currentWord = word;
            await Clients.Caller.SendAsync("NewRound", word.content);
            await Clients.Clients(room.audienceDevices).SendAsync("NewRound", word.content);
        }

        public async Task EndRound()
        {
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (room == null) return;
            Player nextDrawer = getNextDrawer(room);                     
            await Clients.Caller.SendAsync("PrepareRound", nextDrawer.name);
            room.nowDrawing = nextDrawer;
            await Clients.Clients(room.audienceDevices).SendAsync("EndRound", room.currentWord.content);
        }

        private Player getNextDrawer(Room room)
        {
            Player nextDrawer = room.players.Dequeue();
            room.players.Enqueue(nextDrawer);
            return nextDrawer;
        }

        public async Task WordGuessed()
        {
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (room == null) return;
            Player nowDrawing = room.nowDrawing;   
            nowDrawing.score++;    
            await Clients.Caller.SendAsync("NowDrinking",
                room.players.Where(p => p.team != nowDrawing.team),
                room.drinkAmount,
                room.drinkType);
            await Clients.Clients(room.audienceDevices).SendAsync("NowDrinking",
                room.players.Where(p => p.team != nowDrawing.team),
                room.drinkAmount,
                room.drinkType);
        }

        public async Task WordNotGuessed(){
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (room == null) return;     
            await Clients.Caller.SendAsync("NowDrinking",
                room.players.Where(p => p.team == room.nowDrawing.team),
                room.drinkAmount,
                room.drinkType);
            await Clients.Clients(room.audienceDevices).SendAsync("NowDrinking",
                room.players.Where(p => p.team == room.nowDrawing.team),
                room.drinkAmount,
                room.drinkType);
        }

        public async Task ClearCanvas()
        {
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (room == null) return;
            room.drawPoints.Clear();
            await Clients.Caller.SendAsync("ClearCanvas");
            await Clients.Clients(room.audienceDevices).SendAsync("ClearCanvas");
        }

        public async Task IsDrawing(double x, double y, bool dragging)
        {
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if(room != null)
            {
                room.drawPoints.Add(new DrawPoint() { x = x, y = y, dragging = dragging });
                await Clients.Clients(room.audienceDevices).SendAsync("UpdateDrawing", x, y, dragging);
            }
        }
        
        public async Task JoinAudience(int roomId)
        {
            Room room = _rooms.FirstOrDefault(r => r.roomId == roomId);
            if (room != null)
            {
                room.audienceDevices.Add(Context.ConnectionId);
                foreach (DrawPoint drawPoint in room.drawPoints)
                {
                    await Clients.Caller.SendAsync("UpdateDrawing", 
                        drawPoint.x, 
                        drawPoint.y, 
                        drawPoint.dragging);
                }
            }
        }

        public async Task ShowScoreboard()
        {
            Room room = _rooms.FirstOrDefault(r => r.audienceDevices.Contains(Context.ConnectionId));
            if (room == null) return;
            await Clients.Caller.SendAsync("ShowScoreboard", getTeams(room.players));
        }

        private List<Team> getTeams(Queue<Player> players)
        {
            List<Team> teams = new List<Team>();
            foreach (Player player in players)
            {
                Team team;
                if(!teams.Exists(t => t.teamNo == player.team)){
                    team = new Team() { teamNo = player.team };
                    teams.Add(team);
                } else
                {
                    team = teams.First(t => t.teamNo == player.team);
                }                    
                team.players.Add(player);
                team.teamScore += player.score;                
            }
            return teams.OrderByDescending(t => t.teamScore).ToList();
        }
    }

    public class Word
    {
        public string content { get; set; }
        public string language { get; set; }
    }

    public class Room
    {
        public int roomId { get; set; }
        public string artistDevice { get; set; }
        public List<string> audienceDevices { get; set; } = new List<string>();

        public Queue<Player> players { get; set; }
        public Drawing currentDrawing { get; set; }
        public RoomRules roomRules { get; set; }
    }

    public class RoomRules
    {
        public int drinkAmount { get; set; }
        public string drinkType { get; set; }
        public string wordLanguage { get; set; }
    }

    public class Player
    {
        public int team { get; set; }
        public string name { get; set; }
        public int score { get; set; }
    }

    public class Team
    {
        public int teamNo { get; set; }
        public List<Player> players { get; set; } = new List<Player>();
        public int teamScore { get; set; }
    }

    public class Drawing
    {
        public Word word { get; set; }
        public Player artist { get; set; }
        public IList<DrawPoint> drawPoints { get; set; }
    }

    public class DrawPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public bool dragging { get; set; }
    }
}
