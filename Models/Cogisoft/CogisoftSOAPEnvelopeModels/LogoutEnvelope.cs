﻿namespace CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels
{
    public class LogoutEnvelope
    {
        public static string Envelope = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
        <soapenv:Header></soapenv:Header>
        <soapenv:Body>

        <ns2:logout xmlns:ns2=""http://srv.dash.cogisoft.pl/"">
        <token></token>
        </ns2:logout>

        </soapenv:Body>
        </soapenv:Envelope>";
    }
}