using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("workstation id=ComPlanDb.mssql.somee.com;packet size=4096;user id=tivsas_SQLLogin_1;pwd=f9ca54zls2;data source=ComPlanDb.mssql.somee.com;persist security info=False;initial catalog=ComPlanDb;TrustServerCertificate=True");

        return new AppDbContext(optionsBuilder.Options);
    }
}
