using Sycapooke.Migrator;

var connectionString = Environment.GetEnvironmentVariable(MigratorApp.ConnectionStringVariable);
return args.Contains(MigratorApp.ProvisionFounderFlag)
    ? MigratorApp.RunProvisionFounder(Console.Out, connectionString, Environment.GetEnvironmentVariable(MigratorApp.FounderVariable))
    : MigratorApp.Run(Console.Out, connectionString);
