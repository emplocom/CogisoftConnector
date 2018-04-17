namespace CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels
{
    public static class RequestEnvelope
    {
        public static string Envelope = @"
        <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
        <soapenv:Header></soapenv:Header>
        <soapenv:Body>

        <ns2:request xmlns:ns2=""http://srv.dash.cogisoft.pl/"" xmlns:ns3=""http://ws.dash.cogisoft.pl/"" r=""json""> 
        <json>

        </json> 
        </ns2:request>

        </soapenv:Body>
        </soapenv:Envelope>
";
    }
}
