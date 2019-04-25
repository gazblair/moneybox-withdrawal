using System;
using Moneybox.App.Domain.Services;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal FivePound = 500m;
        public Account(Guid id,
            User user,
            decimal balance,
            decimal withdrawn,
            decimal paidIn)
        {
            Id = id;
            User = user;
            Balance = balance;
            Withdrawn = withdrawn;
            PaidIn = paidIn;
        }

        public Guid Id { get; private set; }

        public User User { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }

        public void PayIn(decimal amountInPence, INotificationService notificationService)
        {
            var newPaidIn = PaidIn + amountInPence;
            if (newPaidIn > Account.PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (Account.PayInLimit - newPaidIn < FivePound)
            {
                notificationService.NotifyApproachingPayInLimit(User.Email);
            }

            Balance = Balance + amountInPence;
            PaidIn = newPaidIn;
        }

        public void Withdraw(decimal amountInPence, INotificationService notificationService)
        {
            var newBalance = Balance - amountInPence;
            if (newBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (newBalance < FivePound)
            {
                notificationService.NotifyFundsLow(User.Email);
            }

            Balance = newBalance;
            Withdrawn = Withdrawn - amountInPence;
        }
    }
}
