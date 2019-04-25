using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using System;

namespace moneybox_unit_tests
{
    public class TransferMoneyUnitTest
    {
        [Test]
        public void PayerHasNoFunds_TransferMoney_RaiseExceptionForInsufficientFunds()
        {
            payerAccount.WithBalance(0);

            var raisedAssertion = Assert.Throws<InvalidOperationException>(
                () => TransferAmount(TenPounds));

            Assert.AreEqual("Insufficient funds to make transfer", raisedAssertion.Message, "Invalid exception message");
        }

        [Test]
        public void PayerHasLowFunds_TransferMoney_NotifyLowFunds()
        {
            payerAccount.WithBalance(SixPounds).WithUserEmail("jbloggs@gmail.com");

            TransferAmount(FivePounds);

            notificationService.Verify(ns => ns.NotifyFundsLow("jbloggs@gmail.com"), Times.Exactly(1));
        }

        [Test]
        public void PayeeNearPaidInLimit_TransferMoneyGoingOverLimit_RaiseExceptionForPayInLimitReached()
        {
            payeeAccount.WithPaidInAmount(ThirtyNinePounds);
            payerAccount.WithBalance(TenPounds);

            var raisedAssertion = Assert.Throws<InvalidOperationException>(
                () => TransferAmount(FivePounds));

            Assert.AreEqual("Account pay in limit reached", raisedAssertion.Message, "Invalid exception message");
        }

        [Test]
        public void PayeeNothingPaidIn_TransferAmountNearPaidInLimit_NotifyApproachingPaidInLimit()
        {
            payeeAccount.WithPaidInAmount(0).WithUserEmail("jbloggs@gmail.com");
            payerAccount.WithBalance(ThirtyNinePounds);

            TransferAmount(ThirtyNinePounds);

            notificationService.Verify(ns => ns.NotifyApproachingPayInLimit("jbloggs@gmail.com"), Times.Exactly(1));
        }

        [Test]
        public void PayerHasFundsAndPayeeNothingPaidIn_TransferAmount_PayeeBalanceAndPaidInAmountsUpdated()
        {
            payerAccount.WithBalance(TenPounds);

            TransferAmount(FivePounds);

            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == payeeId) && a.Balance == FivePounds)), Times.Exactly(1));
            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == payeeId) && a.PaidIn == FivePounds)), Times.Exactly(1));
        }

        [Test]
        public void PayerHasFundsAndPayeeNothingPaidIn_TransferAmount_PayerBalanceAndWithdrawnAmountsUpdated()
        {
            payerAccount.WithBalance(TenPounds);

            TransferAmount(FivePounds);

            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == payerId) && a.Balance == FivePounds)), Times.Exactly(1));
            accountRepository.Verify(
                ar => ar.Update(It.Is<Account>(a => (a.Id == payerId) && a.Withdrawn == -FivePounds)), Times.Exactly(1));
        }

        private Mock<IAccountRepository> accountRepository;
        private Mock<INotificationService> notificationService;
        private TransferMoney transferMoney;
        private AccountBuilder payerAccount;
        private AccountBuilder payeeAccount;
        private Guid payerId = Guid.NewGuid();
        private Guid payeeId = Guid.NewGuid();
        private const decimal FivePounds = 500;
        private const decimal SixPounds = 600;
        private const decimal TenPounds = 1000;
        private const decimal ThirtyNinePounds = 3900;

        public void TransferAmount(decimal amountInPence)
        {
            transferMoney = new TransferMoney(accountRepository.Object, notificationService.Object);

            accountRepository.Setup(ar => ar.GetAccountById(payerId)).Returns(payerAccount.Build());
            accountRepository.Setup(ar => ar.GetAccountById(payeeId)).Returns(payeeAccount.Build());

            transferMoney.Execute(payerId, payeeId, amountInPence);
        }

        [SetUp]
        public void Setup()
        {
            accountRepository = new Mock<IAccountRepository>();
            notificationService = new Mock<INotificationService>();
            transferMoney = new TransferMoney(accountRepository.Object, notificationService.Object);
            payeeAccount = new AccountBuilder(payeeId);
            payerAccount = new AccountBuilder(payerId);
        }
    }
}

