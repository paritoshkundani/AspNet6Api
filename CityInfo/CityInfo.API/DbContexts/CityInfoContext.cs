using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
    public class CityInfoContext : DbContext
    {
        // in order to get migrations, need to add microsoft.entityframework.tools package (ex. add-migration CityInfoDBInitialMigration)
        // he also added an extension SqlLite/Sql Server Compact Toolbox

        // null! is the null forgiving operator, VS will warn that these could be null
        // dbcontext though makes sure they won't, so rather than making them nullable
        // we tell VS to ignore that warning with null!
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<PointOfInterest> PointOfInterest { get; set; } = null!;

        // one way to send it connection string is to override the OnConfiguring and provide it there
        // we won't do that, commented it out below, will instead use constructor
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseSqlite("connectionstring");
            base.OnConfiguring(optionsBuilder);
        }

        // we will provide the options in Program.cs when we register the context
        public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options)
        {

        }

        // use this to customize the entities setup if we dont like how it was done via migration
        // also use to seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // seeding data
            modelBuilder.Entity<City>().HasData(
                new City("New York City") { 
                        Id = 1, 
                        Description = "The one with the big park."
                    },
                new City("Antwerp")
                    {
                        Id = 2,
                        Description = "The one with the catheral that was never really finished."
                    },
                new City("Paris")
                    {
                        Id = 3,
                        Description = "The one with the big tower."
                    }
                );

            modelBuilder.Entity<PointOfInterest>().HasData(
                    new PointOfInterest("Central Park")
                    {
                        Id = 1,
                        CityId = 1,
                        Description = "The most visited urban park in the United States."
                    },
                    new PointOfInterest("Empire State Building")
                    {
                        Id = 2,
                        CityId = 1,
                        Description = "A 102-story skyscraper located in Midtown Manhattan."
                    },
                    new PointOfInterest("Cathedral of Our Lady")
                    {
                        Id = 3,
                        CityId = 2,
                        Description = "A Gothic style cathedral, conceived by architects Jan and Pieter Appelmans."
                    },
                    new PointOfInterest("Antwerp Central Station")
                    {
                        Id = 4,
                        CityId = 2,
                        Description = "The the finest example of railway architecture in Belgium."
                    },
                    new PointOfInterest("Eiffel Tower")
                    {
                        Id = 5,
                        CityId = 3,
                        Description = "A wrought iron lattice tower on the Champ de Mars, named after engineer Gustave Eiffel."
                    },
                    new PointOfInterest("The Louvre")
                    {
                        Id = 6,
                        CityId = 3,
                        Description = "The world's largest museum."
                    }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
