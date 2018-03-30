using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CogisoftConnector.Models.WebhookModels.CogisoftResponseModels
{
    public class AsyncCommisionResponseCogisoftModel
    {
        //{"f":"json","commision":[1883056]}

        public string f = "json";
        public int[] commision = new int[1];
    }
}
