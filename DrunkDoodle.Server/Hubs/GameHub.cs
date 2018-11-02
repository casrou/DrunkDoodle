using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrunkDoodle.Server.Models;
using DrunkDoodle.Server.Helpers;

namespace DrunkDoodle.Server.Hubs
{
    public class GameHub : Hub
    {
        private List<Room> _rooms = new List<Room>();
        private Random _random = new Random();
        private IEnumerable<Word> _words;

        public GameHub(IHostingEnvironment hostingEnvironment)
        {            
            _words = WordInitializer.InitializeWords(hostingEnvironment);
        }      
        
        public async Task CreateRoom(List<Player> players, RoomRules roomRules)
        {
            Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
            if (room == null)
            {
                room = new Room()
                {
                    roomId = _random.Next(1000, 9999),
                    artistDevice = Context.ConnectionId,
                    players = PlayerShuffler.Shuffle(players),
                    roomRules = roomRules
                };
                _rooms.Add(room);
            }
            await Clients.Caller.SendAsync("RoomCreated", room.roomId);
        }

        //public async Task StartRound()
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    List<Word> wordsInLanguage = _words.Where(w => w.language == room.wordLanguage.ToLower()).ToList();
        //    Word word = wordsInLanguage[_random.Next(wordsInLanguage.Count)];
        //    room.currentWord = word;
        //    await Clients.Caller.SendAsync("NewRound", word.content);
        //    await Clients.Clients(room.audienceDevices).SendAsync("NewRound", word.content);
        //}

        //public async Task EndRound()
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    if (room == null) return;
        //    Player nextDrawer = getNextDrawer(room);                     
        //    await Clients.Caller.SendAsync("PrepareRound", nextDrawer.name);
        //    room.nowDrawing = nextDrawer;
        //    await Clients.Clients(room.audienceDevices).SendAsync("EndRound", room.currentWord.content);
        //}

        //private Player getNextDrawer(Room room)
        //{
        //    Player nextDrawer = room.players.Dequeue();
        //    room.players.Enqueue(nextDrawer);
        //    return nextDrawer;
        //}

        //public async Task WordGuessed()
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    if (room == null) return;
        //    Player nowDrawing = room.nowDrawing;   
        //    nowDrawing.score++;    
        //    await Clients.Caller.SendAsync("NowDrinking",
        //        room.players.Where(p => p.team != nowDrawing.team),
        //        room.drinkAmount,
        //        room.drinkType);
        //    await Clients.Clients(room.audienceDevices).SendAsync("NowDrinking",
        //        room.players.Where(p => p.team != nowDrawing.team),
        //        room.drinkAmount,
        //        room.drinkType);
        //}

        //public async Task WordNotGuessed(){
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    if (room == null) return;     
        //    await Clients.Caller.SendAsync("NowDrinking",
        //        room.players.Where(p => p.team == room.nowDrawing.team),
        //        room.drinkAmount,
        //        room.drinkType);
        //    await Clients.Clients(room.audienceDevices).SendAsync("NowDrinking",
        //        room.players.Where(p => p.team == room.nowDrawing.team),
        //        room.drinkAmount,
        //        room.drinkType);
        //}

        //public async Task ClearCanvas()
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    if (room == null) return;
        //    room.drawPoints.Clear();
        //    await Clients.Caller.SendAsync("ClearCanvas");
        //    await Clients.Clients(room.audienceDevices).SendAsync("ClearCanvas");
        //}

        //public async Task IsDrawing(double x, double y, bool dragging)
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.artistDevice == Context.ConnectionId);
        //    if(room != null)
        //    {
        //        room.drawPoints.Add(new DrawPoint() { x = x, y = y, dragging = dragging });
        //        await Clients.Clients(room.audienceDevices).SendAsync("UpdateDrawing", x, y, dragging);
        //    }
        //}
        
        //public async Task JoinAudience(int roomId)
        //{
        //    Room room = _rooms.FirstOrDefault(r => r.roomId == roomId);
        //    if (room != null)
        //    {
        //        room.audienceDevices.Add(Context.ConnectionId);
        //        foreach (DrawPoint drawPoint in room.drawPoints)
        //        {
        //            await Clients.Caller.SendAsync("UpdateDrawing", 
        //                drawPoint.x, 
        //                drawPoint.y, 
        //                drawPoint.dragging);
        //        }
        //    }
        //}

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
}
