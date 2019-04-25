using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using System;

namespace moneybox_unit_tests
{
    public class WithdrawMoneyUnitTest
    {
        [Test]
        public void AccountHasNoFunds_WithdrawMoney_RaiseExceptionForInsufficientFunds()
        {
            account.WithBalance(0);
            
            var raisedAssertion = Assert.Throws<InvalidOperationException>(
                () => WithdrawAmount(TenPounds));

            Assert.AreEqual("Insufficient funds to make transfer", raisedAssertion.Message, "Invalid exception message");
        }

        [Test]
        public void AccountHasLowFunds_WithdrawMoney_NotifyLowFunds()
        {
            account.WithBalance(SixPounds).WithUserEmail("jbloggs@gmail.com");

            WithdrawAmount(FivePounds);

            notificationService.Verify(ns => ns.NotifyFundsLow("jbloggs@gmail.com"), Times.Exactly(1));
        }

        [Test]
        public void AccountHasFunds_WithdrawAmount_BalanceAndWithdrawnAmountsUpdated()
        {
            account.WithBalance(TenPounds);

            WithdrawAmount(FivePounds);

            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == accountId) && a.Balance == FivePounds)), Times.Exactly(1));
            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == accountId) && a.Withdrawn == -FivePounds)), Times.Exactly(1));
        }

        private Mock<IAccountRepository> accountRepository;
        private Mock<INotificationService> notificationService;
        private WithdrawMoney withdrawMoney;
        private AccountBuilder account;
        private Guid accountId = Guid.NewGuid();
        private const decimal FivePounds = 500;
        private const decimal SixPounds = 600;
        private const decimal TenPounds = 1000;

        public void WithdrawAmount(decimal amountInPence)
        {
            withdrawMoney = new WithdrawMoney(accountRepository.Object, notificationService.Object);

            accountRepository.Setup(ar => ar.GetAccountById(accountId)).Returns(account.Build());

            withdrawMoney.Execute(accountId, amountInPence);
        }

        [SetUp]
        public void Setup()
        {
            accountRepository = new Mock<IAccountRepository>();
            notificationService = new Mock<INotificationService>();
            withdrawMoney = new WithdrawMoney(accountRepository.Object, notificationService.Object);
            account = new AccountBuilder(accountId);
        }
    }
}
