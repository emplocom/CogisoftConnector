using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CogisoftConnector.Models.WebhookModels.CogisoftResponseModels
{
    public class VacationDataResponseCogisoftModel
    {
        //{
        //    "f" : "json", "qr" : [ {
        //        "trc" : 1, "v" : -2, "h" : {
        //            "c" : [ "fk", "string", "numeric", "string","numeric","numeric" ]
        //        }, "p" : [ {
        //            "i" : 0, "left" : 0, "off" : 0, "r" : [ {
        //                "sc" : [ 29, {
        //                    "n":"1"
        //                }, {
        //                    "n":"1"
        //                }, {
        //                    "n":"1"
        //                }, {
        //                    "n":"1"
        //                }, {
        //                    "n":"1"
        //                } ]
        //            } ]
        //        } ], "qid" : "a46bf4f1"
        //    } ]
        //}

        public class H
        {
            public List<string> c { get; set; }
        }

        public class R
        {
            public List<object> sc { get; set; }
        }

        public class P
        {
            public int i { get; set; }
            public int left { get; set; }
            public int off { get; set; }
            public List<R> r { get; set; }
        }

        public class Qr
        {
            public int trc { get; set; }
            public int v { get; set; }
            public H h { get; set; }
            public List<P> p { get; set; }
            public string qid { get; set; }
        }

        public string f { get; set; }
        public List<Qr> qr { get; set; }

        public List<R> GetEmployeeCollection()
        {
            return this.qr[0].p[0].r;
        }

        public bool AnyRemainingObjectsLeft()
        {
            return this.qr[0].p[0].left > 0;
        }
    }
}
