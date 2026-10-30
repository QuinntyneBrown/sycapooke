using Sycapooke.Migrator;

return MigratorApp.Run(Console.Out, Environment.GetEnvironmentVariable(MigratorApp.ConnectionStringVariable));
