using Microsoft.EntityFrameworkCore.Design;

namespace WebTennisFieldReservation.Data
{
    public class DesignTimeCourtComplexDbContextFactory : IDesignTimeDbContextFactory<CourtComplexDbContext>
    {        
        private readonly string DbFolder = Directory.GetCurrentDirectory();
        private readonly string DbFileName = "CourtComplex.mdf";

        public CourtComplexDbContext CreateDbContext(string[] args)
        {
            string connString = @"Server=(LocalDB)\MSSQLLocalDB;Integrated Security=True;AttachDbFilename=""" + Path.Combine(DbFolder, DbFileName) + @"""";

            return CourtComplexDbContext.CreateDbContext(connString);
        }
    }
}
