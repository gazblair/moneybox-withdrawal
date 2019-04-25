using Moneybox.App;
using System;

namespace moneybox_unit_tests
{
    public class AccountBuilder
    {
        private Guid id;
        private User user = new User();
        private decimal balance;
        private decimal paidIn;

        public AccountBuilder(Guid id)
        {
            this.id = id;
        }

        public AccountBuilder WithPaidInAmount(decimal paidIn)
        {
            this.paidIn = paidIn;
            return this;
        }

        public AccountBuilder WithBalance(decimal paidIn)
        {
            this.balance = paidIn;
            return this;
        }

        public AccountBuilder WithUserEmail(string email)
        {
            user.Email = email;
            return this;
        }

        public Account Build()
        {
            return new Account(id, user, balance, 0, paidIn);
        }
    }
}
