using System.Configuration;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class LoginRequestCogisoftModel : IRequestCogisoftModel
    {
        public LoginData c { get; set; }

        public LoginRequestCogisoftModel(string linkName, string linkPassword, string operatorLogin, string operatorPassword)
        {
            c = new LoginData()
            {
                linkName = linkName,
                linkPassword = linkPassword,
                operatorLogin = operatorLogin,
                operatorPassword = operatorPassword
            };
        }

        public class LoginData
        {
            public string linkName;
            public string linkPassword;
            public string operatorLogin;
            public string operatorPassword;
        }

        public string GetSOAPEnvelope()
        {
            return LoginEnvelope.Envelope;
        }

        public string GetSOAPEndpoint()
        {
            return $@"{ConfigurationManager.AppSettings["EndpointAddress"]}/DASH/Login?wsdl";
        }

        public void SetToken(string token)
        {
            //noop
        }
    }
}
