using System;
using System.Collections.Generic;
using System.Text;
using Moneybox.App;
using Moneybox.App.DataAccess;
using Moq;

namespace moneybox_unit_tests
{
    public class AccountBuilder
    {
        private Account account = new Account();

        public AccountBuilder(Guid id)
        {
            account.Id = id;
            account.User = new User();
        }

        public AccountBuilder WithWithdrawn(decimal withdrawn)
        {
            account.Withdrawn = withdrawn;
            return this;
        }


        public AccountBuilder WithBalance(decimal balance)
        {
            account.Balance = balance;
            return this;
        }

        public AccountBuilder WithPaidInAmount(decimal paidIn)
        {
            account.PaidIn = paidIn;
            return this;
        }

        public AccountBuilder WithUserEmail(string email)
        {
            account.User.Email = email;
            return this;
        }

        public Account Build()
        {
            return account;
        }
    }
}
