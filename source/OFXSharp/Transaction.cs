using System;
using System.Globalization;
using System.Xml;

namespace OFXSharp
{
   public class Transaction
   {
      public OFXTransactionType TransType { get; set; }

      public DateTime Date { get; set; }

      public decimal Amount { get; set; }

      public string TransactionID { get; set; }

      public string Name { get; set; }

      public DateTime TransactionInitializationDate { get; set; }

      public DateTime FundAvaliabilityDate { get; set; }

      public string Memo { get; set; }

      public string IncorrectTransactionID { get; set; }

      public TransactionCorrectionType TransactionCorrectionAction { get; set; }

      public string ServerTransactionID { get; set; }

      public string CheckNum { get; set; }

      public string ReferenceNumber { get; set; }

      public string Sic { get; set; }

      public string PayeeID { get; set; }

      public Account TransactionSenderAccount { get; set; }

      public string Currency { get; set; }

      public Transaction()
      {
      }

      public Transaction(XmlNode node, string currency)
      {
         TransType = GetTransactionType(node.GetValue("TRNTYPE"));
         Date = node.GetValue("DTPOSTED").ToDate();
         TransactionInitializationDate = node.GetValue("DTUSER").ToDate();
         FundAvaliabilityDate = node.GetValue("DTAVAIL").ToDate();

         try
         {
            Amount = Convert.ToDecimal(node.GetValue("TRNAMT"), CultureInfo.InvariantCulture);
         }
         catch (Exception ex)
         {
            throw new OFXParseException("Transaction Amount unknown", ex);
         }

         try
         {
            TransactionID = node.GetValue("FITID");
         }
         catch (Exception ex)
         {
            throw new OFXParseException("Transaction ID unknown", ex);
         }

         IncorrectTransactionID = node.GetValue("CORRECTFITID");


         //If Transaction Correction Action exists, populate
         var tempCorrectionAction = node.GetValue("CORRECTACTION");

         TransactionCorrectionAction = !String.IsNullOrEmpty(tempCorrectionAction)
                                          ? GetTransactionCorrectionType(tempCorrectionAction)
                                          : TransactionCorrectionType.NA;

         ServerTransactionID = node.GetValue("SRVRTID");
         CheckNum = node.GetValue("CHECKNUM");
         ReferenceNumber = node.GetValue("REFNUM");
         Sic = node.GetValue("SIC");
         PayeeID = node.GetValue("PAYEEID");
         Name = node.GetValue("NAME");
         Memo = node.GetValue("MEMO");

         //If differenct currency to CURDEF, populate currency 
         if (NodeExists(node, "CURRENCY"))
            Currency = node.GetValue("CURRENCY");
         else if (NodeExists(node, "ORIGCURRENCY"))
            Currency = node.GetValue("ORIGCURRENCY");
            //If currency not different, set to CURDEF
         else
            Currency = currency;

         //If senders bank/credit card details avaliable, add
         if (NodeExists(node, "BANKACCTTO"))
            TransactionSenderAccount = new Account(node.SelectSingleNode("BANKACCTTO"), AccountType.BANK);
         else if (NodeExists(node, "CCACCTTO"))
            TransactionSenderAccount = new Account(node.SelectSingleNode("CCACCTTO"), AccountType.CC);
      }

      /// <summary>
      /// Returns TransactionType from string version
      /// </summary>
      /// <param name="transactionType">string version of transaction type</param>
      /// <returns>Enum version of given transaction type string</returns>
      private OFXTransactionType GetTransactionType(string transactionType)
      {
         return (OFXTransactionType) Enum.Parse(typeof (OFXTransactionType), transactionType);
      }

      /// <summary>
      /// Returns TransactionCorrectionType from string version
      /// </summary>
      /// <param name="transactionCorrectionType">string version of Transaction Correction Type</param>
      /// <returns>Enum version of given TransactionCorrectionType string</returns>
      private TransactionCorrectionType GetTransactionCorrectionType(string transactionCorrectionType)
      {
         return (TransactionCorrectionType) Enum.Parse(typeof (TransactionCorrectionType), transactionCorrectionType);
      }

      /// <summary>
      /// Checks if a node exists
      /// </summary>
      /// <param name="node">Node to search in</param>
      /// <param name="xpath">XPath to node you want to see if exists</param>
      /// <returns></returns>
      private bool NodeExists(XmlNode node, string xpath)
      {
         return (node.SelectSingleNode(xpath) != null);
      }
   }
}