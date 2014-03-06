using System;
using System.Xml;

namespace OFXSharp
{
    public class SignOn
    {
        public string StatusSeverity { get; set; }

        public DateTime DTServer { get; set; }

        public int StatusCode { get; set; }

        public string Language { get; set; }

        public string IntuBid { get; set; }

        public SignOn(XmlNode node)
        {
            StatusCode = Convert.ToInt32(node.GetValue("//CODE"));
            StatusSeverity = node.GetValue("//SEVERITY");
            DTServer = node.GetValue("//DTSERVER").ToDate();
            Language = node.GetValue("//LANGUAGE");
            IntuBid = node.GetValue("//INTU.BID");
        }
    }
}