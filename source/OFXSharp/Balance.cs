using System;
using System.Globalization;
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
         var tempLedgerBalance = ledgerNode.GetValue("//BALAMT");

         if (!String.IsNullOrEmpty(tempLedgerBalance))
         {
            // ***** Forced Invariant Culture. 
            // If you don't force it, it will use the computer's default (defined in windows control panel, regional settings)
            // So, if the number format of the computer in use it's different from OFX standard (i suppose the english/invariant), 
            // the next line of could crash or (worse) the number would be wrongly interpreted. 
            // For example, my computer has a brazilian regional setting, with "." as thousand separator and "," as 
            // decimal separator, so the value "10.99" (ten 'dollars' (or whatever currency) and ninetynine cents) would be interpreted as "1099" 
            // (one thousand and ninetynine dollars - the "." would be ignored)
            LedgerBalance = Convert.ToDecimal(tempLedgerBalance, CultureInfo.InvariantCulture);
         }
         else
         {
            throw new OFXParseException("Ledger balance has not been set");
         }

         // ***** OFX files from my bank don't have the 'avaliableNode' node, so i manage a null situation
         if (avaliableNode == null)
         {
            AvaliableBalance = 0;

            // ***** this member veriable should be a nullable DateTime, declared as: 
            // public DateTime? LedgerBalanceDate { get; set; }
            // and next line could be:
            // AvaliableBalanceDate = null; 
            AvaliableBalanceDate = new DateTime();
         }
         else
         {
            var tempAvaliableBalance = avaliableNode.GetValue("//BALAMT");

            if (!String.IsNullOrEmpty(tempAvaliableBalance))
            {
               // ***** Forced Invariant Culture. (same commment as above)
               AvaliableBalance = Convert.ToDecimal(tempAvaliableBalance, CultureInfo.InvariantCulture);
            }
            else
            {
               throw new OFXParseException("Avaliable balance has not been set");
            }
            AvaliableBalanceDate = avaliableNode.GetValue("//DTASOF").ToDate();
         }

         LedgerBalanceDate = ledgerNode.GetValue("//DTASOF").ToDate();
      }
   }
}