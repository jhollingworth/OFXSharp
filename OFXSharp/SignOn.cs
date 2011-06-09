using System;
using System.Xml;

namespace OFXSharp
{
    public class SignOn
    {
        public string StatusSeverity { get; set; }

        public Int64 DTServer { get; set; }

        public int StatusCode { get; set; }

        public string Language { get; set; }

        public string IntuBid { get; set; }

        public SignOn(XmlNode node)
        {
            StatusCode = Convert.ToInt32(node.GetValue("STATUS/CODE"));
            StatusSeverity = node.GetValue("STATUS/SEVERITY");
            DTServer = Convert.ToInt64(node.GetValue("DTSERVER"));
            Language = node.GetValue("LANGUAGE");
            IntuBid = node.GetValue("INTU.BID");
        }
    }
}