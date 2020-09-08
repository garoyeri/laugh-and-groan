namespace LaughAndGroan.Actions
{
    using Microsoft.Extensions.Configuration;

    public class ConfigurationProvider
    {
        public ConfigurationProvider()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }

        public Settings Settings => Configuration.Get<Settings>();
    }
}
