using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sgml;

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
            var accountTypes = GetAccountTypes(ofxString);

            //Load into xml document
            var doc = new XmlDocument();
            doc.Load(new StringReader(ofxString));

            var ofx = new OFXDocument { Statements = new List<Statement>() };

            //Get sign on node from OFX file
            var signOnNode = doc.SelectSingleNode(Resources.SignOn);

            //If exists, populate signon obj, else throw parse error
            if (signOnNode != null)
            {
                ofx.SignOn = new SignOn(signOnNode);
            }
            else
            {
                throw new OFXParseException("Sign On information not found");
            }

            // OFX supports multiple statements and account types in a single file, so loop through them
            foreach (var accountType in accountTypes)
            {
                // get the statement responses for this account type
                var statementNodes = doc.SelectNodes(GetXPath(accountType, OFXSection.Statement));

                if (statementNodes == null || statementNodes.Count == 0)
                {
                    throw new OFXParseException("No statement responses found for account type " + accountType);
                }

                // now we can get into the nitty gritty
                foreach (XmlNode statementNode in statementNodes)
                {
                    var statement = new Statement { AccType = accountType };

                    // Get currency info
                    var currencyNode = statementNode.SelectSingleNode(GetXPath(accountType, OFXSection.Currency));
                    if (currencyNode != null)
                    {
                        statement.Currency = currencyNode.FirstChild.Value;
                    }
                    else
                    {
                        throw new OFXParseException("Currency not found");
                    }

                    // Get account information for ofx doc
                    var accountNode = statementNode.SelectSingleNode(GetXPath(accountType, OFXSection.AccountInfo));
                    if (accountNode != null)
                    {
                        statement.Account = new Account(accountNode, accountType);
                    }
                    else
                    {
                        throw new OFXParseException("Account information not found");
                    }

                    // Get list of transactions
                    ImportTransations(statement, statementNode);

                    // Get balance info from ofx doc
                    var ledgerNode = statementNode.SelectSingleNode(".//LEDGERBAL"); //GetXPath(accountType, OFXSection.Balance) + 
                    var avaliableNode = statementNode.SelectSingleNode(".//AVAILBAL"); //GetXPath(accountType, OFXSection.Balance) +

                    // ***** OFX files from my bank don't have the 'avaliableNode' node, so i manage a 'null' situation
                    if (ledgerNode != null) // && avaliableNode != null
                    {
                        statement.Balance = new Balance(ledgerNode, avaliableNode);
                    }
                    else
                    {
                        throw new OFXParseException("Balance information not found");
                    }

                    ofx.Statements.Add(statement);
                }

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
                case AccountType.Bank:
                    xpath = Resources.BankAccount;
                    accountInfo = ".//BANKACCTFROM";
                    break;
                case AccountType.CreditCard:
                    xpath = Resources.CCAccount;
                    accountInfo = ".//CCACCTFROM";
                    break;
                default:
                    throw new OFXException("Account Type not supported. Account type " + type);
            }

            switch (section)
            {
                case OFXSection.Statement:
                    return xpath;
                case OFXSection.AccountInfo:
                    return accountInfo; //xpath + 
                case OFXSection.Balance:
                    return xpath;
                case OFXSection.Transactions:
                    return ".//BANKTRANLIST"; //xpath + 
                case OFXSection.SignOn:
                    return Resources.SignOn;
                case OFXSection.Currency:
                    return ".//CURDEF"; //xpath + 
                default:
                    throw new OFXException("Unknown section found when retrieving XPath. Section " + section);
            }
        }

        /// <summary>
        /// Returns list of all transactions in OFX document
        /// </summary>
        /// <param name="ofxStatement">OFX Statement</param>
        /// <param name="node">OFX document</param>
        /// <returns>List of transactions found in OFX document</returns>
        private void ImportTransations(Statement ofxStatement, XmlNode node)
        {
            var xpath = GetXPath(ofxStatement.AccType, OFXSection.Transactions);

            ofxStatement.StatementStart = node.GetValue(xpath + "//DTSTART").ToDate();
            ofxStatement.StatementEnd = node.GetValue(xpath + "//DTEND").ToDate();

            var transactionNodes = node.SelectNodes(xpath + "//STMTTRN");
            ofxStatement.Transactions = new List<Transaction>();

            if (transactionNodes == null || transactionNodes.Count == 0)
            {
                return;
            }

            foreach (XmlNode txNode in transactionNodes)
            {
                ofxStatement.Transactions.Add(new Transaction(txNode, ofxStatement.Currency));
            }
        }


        /// <summary>
        /// Checks account types of supplied file
        /// </summary>
        /// <param name="file">OFX file want to check</param>
        /// <returns>Account type for account supplied in ofx file</returns>
        private IEnumerable<AccountType> GetAccountTypes(string file)
        {
            var accountTypes = new List<AccountType>();
            var accountTypeFound = false;

            if (file.IndexOf("<CREDITCARDMSGSRSV1>") != -1)
            {
                accountTypes.Add(AccountType.CreditCard);
                accountTypeFound = true;
            }

            if (file.IndexOf("<BANKMSGSRSV1>") != -1)
            {
                accountTypes.Add(AccountType.Bank);
                accountTypeFound = true;
            }

            if (!accountTypeFound)
            {
                throw new OFXException("Unsupported Account Type");
            }

            return accountTypes;
        }

        /// <summary>
        /// Check if OFX file is in SGML or XML format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsXmlVersion(string file)
        {
            return (file.IndexOf("OFXHEADER:100") == -1);
        }

        /// <summary>
        /// Converts SGML to XML
        /// </summary>
        /// <param name="file">OFX File (SGML Format)</param>
        /// <returns>OFX File in XML format</returns>
        private static string SGMLToXML(string file)
        {
            var reader = new SgmlReader();

            //Inititialize SGML reader
            reader.InputStream = new StringReader(ParseHeader(file));
            reader.DocType = "OFX";

            var sw = new StringWriter();
            var xml = new XmlTextWriter(sw);

            //write output of sgml reader to xml text writer
            while (!reader.EOF)
                xml.WriteNode(reader, true);

            //close xml text writer
            xml.Flush();
            xml.Close();

            var temp = sw.ToString().TrimStart().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return String.Join("", temp);
        }

        /// <summary>
        /// Checks that the file is supported by checking the header. Removes the header.
        /// </summary>
        /// <param name="file">OFX file</param>
        /// <returns>File, without the header</returns>
        private static string ParseHeader(string file)
        {
            //Select header of file and split into array
            //End of header worked out by finding first instance of '<'
            //Array split based of new line & carrige return
            var header = file.Substring(0, file.IndexOf('<'))
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //Check that no errors in header
            CheckHeader(header);

            //Remove header
            return file.Substring(file.IndexOf('<') - 1);
        }

        /// <summary>
        /// Checks that all the elements in the header are supported
        /// </summary>
        /// <param name="header">Header of OFX file in array</param>
        private static void CheckHeader(string[] header)
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
            SignOn,
            Statement,
            AccountInfo,
            Transactions,
            Balance,
            Currency
        }

        #endregion
    }
}