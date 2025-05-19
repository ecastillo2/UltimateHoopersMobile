using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class SquadTeamContext : DbContext
    {
        public SquadTeamContext(DbContextOptions<SquadTeamContext> options) : base(options)
		{

		}

        public DbSet<SquadTeam> SquadTeam { get; set; }
    }
}
