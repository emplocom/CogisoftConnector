namespace CogisoftConnector.Models.Cogisoft.CogisoftResponseModels
{
    public class LoginResponseCogisoftModel
    {
        public LogonData logon { get; set; }
    }

    public class LogonData
    {
        public string token { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}