using System;
using NUnit.Framework;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Grpc.Models;

namespace Service.Sendgrid.Profile.Tests
{
    public class TestExample
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {

            var t1 = new DateTime(2020, 3, 31);
            var t = t1.AddMonths(1);
            
            
            var trueProfile = new KycStatusResponse()
            {
                TradeStatus = KycOperationStatus.Allowed,
                DepositStatus = KycOperationStatus.AllowedWithKycAlert,
                WithdrawalStatus = KycOperationStatus.Allowed
            };
            var falseProfile = new KycStatusResponse()
            {
                TradeStatus = KycOperationStatus.Blocked,
                DepositStatus = KycOperationStatus.AllowedWithKycAlert,
                WithdrawalStatus = KycOperationStatus.Allowed
            };
            var progProfile = new KycStatusResponse()
            {
                VerificationInProgress = true,
                TradeStatus = KycOperationStatus.Blocked,
                DepositStatus = KycOperationStatus.AllowedWithKycAlert,
                WithdrawalStatus = KycOperationStatus.Allowed
            };
            Assert.AreEqual("false", GetKycStatus(falseProfile));
            Assert.AreEqual("prog", GetKycStatus(progProfile));
            Assert.AreEqual("true", GetKycStatus(trueProfile));


            string GetKycStatus(KycStatusResponse response)
            {
                if (response.VerificationInProgress)
                    return "prog";
                if (response.DepositStatus == KycOperationStatus.Allowed || response.DepositStatus == KycOperationStatus.AllowedWithKycAlert &&
                    response.TradeStatus == KycOperationStatus.Allowed || response.TradeStatus == KycOperationStatus.AllowedWithKycAlert &&
                    response.WithdrawalStatus == KycOperationStatus.Allowed || response.WithdrawalStatus == KycOperationStatus.AllowedWithKycAlert)
                    return "true";

                return "false";
            }
            Console.WriteLine("Debug output");
            Assert.Pass();
        }
    }
}
