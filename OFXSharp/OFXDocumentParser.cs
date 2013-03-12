﻿using Sgml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace OFXSharp
{
   public class OFXDocumentParser
   {
      public OFXDocument Import(FileStream stream)
      {
         using (var reader = new StreamReader(stream))
         {
            return Import(reader.ReadToEnd());
         }
      }

      public OFXDocument Import(string ofx)
      {
         return ParseOfxDocument(ofx);
      }

      private OFXDocument ParseOfxDocument(string ofxString)
      {
         //If OFX file in SGML format, convert to XML
         if (!IsXmlVersion(ofxString))
         {
            ofxString = SGMLToXML(ofxString);
         }
         return Parse(ofxString);
      }

      private OFXDocument Parse(string ofxString)
      {
          var ofx = new OFXDocument {AccType = GetAccountType(ofxString)};

          ofx.Statements = new List<OFXStatement>();

         //Load into xml document
         var xml = new XmlDocument();
         xml.Load(new StringReader(ofxString));

          // need to parse this for multiple banks
         // xpath = ofx/CREDITCARDMSGSRSV1/CCSTMTTRNRS      --  /CCSTMTRS
         // xpath = ofx/BANKMSGSRSV1/STMTTRNRS              --  /STMTRS
         var STMTTRNRS = xml.SelectNodes(GetXPath(ofx.AccType, OFXSection.STATEMENTS));

         foreach (XmlNode doc in STMTTRNRS)
         {
             OFXStatement ofxStatement = new OFXStatement();

             var currencyNode = doc.SelectSingleNode(GetRelativeXPath(ofx.AccType, OFXSection.CURRENCY));

             if (currencyNode != null)
             {
                 ofxStatement.Currency = currencyNode.FirstChild.Value;
             }
             else
             {
                 throw new OFXParseException("Currency not found");
             }

             //Get sign on node from OFX file
             var signOnNode = xml.SelectSingleNode(Resources.SignOn);

             //If exists, populate signon obj, else throw parse error
             if (signOnNode != null)
             {
                 ofxStatement.SignOn = new SignOn(signOnNode);
             }
             else
             {
                 throw new OFXParseException("Sign On information not found");
             }

             //Get Account information for ofx doc
             var accountNode = doc.SelectSingleNode(GetRelativeXPath(ofx.AccType, OFXSection.ACCOUNTINFO));

             //If account info present, populate account object
             if (accountNode != null)
             {
                 ofxStatement.Account = new Account(accountNode, ofx.AccType);
             }
             else
             {
                 throw new OFXParseException("Account information not found");
             }

             //Get list of transactions
             ImportTransations(ofxStatement, doc, ofx);

             //Get balance info from ofx doc
             var ledgerNode = doc.SelectSingleNode(GetRelativeXPath(ofx.AccType, OFXSection.LEDGERBAL));
             var avaliableNode = doc.SelectSingleNode(GetRelativeXPath(ofx.AccType, OFXSection.AVAILBAL));

             //If balance info present, populate balance object
             // ***** OFX files from my bank don't have the 'avaliableNode' node, so i manage a 'null' situation
             if (ledgerNode != null) // && avaliableNode != null
             {
                 ofxStatement.Balance = new Balance(ledgerNode, avaliableNode);
             }
             else
             {
                 throw new OFXParseException("Balance information not found");
             }

             ofx.Statements.Add(ofxStatement);
         }
          
         return ofx;

      }


      /// <summary>
      /// Returns the correct xpath to specified section for given account type
      /// </summary>
      /// <param name="type">Account type</param>
      /// <param name="section">Section of OFX document, e.g. Transaction Section</param>
      /// <exception cref="OFXException">Thrown in account type not supported</exception>
      private string GetXPath(AccountType type, OFXSection section)
      {
          string xpath, accountInfo;

          switch (type)
          {
              case AccountType.BANK:
                  xpath = Resources.BankAccount;
                  accountInfo = "/BANKACCTFROM";
                  break;
              case AccountType.CC:
                  xpath = Resources.CCAccount;
                  accountInfo = "/CCACCTFROM";
                  break;
              default:
                  throw new OFXException("Account Type not supported. Account type " + type);
          }

          switch (section)
          {
              case OFXSection.ACCOUNTINFO:
                  return xpath + accountInfo;
              case OFXSection.BALANCE:
                  return xpath;
              case OFXSection.TRANSACTIONS:
                  return xpath + "/BANKTRANLIST";
              case OFXSection.SIGNON:
                  return Resources.SignOn;
              case OFXSection.CURRENCY:
                  return xpath + "/CURDEF";
              case OFXSection.STATEMENTS:
                  return xpath;
              default:
                  throw new OFXException("Unknown section found when retrieving XPath. Section " + section);
          }
      }

      /// <summary>
      /// Returns the correct xpath to specified section for given account type
      /// </summary>
      /// <param name="type">Account type</param>
      /// <param name="section">Section of OFX document, e.g. Transaction Section</param>
      /// <exception cref="OFXException">Thrown in account type not supported</exception>
      private string GetRelativeXPath(AccountType type, OFXSection section)
      {
          string xpath, accountInfo;

          switch (type)
          {
              case AccountType.BANK:
                  xpath = Resources.BankAccount;
                  accountInfo = "BANKACCTFROM";
                  break;
              case AccountType.CC:
                  xpath = Resources.CCAccount;
                  accountInfo = "CCACCTFROM";
                  break;
              default:
                  throw new OFXException("Account Type not supported. Account type " + type);
          }

          switch (section)
          {
              case OFXSection.ACCOUNTINFO:
                  return accountInfo;
              case OFXSection.TRANSACTIONS:
                  return "BANKTRANLIST";
              case OFXSection.CURRENCY:
                  return "CURDEF";
              case OFXSection.LEDGERBAL:
                  return "LEDGERBAL";
              case OFXSection.AVAILBAL:
                  return "AVAILBAL"; // 
              default:
                  throw new OFXException("Unknown section found when retrieving XPath. Section " + section);
          }
      }

      /// <summary>
      /// Returns list of all transactions in OFX document
      /// </summary>
      /// <param name="doc">OFX document</param>
      /// <returns>List of transactions found in OFX document</returns>
      private void ImportTransations(OFXStatement ofxStatement, XmlNode doc, OFXDocument ofxDocument)
      {
         var xpath = GetRelativeXPath(ofxDocument.AccType, OFXSection.TRANSACTIONS);

         ofxStatement.StatementStart = doc.GetValue(xpath + "/DTSTART").ToDate();
         ofxStatement.StatementEnd = doc.GetValue(xpath + "/DTEND").ToDate();

         var transactionNodes = doc.SelectNodes(xpath + "/STMTTRN");

         ofxStatement.Transactions = new List<Transaction>();

         foreach (XmlNode node in transactionNodes)
             ofxStatement.Transactions.Add(new Transaction(node, ofxStatement.Currency));
      }


      /// <summary>
      /// Checks account type of supplied file
      /// </summaryof
      /// <param name="file">OFX file want to check</param>
      /// <returns>Account type for account supplied in ofx file</returns>
      private AccountType GetAccountType(string file)
      {
         if (file.IndexOf("<CREDITCARDMSGSRSV1>") != -1)
            return AccountType.CC;

         if (file.IndexOf("<BANKMSGSRSV1>") != -1)
            return AccountType.BANK;

         throw new OFXException("Unsupported Account Type");
      }

      /// <summary>
      /// Check if OFX file is in SGML or XML format
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      private bool IsXmlVersion(string file)
      {
         return (file.IndexOf("OFXHEADER:100") == -1);
      }

      /// <summary>
      /// Converts SGML to XML
      /// </summary>
      /// <param name="file">OFX File (SGML Format)</param>
      /// <returns>OFX File in XML format</returns>
      private string SGMLToXML(string file)
      {
          var reader = new SgmlReader();

          object msgBody = reader.NameTable.Add("MSGBODY");
          //Inititialize SGML reader
          reader.InputStream = new StringReader(ParseHeader(file));
          reader.DocType = "OFX";

          var sw = new StringWriter();
          var writer = new XmlTextWriter(sw);
          // Root.
          writer.WriteStartDocument();

          object previousElement = null;
          Stack elementsWeAlreadyEnded = new Stack();

          while (reader.Read())
          {
              switch (reader.NodeType)
              {
                  case XmlNodeType.Element:
                      previousElement = reader.LocalName;
                      writer.WriteStartElement(reader.LocalName);
                      break;
                  case XmlNodeType.Text:
                      if (String.IsNullOrEmpty(reader.Value) == false)
                      {
                          writer.WriteString(reader.Value.Trim());
                          if (previousElement != null && !previousElement.Equals(msgBody))
                          {
                              writer.WriteEndElement();
                              elementsWeAlreadyEnded.Push(previousElement);
                          }
                      }
                      else Debug.Assert(true, "big problems?");
                      break;
                  case XmlNodeType.EndElement:
                      if (elementsWeAlreadyEnded.Count > 0
                          && Object.ReferenceEquals(elementsWeAlreadyEnded.Peek(),
                             reader.LocalName))
                      {
                          elementsWeAlreadyEnded.Pop();
                      }
                      else
                      {
                          writer.WriteEndElement();
                      }
                      break;
                  default:
                      // doing nothing as below reads in the next node as well as writing (hence it gets missed on a blank line)
                      //writer.WriteNode(reader, false);
                      break;
              }
          }

          //close xml text writer
          writer.WriteEndDocument();
          writer.Flush();
          writer.Close();
          //*/

          var temp = sw.ToString().TrimStart().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

          return String.Join("", temp);
      }

      /// <summary>
      /// Checks that the file is supported by checking the header. Removes the header.
      /// </summary>
      /// <param name="file">OFX file</param>
      /// <returns>File, without the header</returns>
      private string ParseHeader(string file)
      {
         //Select header of file and split into array
         //End of header worked out by finding first instance of '<'
         //Array split based of new line & carrige return
         var header = file.Substring(0, file.IndexOf('<'))
            .Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

         //Check that no errors in header
         CheckHeader(header);

         //Remove header
         return file.Substring(file.IndexOf('<') - 1);
      }

      /// <summary>
      /// Checks that all the elements in the header are supported
      /// </summary>
      /// <param name="header">Header of OFX file in array</param>
      private void CheckHeader(string[] header)
      {
         if (header[0] != "OFXHEADER:100")
            throw new OFXParseException("Incorrect header format");

         if (header[1] != "DATA:OFXSGML")
            throw new OFXParseException("Data type unsupported: " + header[1] + ". OFXSGML required");

         if (header[2] != "VERSION:102")
            throw new OFXParseException("OFX version unsupported. " + header[2]);

         if (header[3] != "SECURITY:NONE")
            throw new OFXParseException("OFX security unsupported");

         if (header[4] != "ENCODING:USASCII")
            throw new OFXParseException("ASCII Format unsupported:" + header[4]);

         if (header[5] != "CHARSET:1252")
            throw new OFXParseException("Charecter set unsupported:" + header[5]);

         if (header[6] != "COMPRESSION:NONE")
            throw new OFXParseException("Compression unsupported");

         if (header[7] != "OLDFILEUID:NONE")
            throw new OFXParseException("OLDFILEUID incorrect");
      }

      #region Nested type: OFXSection

      /// <summary>
      /// Section of OFX Document
      /// </summary>
      private enum OFXSection
      {
         SIGNON,
         ACCOUNTINFO,
         TRANSACTIONS,
         BALANCE,
         CURRENCY, 
         STATEMENTS,
         LEDGERBAL,
         AVAILBAL
      }

      #endregion
   }
}