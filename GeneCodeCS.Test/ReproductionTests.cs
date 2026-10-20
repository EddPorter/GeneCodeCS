using System;
using GeneCodeCS.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class ReproductionTests
  {
    [TestMethod]
    public void Rates_accept_values_from_0_to_100() {
      var target = new Reproduction<CounterBot> { CrossoverRate = 0, MutationRate = 100, RandomBotPercentage = 50 };

      Assert.AreEqual(0, target.CrossoverRate);
      Assert.AreEqual(100, target.MutationRate);
      Assert.AreEqual(50, target.RandomBotPercentage);
    }

    [TestMethod]
    public void CrossoverRate_rejects_out_of_range_values() {
      var target = new Reproduction<CounterBot>();
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.CrossoverRate = 101);
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.CrossoverRate = -1);
    }

    [TestMethod]
    public void MutationRate_rejects_out_of_range_values() {
      var target = new Reproduction<CounterBot>();
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.MutationRate = 101);
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.MutationRate = -1);
    }

    [TestMethod]
    public void RandomBotPercentage_rejects_out_of_range_values() {
      var target = new Reproduction<CounterBot>();
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.RandomBotPercentage = 101);
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => target.RandomBotPercentage = -1);
    }
  }
}
