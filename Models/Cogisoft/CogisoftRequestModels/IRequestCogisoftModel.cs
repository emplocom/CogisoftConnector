using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogisoftConnector.Models.WebhookModels.CogisoftRequestModels
{
    public interface IRequestCogisoftModel
    {
        string GetSOAPEnvelope();
        string GetSOAPEndpoint();
        void SetToken(string token);
    }
}
