namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public interface IRequestCogisoftModel
    {
        string GetSOAPEnvelope();
        string GetSOAPEndpoint();
        void SetToken(string token);
    }
}
