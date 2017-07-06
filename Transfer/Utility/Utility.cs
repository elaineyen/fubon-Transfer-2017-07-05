﻿using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Mvc;

namespace Transfer.Utility
{
    [DataContract]
    public class MSGReturnModel
    {
        [DataMember]
        [DisplayName("Message Return Flag")]
        public bool RETURN_FLAG { get; set; }

        [DataMember]
        [DisplayName("Message Reason Code")]
        public string REASON_CODE { get; set; }

        [DataMember]
        [DisplayName("Message Description")]
        public string DESCRIPTION { get; set; }

        [DataMember]
        public JsonResult Datas { get; set; }
    }

}