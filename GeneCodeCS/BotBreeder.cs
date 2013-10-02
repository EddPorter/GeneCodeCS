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
using GeneCodeCS.Genetics;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class generates a population of randomised bot code, runs the bots through a set of tests, and then breeds the best bots together to create a new generation. The process repeats for a set number of generations or until a fitness threshold is reached.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from <see type="T:GeneCodeCS.BaseBot" /> . </typeparam>
  public sealed class BotBreeder<TBot> where TBot : BaseBot
  {
    // TODO: implement fitness threshold termination.

    /// <remarks>Not null.</remarks>
    private readonly BotCodeCreator<TBot> _codeCreator;

    /// <remarks>Not null.</remarks>
    private readonly BotGenerator<TBot> _generator;

    /// <remarks>Not null.</remarks>
    private readonly ILog _log;
    
    /// <remarks>Can be null.</remarks>
    private readonly Func<Chromosome, Chromosome> _optimise;

    /// <remarks>Not null.</remarks>
    private readonly BotRunner<TBot> _runner;

    public BotBreeder(ILog log, Func<Chromosome, Chromosome> optimise = null) {
      if (log == null) {
        throw new ArgumentNullException("log");
      }

      _log = log;
      _optimise = optimise;

      _runner = new BotRunner<TBot>(log);
      _codeCreator = new BotCodeCreator<TBot>(log);
      _generator = new BotGenerator<TBot>(_log);
    }

    public List<TBot> Run<T>(int generations = 20, int maxTreeDepth = 3, int population = 30, T parameters = default(T),
                             List<TBot> starterBots = null) {
      var previousGenerations = new List<List<TBot>>();
      if (starterBots != null) {
        previousGenerations.Add(starterBots);
      }

      var generator = new BotGenerator<TBot>(_log);
      var bestBots = new List<TBot>();
      for (var generation = 0; generation < generations; ++generation) {
        _log.Info(string.Format("Breeding generation {0:N}", generation));

        // 1. Generate expression trees for generation.
        var lastGeneration = previousGenerations.LastOrDefault();
        var lastGenerationReports = lastGeneration != null
                                      ? lastGeneration.Select(g => g.TerminationReport).ToList()
                                      : null;
        var thisGeneration = generator.CreateNewGeneration(generation, lastGenerationReports, population, maxTreeDepth,
                                                           _optimise);

        // 2. Convert expression trees to code.
        var bots = _codeCreator.CreateBotCode(thisGeneration, generation);

        // 3. Run code to generate fitness.
        var newGeneration =
          _runner.RunGeneration(bots, parameters).OrderByDescending(report => report.TerminationReport.Fitness).ToList();

        // 4. Evaluate bot fitness.
        var bestFitness = newGeneration.First().TerminationReport.Fitness;
        var worstFitness = newGeneration.Last().TerminationReport.Fitness;
        var averageFitness = newGeneration.Average(fp => fp.TerminationReport.Fitness);
        _log.Info(string.Format("Generation {0} report: Best {1:N} / Average {2:N2} / Worst {3:N}", generation,
                                bestFitness, averageFitness, worstFitness));

        bestBots = newGeneration.Where(b => b.TerminationReport.Fitness == bestFitness).ToList();
        _log.Info(string.Format("Best bots: {0}", string.Join(", ", bestBots.Select(b => b.TerminationReport.Bot.Name))));

        previousGenerations.Add(newGeneration);
      }

      _log.Info("Bot creation complete.");
      foreach (var bot in bestBots) {
        _log.Info(string.Format("Bot {0}: Fitness = {1}", bot.TerminationReport.Bot.Name, bot.TerminationReport.Fitness));
      }
      return bestBots;
    }

    public TBot RunSingle<T>(int maxTreeDepth = 3, T parameters = default(T), int fitnessThreshold = 0) {
      while (true) {
        // 1. Generate single bot.
        var bot = _generator.CreateRandomBot(maxTreeDepth, _optimise);
        var thisGeneration = new List<BotInformation> {bot};

        // 2. Convert expression tree to code.
        const int generation = 0;
        var bots = _codeCreator.CreateBotCode(thisGeneration, generation);

        // 3. Run code to generate fitness.
        var newBot = _runner.RunGeneration(bots, parameters).FirstOrDefault();

        Debug.Assert(newBot != null, "newBot != null");

        // 4. Evaluate bot fitness.
        _log.Info(string.Format("{0} report: {1:N}", newBot.TerminationReport.Bot.Name, newBot.TerminationReport.Fitness));
        if (newBot.TerminationReport.Fitness > fitnessThreshold) {
          return newBot;
        }
      }
    }
  }
}