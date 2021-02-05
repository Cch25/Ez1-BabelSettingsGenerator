namespace EasyOne.BabelSettings.Models
{
    public class ApplicationSettings
    {
        public ConnectionString ConnectionString { get; set; }
    }

    public class ConnectionString
    {
        public string MongoConnection { get; set; }
        public string Database { get; set; }
    }

}
