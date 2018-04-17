namespace CogisoftConnector.Models.Cogisoft.CogisoftResponseModels
{
    //Struktura odpowiedzi na zapytanie o status asynchronicznej operacji
    public class AsyncProcessingResultResponseCogisoftModel
    {
        //{"f":"json","ci":[{"processed":true,"id":123,"code":0,"type":"readyCommisionInfo"}]}

        public string f { get; set; }
        public Ci[] ci { get; set; }

        public class Ci
        {
            public bool processed { get; set; }
            public string id { get; set; }
            public int code { get; set; }
            public string type { get; set; }
        }
    }
}