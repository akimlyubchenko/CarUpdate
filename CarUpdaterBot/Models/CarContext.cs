using System.Data.Entity;

namespace CarUpdaterBot.Models
{
    public class CarContext : DbContext
    {
        public CarContext() : base("DefaultConnection") { }

        public DbSet<CarInfo> Cars { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarInfo>().ToTable("CarUpdate");
            Database.SetInitializer<CarContext>(new Initializer());
            base.OnModelCreating(modelBuilder);
        }
    }

    public class Initializer : CreateDatabaseIfNotExists<CarContext>
    {
        protected override void Seed(CarContext context)
        {
            var car = new CarInfo
            {
                Name = "Mazda 3 BK ",
                Price = "4 150",
                Url = "https://cars.av.by/mazda/3/14366318",
                Year = "2004",
                Description = "автомат, 2.0 л., бензин, седан, 180000 км"
            };

            context.Cars.Add(car);
            context.SaveChanges();
            base.Seed(context);
        }
        
    }
}