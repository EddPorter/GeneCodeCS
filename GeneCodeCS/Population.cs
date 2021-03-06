﻿//
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
using System.Linq;
using Common.Logging;
using GeneCodeCS.Genetics;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class represents a population of bots, initially created using randomised tree-generation using the public methods of the underlying <typeparamref
  ///    name="TBot" /> type. The randomised bots are compiled, executed, ranked for fitness (with a user-provided function), and then bred to create new generations.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see cref="BaseBot" /> . Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="IParameter{T}" /> . See the <see cref="BaseBot" /> documentation for restrictions and requirements of implementing this class type. </typeparam>
  public sealed class Population<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to convert the Chromosomes into C# code and then compile them to an assembly.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly Compilation<TBot> _compiler;

    /// <summary>
    ///   Used to execute a compiled bot to generate its fitness score.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly Execution<TBot> _executor;

    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///   An optional method that optimises a given <see cref="Chromosome" /> (gene expression tree). It removes redundant code and consolidates statements. The purpose is to reduce the time taken to evaluate a bot's fitness and to reduce the likelihood that identical bots are created. The resulting <see
    ///    cref="Chromosome" /> can reuse objects from the original tree, but no circular references must have been introduced.
    /// </summary>
    /// <remarks>
    ///   Can be null.
    /// </remarks>
    private readonly Func<Chromosome, Chromosome> _optimise;

    /// <summary>
    ///   Used to create new bot generations by randomly generating bots of by breeding bots from a previous generation together.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly Reproduction<TBot> _reproducer;

    /// <summary>
    ///   Initialises a new instance of the <see cref="Population{TBot}" /> class.
    /// </summary>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    public Population(Func<Chromosome, Chromosome> optimise = null) {
      _log.Trace("ctor: Constructing class.");

      _optimise = optimise;

      _reproducer = new Reproduction<TBot>();
      _compiler = new Compilation<TBot>();
      _executor = new Execution<TBot>();
    }

    /// <summary>
    ///   Simluates a number of generations of bots. At each generation the bots from the previous generation are bred with each other (with occasional mutations). The chance of a bot becoming a parent is directly based on their calculated fitness. The process continues until a given number of generations are produced or a given fitness level is reached.
    /// </summary>
    /// <remarks>
    ///   The initial generation is made of randomly generated bots. A list of starter bots may be provided to seed this generation. Any shortcomings in the population is made up with additional randomly created bots.
    /// </remarks>
    /// <param name="parameters"> Any parameters to pass to the Initialise method of each bot during the execution phase. </param>
    /// <param name="generationLimit"> The maximum number of generations to simulate. </param>
    /// <param name="botsPerGeneration"> The number of bots to create or breed in each generation. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation in the initial generation. </param>
    /// <param name="fitnessThreshold"> The fitness threshold that, if exceeded, will cause simulation to end at the current generation. </param>
    /// <param name="starterBots"> A list of bot information to use in the first generation. Any shortfall in the initial generation after incorporating these bots will be filled with randomly created bots. </param>
    /// <returns> The best bot(s) from the final generation is/are returned. </returns>
    public List<BotInformation<TBot>> SimulateGenerations<T>(T parameters = default(T), int generationLimit = 20,
                                                             int botsPerGeneration = 30, int maxTreeDepth = 3,
                                                             int fitnessThreshold = int.MaxValue,
                                                             List<BotInformation<TBot>> starterBots = null) {
      _log.InfoFormat("Simulating population with {0} generations, {1} bots per generation, and tree depth of {2}.",
                      generationLimit, botsPerGeneration, maxTreeDepth);
      if (fitnessThreshold != int.MaxValue) {
        _log.InfoFormat("A limiting threshold of {0} has been set.", fitnessThreshold);
      }

      var generationHistory = new List<List<BotInformation<TBot>>>();
      List<BotInformation<TBot>> latestOrderedEvaluation = null;

      if (starterBots != null) {
        _log.InfoFormat("Pre-seeding initial generation with {0} bots.", starterBots.Count);
        latestOrderedEvaluation = starterBots.OrderByDescending(b => b.Bot.Fitness).ToList();
      }

      var simulationStart = DateTime.Now;
      var bestBots = new List<BotInformation<TBot>>();
      for (var generationNumber = 0; generationNumber < generationLimit; ++generationNumber) {
        _log.InfoFormat("Starting generation {0}", generationNumber);
        var generationStart = DateTime.Now;

        // 1. Generate expression trees for generation.
        _log.Trace("SimulateGenerations`1: Breeding generation.");
        var generationInformation = _reproducer.BreedGeneration(generationNumber, botsPerGeneration, maxTreeDepth,
                                                                latestOrderedEvaluation, _optimise);

        // 2. Convert expression trees to code.
        _log.Trace("SimulateGenerations`1: Compiling bot code.");
        if (!_compiler.CompileBots(generationInformation, generationNumber)) {
          throw new BotCompileException();
        }

        // 3. Run code to generate fitness.
        _log.Trace("SimulateGenerations`1: Executing bots.");
        _executor.Run(generationInformation, parameters);

        // 4. Evaluate bot fitness.
        _log.Trace("SimulateGenerations`1: Evaluating bot fitness.");
        latestOrderedEvaluation = generationInformation.OrderByDescending(report => report.Bot.Fitness).ToList();

        var generationEnd = DateTime.Now;

        // We use First() and Last() since the list is sorted.
        var bestFitness = latestOrderedEvaluation.First().Bot.Fitness;
        if (_log.IsInfoEnabled) {
          var worstFitness = latestOrderedEvaluation.Last().Bot.Fitness;
          var meanFitness = latestOrderedEvaluation.Average(fp => fp.Bot.Fitness);

          _log.InfoFormat("Generation {0} report: Best {1:N} / Mean average {2:N2} / Worst {3:N}. Time taken: {4:T}.",
                          generationNumber, bestFitness, meanFitness, worstFitness, generationEnd - generationStart);

          bestBots = latestOrderedEvaluation.TakeWhile(b => b.Bot.Fitness == bestFitness).ToList();
          _log.InfoFormat("Best bots: '{0}'", string.Join("', '", bestBots.Select(b => b.Name)));
        }

        generationHistory.Add(latestOrderedEvaluation);

        if (bestFitness > fitnessThreshold) {
          _log.InfoFormat("Exceeded threshold limit ({0}) with {1}.", fitnessThreshold, bestFitness);
          break;
        }
      }

      var simulationEnd = DateTime.Now;

      _log.InfoFormat("Bot creation complete. Time taken: {0:T}.", simulationEnd - simulationStart);
      foreach (var bot in bestBots) {
        _log.InfoFormat("Bot '{0}': Fitness = {1:N}", bot.Name, bot.Bot.Fitness);
      }
      return bestBots;
    }

    /// <summary>
    ///   Simulates the creation of individual random bots until a given fitness level is reached. No breeding is performed.
    /// </summary>
    /// <param name="parameters"> Any parameters to pass to the Initialise method of a bot during the execution phase. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation. </param>
    /// <param name="fitnessThreshold"> The fitness threshold that, if exceeded, will cause simulation to end. </param>
    /// <returns> The first bot to exceed the fitness threshold. </returns>
    public BotInformation<TBot> SimulateIndividuals<T>(T parameters = default(T), int maxTreeDepth = 3,
                                                       int fitnessThreshold = 0) {
      _log.InfoFormat("Simulating individual bots with tree depth of {0} and a limiting threshold of {1}.", maxTreeDepth,
                      fitnessThreshold);

      while (true) {
        // 1. Generate single bot.
        _log.Trace("SimulateIndividuals`1: Breeding single bot.");
        var botInformation = _reproducer.CreateBot(maxTreeDepth, _optimise);

        // 2. Convert expression tree to code.
        _log.Trace("SimulateIndividuals`1: Compiling bot code.");
        const int generationNumber = 0;
        if (!_compiler.CompileBot(botInformation, generationNumber)) {
          throw new BotCompileException();
        }

        // 3. Run code to generate fitness.
        _log.Trace("SimulateIndividuals`1: Executing bot.");
        _executor.Run(botInformation, parameters);

        // 4. Evaluate bot fitness.
        _log.InfoFormat("Bot '{0}': Fitness = {1:N}", botInformation.Name, botInformation.Bot.Fitness);
        if (botInformation.Bot.Fitness > fitnessThreshold) {
          _log.InfoFormat("Exceeded threshold limit ({0}) with {1}.", fitnessThreshold, botInformation.Bot.Fitness);
          return botInformation;
        }
      }
    }
  }
}