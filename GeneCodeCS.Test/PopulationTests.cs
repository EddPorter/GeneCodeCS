using System.Linq;
using GeneCodeCS.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class PopulationTests
  {
    [TestMethod]
    public void SimulateGenerations_compiles_runs_and_returns_best_bots_without_writing_files() {
      var before = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), "*.cs");
      var population = new Population<CounterBot>();

      var best = population.SimulateGenerations<object>(null, generationLimit: 5, botsPerGeneration: 20, maxTreeDepth: 3);

      Assert.IsTrue(best.Count > 0);
      Assert.IsTrue(best.All(b => b.Bot != null));
      Assert.IsTrue(best.All(b => b.Bot.Fitness > int.MinValue), "Bots should run without throwing.");
      CollectionAssert.AreEquivalent(before, System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), "*.cs"));
    }

    [TestMethod]
    public void SimulateGenerations_evolves_a_good_solution_to_the_counter_problem() {
      var population = new Population<CounterBot>();

      var best = population.SimulateGenerations<object>(null, generationLimit: 30, botsPerGeneration: 60, maxTreeDepth: 3,
                                                        fitnessThreshold: 999);

      Assert.AreEqual(1000, best.First().Bot.Fitness);
    }

    [TestMethod]
    public void SimulateIndividuals_returns_a_bot_exceeding_the_threshold() {
      var population = new Population<CounterBot>();

      var bot = population.SimulateIndividuals<object>(null, maxTreeDepth: 3, fitnessThreshold: 995);

      Assert.IsTrue(bot.Bot.Fitness > 995);
    }
  }
}
