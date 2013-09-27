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
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class represents a population of bots, initially created using randomised tree-generation using the public methods of the underlying <typeparamref
  ///    name="TBot" /> type. The randomised bots are compiled, executed, ranked for fitness, and then bred to create new generations.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see type="T:GeneCodeCS.BaseBot" /> . Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="T:GeneCodeCS.Genetics.IParameter`1" /> . </typeparam>
  public sealed class Population<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to convert the Chromosomes into C# code and then compile them to an assembly.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly Compilation<TBot> _compiler;

    /// <summary>
    ///   Used to execute a compiled bot to generate its fitness score.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly Execution<TBot> _executor;

    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly ILog _log;

    /// <summary>
    ///   An optional method used to optimise a chromosome's tree.
    /// </summary>
    /// <remarks>Can be null.</remarks>
    private readonly Reproduction<TBot>.ChromosomeOptimiser _optimise;

    /// <summary>
    ///   Used to create new bot generations by randomly generating bots of by breeding bots from a previous generation together.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly Reproduction<TBot> _reproducer;

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.Population`1" /> class.
    /// </summary>
    /// <param name="log"> An instance of a <see cref="T:Common.Logging.ILog" /> interface. This is used to log the status of the population during simulation. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    public Population(ILog log, Reproduction<TBot>.ChromosomeOptimiser optimise = null) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Population`1.ctor: Constructing class.");

      _log = log;
      _optimise = optimise;

      _reproducer = new Reproduction<TBot>(log);
      _compiler = new Compilation<TBot>(log);
      _executor = new Execution<TBot>(log);
    }

    /// <summary>
    ///   Simluates a number of generations of bots. At each generation the bots from the previous generation are bred with each other (with occasional mutations). The chance of a bot becoming a parent is directly based on their calculated fitness. The process continues until a given number of generations are produced or a given fitness level is reached.
    /// </summary>
    /// <remarks>
    ///   Public API. The initial generation is made of randomly generated bots. A list of starter bots may be provided to seed this generation. Any shortcomings in the population is made up with additional randomly created bots.
    /// </remarks>
    /// <param name="generationLimit"> The maximum number of generations to simulate. </param>
    /// <param name="botsPerGeneration"> The number of bots to create or breed in each generation. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation in the initial generation. </param>
    /// <param name="parameters"> Any parameters to pass to the Initialise method of each bot during the execution phase. </param>
    /// <param name="fitnessThreshold"> The fitness threshold that, if exceeded, will cause simulation to end at the current generation. </param>
    /// <param name="starterBots"> A list of bots to use in the first generation. Any shortfall in the initial generation will be filled with randomly created bots. </param>
    /// <returns> The best bot(s) from the final generation is/are returned. </returns>
    // TODO: Expose mutation rate property in Reproduction class.
    public List<TBot> SimulateGenerations<T>(int generationLimit = 20, int botsPerGeneration = 30, int maxTreeDepth = 3,
                                             T parameters = default(T), int fitnessThreshold = int.MaxValue,
                                             List<TBot> starterBots = null) {
      _log.InfoFormat("Simulating population with {0} generations, {1} bots per generation, and tree depth of {2}.",
                      generationLimit, botsPerGeneration, maxTreeDepth);
      if (fitnessThreshold != int.MaxValue) {
        _log.InfoFormat("A limiting threshold of {0} has been set.", fitnessThreshold);
      }

      var generationHistory = new List<List<TBot>>();

      if (starterBots != null) {
        _log.InfoFormat("Pre-seeding initial generation with {0} bots.", starterBots.Count);
        generationHistory.Add(starterBots);
      }
      var latestEvaluation = generationHistory.LastOrDefault();

      var bestBots = new List<TBot>();
      for (var generationNumber = 0; generationNumber < generationLimit; ++generationNumber) {
        _log.InfoFormat("Starting generation {0:N}", generationNumber);

        // 1. Generate expression trees for generation.
        _log.Trace("GeneCodeCS.Population`1.SimulateGenerations`1: Breeding generation.");
        var latestFitnessReports = latestEvaluation != null
                                     ? latestEvaluation.Select(b => b.FitnessReport).ToList()
                                     : null;
        var generationInformation = _reproducer.BreedGeneration(generationNumber, latestFitnessReports,
                                                                botsPerGeneration, maxTreeDepth, _optimise);

        // 2. Convert expression trees to code.
        _log.Trace("GeneCodeCS.Population`1.SimulateGenerations`1: Compiling bot code.");
        var compiledBots = _compiler.CompileBots(generationInformation, generationNumber);

        // 3. Run code to generate fitness.
        _log.Trace("GeneCodeCS.Population`1.SimulateGenerations`1: Executing bots.");
        var evaluatedBots = _executor.Run(compiledBots, parameters);
        var orderedEvaluation = evaluatedBots.OrderByDescending(report => report.FitnessReport.Fitness).ToList();

        // 4. Evaluate bot fitness - we use First() and Last() since the list is already sorted.
        var bestFitness = orderedEvaluation.First().FitnessReport.Fitness;
        var worstFitness = orderedEvaluation.Last().FitnessReport.Fitness;
        var meanFitness = orderedEvaluation.Average(fp => fp.FitnessReport.Fitness);

        _log.InfoFormat("Generation {0} report: Best {1:N} / Mean average {2:N2} / Worst {3:N}", generationNumber,
                        bestFitness, meanFitness, worstFitness);

        bestBots = orderedEvaluation.TakeWhile(b => b.FitnessReport.Fitness == bestFitness).ToList();
        _log.InfoFormat("Best bots: '{0}'", string.Join("', '", bestBots.Select(b => b.FitnessReport.Bot.Name)));

        generationHistory.Add(orderedEvaluation);
        latestEvaluation = orderedEvaluation;

        if (bestFitness > fitnessThreshold) {
          _log.InfoFormat("Exceeded threshold limit ({0}) with {1}.", fitnessThreshold, bestFitness);
          break;
        }
      }

      _log.Info("Bot creation complete.");
      foreach (var bot in bestBots) {
        _log.InfoFormat("Bot '{0}': Fitness = {1:N}", bot.FitnessReport.Bot.Name, bot.FitnessReport.Fitness);
      }
      return bestBots;
    }

    /// <summary>
    ///   Simulates the creation of individual random bots until a given fitness level is reached. No breeding is performed.
    /// </summary>
    /// <remarks>Public API.</remarks>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation. </param>
    /// <param name="parameters"> Any parameters to pass to the Initialise method of a bot during the execution phase. </param>
    /// <param name="fitnessThreshold"> The fitness threshold that, if exceeded, will cause simulation to end. </param>
    /// <returns> The first bot to exceed the fitness threshold. </returns>
    public TBot SimulateIndividuals<T>(int maxTreeDepth = 3, T parameters = default(T), int fitnessThreshold = 0) {
      _log.InfoFormat("Simulating indivudual bots with tree depth of {0} and a limiting threshold of {1}.", maxTreeDepth,
                      fitnessThreshold);

      while (true) {
        // 1. Generate single bot.
        _log.Trace("GeneCodeCS.Population`1.SimulateIndividuals`1: Breeding single bot.");
        var botInformation = _reproducer.CreateBot(maxTreeDepth, _optimise);
        var generationInformation = new List<BotInformation> { botInformation };

        // 2. Convert expression tree to code.
        _log.Trace("GeneCodeCS.Population`1.SimulateIndividuals`1: Compiling bot code.");
        const int generationNumber = 0;
        var compiledBots = _compiler.CompileBots(generationInformation, generationNumber);

        // 3. Run code to generate fitness.
        _log.Trace("GeneCodeCS.Population`1.SimulateIndividuals`1: Executing bot.");
        var evaluatedBot = _executor.Run(compiledBots, parameters).First();

        Debug.Assert(evaluatedBot != null, "newBot != null");

        // 4. Evaluate bot fitness.
        var fitness = evaluatedBot.FitnessReport.Fitness;
        _log.InfoFormat("Bot '{0}': Fitness = {1:N}", evaluatedBot.FitnessReport.Bot.Name, fitness);
        if (fitness > fitnessThreshold) {
          _log.InfoFormat("Exceeded threshold limit ({0}) with {1}.", fitnessThreshold, fitness);
          return evaluatedBot;
        }
      }
    }
  }
}