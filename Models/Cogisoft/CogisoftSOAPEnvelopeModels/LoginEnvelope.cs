namespace CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels
{
    public static class LoginEnvelope
    {
        public static string Envelope = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
        <soapenv:Header></soapenv:Header>
        <soapenv:Body>

        <ns2:login xmlns:ns2=""http://srv.dash.cogisoft.pl/"">
        <json>
        
        </json>

        </ns2:login>
        </soapenv:Body>
        </soapenv:Envelope>";
    }
}
