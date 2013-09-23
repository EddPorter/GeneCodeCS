// 
// GeneCodeCS - Genetic programming library for code bot natural selection.
// Copyright (C) 2013 Edd Porter <genecodecs@eddporter.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see {http://www.gnu.org/licenses/}.
//  

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Logging;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class generates a population of randomised bot code, runs the bots through a set of tests, and then breeds the best bots together to create a new generation. The process repeats for a set number of generations or until a fitness threshold is reached.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  public sealed class BotBreeder<TBot> where TBot : BaseBot
  {
    // TODO: implement fitness threshold termination.

    private readonly BotCodeCreator<TBot> _codeCreator;
    private readonly ILog _log;
    private readonly Func<ExpressionTree, ExpressionTree> _optimise;
    private readonly BotRunner<TBot> _runner;
    private int _generations;
    private int _maxTreeDepth;
    private int _population;

    public BotBreeder(ILog log, Func<ExpressionTree, ExpressionTree> optimise = null) {
      _log = log;
      _optimise = optimise;
      _runner = new BotRunner<TBot>(log);
      _codeCreator = new BotCodeCreator<TBot>(log);
    }

    public BotReport CreateRandomBot(int maxTreeDepth) {
      var generator = new BotGenerator<TBot>(_log);
      return generator.CreateRandomBot(maxTreeDepth, _optimise);
    }

    public List<BotReport> Run<T>(int generations = 20, int maxTreeDepth = 3, int population = 30,
                                  T parameters = default(T), List<BotReport> starterBots = null) {
      _generations = generations;
      _maxTreeDepth = maxTreeDepth;
      _population = population;

      var generator = new BotGenerator<TBot>(_log);
      var previousGenerations = new List<List<BotReport>>();
      if (starterBots != null) {
        previousGenerations.Add(starterBots);
      }

      var bestBots = new List<BotReport>();
      for (var generation = 0; generation < _generations; ++generation) {
        _log.Info(string.Format("Breeding generation {0:N}", generation));
        // 1. Generate expression trees for generation.

        var thisGeneration = generator.CreateNewGeneration(generation, previousGenerations.LastOrDefault(), _population,
                                                           _maxTreeDepth, _optimise);
        // 2. Convert expression trees to code.
        var bots = _codeCreator.CreateBotCode(generation, thisGeneration);
        // 3. Run code to generate fitness.
        var newGeneration = _runner.RunGeneration(bots, parameters, thisGeneration);

        var bestFitness = newGeneration.First().Fitness;
        bestBots = newGeneration.Where(b => b.Fitness == bestFitness).ToList();
        var worstFitness = newGeneration.Last().Fitness;
        var averageFitness = newGeneration.Average(fp => fp.Fitness);
        _log.Info(string.Format("Generation {0} report: Best {1:N} / Average {2:N1} / Worst {3:N}", generation,
                                bestFitness, averageFitness, worstFitness));
        _log.Info(string.Format("Best bots: {0}", string.Join(", ", bestBots.Select(b => b.Name))));

        previousGenerations.Add(newGeneration);
      }

      _log.Info("Bot creation complete.");
      foreach (var bot in bestBots) {
        _log.Info(string.Format("Bot {0}. Fitness {1} (Profit: £{2:N2})", bot.Name, bot.Fitness, bot.Information));
      }
      return bestBots;
    }

    public BotReport RunSingle<T>(int maxTreeDepth = 3, T parameters = default(T)) {
      _maxTreeDepth = maxTreeDepth;

      var generator = new BotGenerator<TBot>(_log);
      while (true) {
        var bot = generator.CreateRandomBot(_maxTreeDepth, _optimise);
        const int generation = 0;
        var thisGeneration = new List<BotReport> { bot };
        var bots = _codeCreator.CreateBotCode(generation, thisGeneration);
        var newBot = _runner.RunGeneration(bots, parameters, thisGeneration).FirstOrDefault();

        Debug.Assert(newBot != null, "newBot != null");

        _log.Info(string.Format("{0} report: {1:N}", newBot.Name, newBot.Information));
        if (newBot.Fitness > 0) {
          return bot;
        }
      }
    }
  }
}