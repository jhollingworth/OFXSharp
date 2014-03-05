using System;
using System.Xml;

namespace OFXSharp
{
    public class Account
    {
        public string AccountID { get; set; }
        public string AccountKey { get; set; }
        public AccountType AccountType { get; set; }

        #region Bank Only

        private BankAccountType _BankAccountType = BankAccountType.NA;

        public string BankID { get; set; }

        public string BranchID { get; set; }


        public BankAccountType BankAccountType
        {
            get
            {
                if (AccountType == AccountType.BANK)
                    return _BankAccountType;
                
                return BankAccountType.NA;
            }
            set 
            {
                _BankAccountType = AccountType == AccountType.BANK ? value : BankAccountType.NA;
            }
        }

        #endregion

        public Account(XmlNode node, AccountType type)
        {
            AccountType = type;

            AccountID = node.GetValue("//ACCTID");
            AccountKey = node.GetValue("//ACCTKEY");

            switch (AccountType)
            {
                case AccountType.BANK:
                    InitializeBank(node);
                    break;
                case AccountType.AP:
                    InitializeAP(node);
                    break;
                case AccountType.AR:
                    InitializeAR(node);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes information specific to bank
        /// </summary>
        private void InitializeBank(XmlNode node)
        {
            BankID = node.GetValue("//BANKID");
            BranchID = node.GetValue("//BRANCHID");

            //Get Bank Account Type from XML
            string bankAccountType = node.GetValue("//ACCTTYPE");

            //Check that it has been set
            if (String.IsNullOrEmpty(bankAccountType))
                throw new OFXParseException("Bank Account type unknown");

            //Set bank account enum
            _BankAccountType = bankAccountType.GetBankAccountType();
        }

        #region Account types not supported

        private void InitializeAP(XmlNode node)
        {
            throw new OFXParseException("AP Account type not supported");
        }

        private void InitializeAR(XmlNode node)
        {
            throw new OFXParseException("AR Account type not supported");
        }

        #endregion
    }
}