using System.ComponentModel;

namespace OFXSharp
{
    public enum AccountType
    {
        [Description("Bank Account")]
        Bank,
        [Description("Credit Card")]
        CreditCard,
        [Description("Accounts Payable")]
        AccountsPayable,
        [Description("Accounts Recievable")]
        AccountsReceivable,
        NA,
    }
}