using System;
using System.Xml;

namespace OFXSharp
{
    public static class OFXHelperMethods
    {
        /// <summary>
        /// Converts string representation of AccountInfo to enum AccountInfo
        /// </summary>
        /// <param name="bankAccountType">representation of AccountInfo</param>
        /// <returns>AccountInfo</returns>
        public static BankAccountType GetBankAccountType(this string bankAccountType)
        {
            return (BankAccountType)Enum.Parse(typeof(BankAccountType), bankAccountType);
        }

        /// <summary>
        /// Flips date from YYYYMMDD to DDMMYYYY         
        /// </summary>
        /// <param name="date">Date in YYYYMMDD format</param>
        /// <returns>Date in format DDMMYYYY</returns>
        public static DateTime ToDate(this string date)
        {
            try
            {
                //Try converting to date (may possibly be my own bank that has weird date)
                return Convert.ToDateTime(date);
            }
            catch (FormatException)
            {
                //If not in format that can be parsed by .Net or me, return empty datetime
                if (date.Length < 8)
                {
                    return new DateTime();
                }

                string dd = date.Substring(6, 2);
                string mm = date.Substring(4, 2);
                string yyyy = date.Substring(0, 4);

                return Convert.ToDateTime(String.Format("{0}/{1}/{2}", dd, mm, yyyy));
            }
        }

        /// <summary>
        /// Returns value of specified node
        /// </summary>
        /// <param name="node">Node to look for specified node</param>
        /// <param name="xpath">XPath for node you want</param>
        /// <returns></returns>
        public static string GetValue(this XmlNode node, string xpath)
        {
            var tempNode = node.SelectSingleNode(xpath);
            return tempNode != null ? tempNode.FirstChild.Value : "";
        }
    }
}
