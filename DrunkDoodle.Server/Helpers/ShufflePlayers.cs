using DrunkDoodle.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrunkDoodle.Server.Helpers
{
    public static class PlayerShuffler
    {
        private static Random _random = new Random();

        /*
        Creates a queue from a list of players. 
        The queue is both shuffled in the order of teams, 
        and the order of the players on each team.
        */
        public static Queue<Player> Shuffle(List<Player> players)
        {
            // Get list of all teams in random order
            List<int> teams = players.Select(p => p.team).Distinct().ToList()
                .OrderBy(a => Guid.NewGuid()).ToList();

            Queue<Player> shuffledPlayers = new Queue<Player>();
            while (players.Count > 0)
            {
                foreach (int team in teams)
                {
                    // Get all players of the specific team
                    IEnumerable<Player> playersInTeam = players.Where(pt => pt.team == team);
                    if (playersInTeam.Count() > 0)
                    {
                        // Get one random player from team, add to the queue 
                        // and remove from initial list of players
                        Player p = playersInTeam.ElementAt(_random.Next(playersInTeam.Count()));
                        shuffledPlayers.Enqueue(p);
                        players.Remove(p);
                    }
                }
            }
            return shuffledPlayers;
        }
    }
}
