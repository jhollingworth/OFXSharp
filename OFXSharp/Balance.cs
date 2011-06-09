using System;
using System.Xml;

namespace OFXSharp
{
    public class Balance
    {
        public decimal LedgerBalance { get; set; }

        public DateTime LedgerBalanceDate { get; set; }

        public decimal AvaliableBalance { get; set; }

        public DateTime AvaliableBalanceDate { get; set; }

        public Balance(XmlNode ledgerNode, XmlNode avaliableNode)
        {
            string tempLedgerBalance = ledgerNode.GetValue("BALAMT");

            if (!String.IsNullOrEmpty(tempLedgerBalance))
            {
                LedgerBalance = Convert.ToDecimal(tempLedgerBalance);
            }
            else
            {
                throw new OFXParseException("Ledger balance has not been set");
            }

            string tempAvaliableBalance = avaliableNode.GetValue("BALAMT");

            if (!String.IsNullOrEmpty(tempAvaliableBalance))
            {
                AvaliableBalance = Convert.ToDecimal(tempAvaliableBalance);
            }
            else
            {
                throw new OFXParseException("Avaliable balance has not been set");
            }

            LedgerBalanceDate = ledgerNode.GetValue("DTASOF").ToDate();
            AvaliableBalanceDate = avaliableNode.GetValue("DTASOF").ToDate();
        }
    }
}