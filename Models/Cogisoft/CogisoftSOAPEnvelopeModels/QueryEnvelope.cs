namespace CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels
{
    public static class QueryEnvelope
    {
        public static string Envelope = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
        <soapenv:Header></soapenv:Header>
        <soapenv:Body>

        <ns2:queryData xmlns:ns2=""http://srv.dash.cogisoft.pl/"" xmlns:ns3=""http://ws.dash.cogisoft.pl/"">
        <json>
        
        </json>
        </ns2:queryData>

        </soapenv:Body>
        </soapenv:Envelope>";
    }
}
