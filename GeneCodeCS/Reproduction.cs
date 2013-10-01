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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Common.Logging;
using GeneCodeCS.Genetics;
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class generates new bots using random generation and fitness-based breeding. The provided <typeparamref
  ///    name="TBot" /> type parameter determines the genes used to express each new bots behaviour.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see type="T:GeneCodeCS.BaseBot" /> . Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="T:GeneCodeCS.Genetics.IParameter`1" /> . </typeparam>
  /// <remarks>
  ///   This class makes use of the <see cref="T:System.Random" /> class and so should not be considered cryptographically secure.
  /// </remarks>
  public sealed class Reproduction<TBot> where TBot : BaseBot
  {
    #region Delegates

    /// <summary>
    ///   A method that optimises a given <see cref="T:GeneCodeCS.Genetics.Chromosome" /> (gene expression tree). It removes redundant code and consolidates statements. The purpose is to reduce the time taken to evaluate a bot's fitness and to reduce the likelihood that identical bots are created.
    /// </summary>
    /// <param name="c"> The <see cref="T:GeneCodeCS.Genetics.Chromosome" /> to optimise. </param>
    /// <returns> An optimised <see cref="T:GeneCodeCS.Genetics.Chromosome" /> . This can reuse objects from the original tree, but no circular references must have been introduced. </returns>
    public delegate Chromosome ChromosomeOptimiser(Chromosome c);

    #endregion

    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log;

    /// <summary>
    ///   A list of non-terminal genes, formed from the branch methods found in the <typeparamref name="TBot" /> type and constructed sequence genes.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly List<IGene> _nonTerminals = new List<IGene>();

    /// <summary>
    ///   A list of the terminal methods found in the <typeparamref name="TBot" /> type.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly List<TerminalGene> _terminals = new List<TerminalGene>();

    /// <summary>
    ///   The percentage of the best bots to copy from one generation to the next without modification (0-100). Defaults to 10.
    /// </summary>
    private int _elitePercentage = 10;

    /// <summary>
    ///   The mutation rate (chance) to use during breeding (0-100). Defaults to 5.
    /// </summary>
    private int _mutationRate = 5;

    /// <summary>
    ///   A class for generating random numbers.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private Random _random;

    /// <summary>
    ///   The percentage of the bots to generate randomly in each generation after first (0-100). Defaults to 20.
    /// </summary>
    private int _randomBotPercentage = 20;

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.Reproduction`1" /> class. The provided <typeparamref
    ///    name="TBot" /> type is inspected and the qualifying terminal and non-terminal methods are extracted and stored.
    /// </summary>
    /// <param name="log"> An instance of an <see cref="T:Common.Logging.ILog" /> interface. This is used to log the status of the population during simulation. </param>
    /// <param name="randomSeed"> A value to use to seed the randomiser used within the class. This allows for reproducibility if required. If 0, then the current time is used to seed the randomiser. </param>
    public Reproduction(ILog log, int randomSeed = 0) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Reproduction`1.ctor: Constructing class.");

      _log = log;

      Initialise(randomSeed);
    }

    /// <summary>
    ///   Gets or sets the percentage of the best bots to copy from one generation to the next without modification (0-100). Defaults to 10.
    /// </summary>
    public int ElitePercentage {
      get { return _elitePercentage; }
      set {
        if (_elitePercentage < 0 || _elitePercentage > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.ElitePercentageValidRange);
        }
        _log.TraceFormat("GeneCodeCS.Reproduction`1.get_ElitePercentage: Elite percentage set to {0}.", value);
        _elitePercentage = value;
      }
    }

    /// <summary>
    ///   Gets or sets the mutation rate (chance) to use during breeding (0-100). Defaults to 5.
    /// </summary>
    public int MutationRate {
      get { return _mutationRate; }
      set {
        if (_mutationRate < 0 || _mutationRate > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.MutationRateValidRange);
        }
        _log.TraceFormat("GeneCodeCS.Reproduction`1.get_MutationRate: Mutation rate set to {0}.", value);
        _mutationRate = value;
      }
    }

    /// <summary>
    ///   Gets or sets the percentage of the bots to generate randomly in each generation after first (0-100). Defaults to 20.
    /// </summary>
    public int RandomBotPercentage {
      get { return _randomBotPercentage; }
      set {
        if (_randomBotPercentage < 0 || _randomBotPercentage > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.RandomBotPercentageValidRange);
        }
        _log.TraceFormat("GeneCodeCS.Reproduction`1.get_RandomBotPercentage: Random bot percentage set to {0}.", value);
        _randomBotPercentage = value;
      }
    }

    /// <summary>
    ///   Breeds a generation of <paramref name="botsPerGeneration" /> bots. If the <paramref name="generationNumber" /> is 0 then copies any bots provided in <paramref
    ///    name="previousGenerationReports" /> and creates the remaining number randomly. If this is not the first generation, the generation is formed from the top 10% of bots from the last generation, 10% of randomly created bots. The remaining 80% are created by breeding (swapping chromosome expression trees at random points and introducing mutations) the previous generation. This is done such bots from the previous generation are more likely to be parents the greater their fitness.
    /// </summary>
    /// <param name="generationNumber"> The number of the generation to breed. Used for creating the bot class names. If the value is 0 then the <paramref
    ///    name="previousGenerationReports" /> are copied and the remainder of the generation is created randomly. </param>
    /// <param name="previousGenerationReports"> The report for the previous generation or null if this is the first generation. Can also be used to pre-seed (fully or partially) the first generation if <paramref
    ///    name="generationNumber" /> is 0. </param>
    /// <param name="botsPerGeneration"> The number of bots to create or breed in each generation. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation in the initial generation. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    /// <returns> </returns>
    public List<BotInformation> BreedGeneration(int generationNumber, List<BotReport> previousGenerationReports,
                                                int botsPerGeneration, int maxTreeDepth,
                                                ChromosomeOptimiser optimise = null) {
      _log.TraceFormat(
        "GeneCodeCS.Reproduction`1.BreedGeneration: Breeding generation {0} with {1} bots per generation and tree depth of {2}.",
        generationNumber, botsPerGeneration, maxTreeDepth);

      var generationInformation = new List<BotInformation>();
      if (generationNumber == 0) {
        // First generation, no breeding is required.
        if (previousGenerationReports != null) {
          _log.InfoFormat("Pre-seeding initial generation with {0} bots.", previousGenerationReports.Count);
          generationInformation.AddRange(previousGenerationReports.Select(report => report.Bot));
        }
        PopulateGenerationWithUniqueBots(generationInformation, generationNumber, botsPerGeneration, maxTreeDepth,
                                         optimise);
      } else {
        // Copy the elite bots verbatim.
        var eliteBotCount = botsPerGeneration * ElitePercentage / 100;
        _log.TraceFormat(
          "GeneCodeCS.Reproduction`1.BreedGeneration: Copying {0} elite bots verbatim from previous generation.",
          eliteBotCount);
        generationInformation.AddRange(previousGenerationReports.Take(eliteBotCount).Select(report => report.Bot));

        // Generate a percentage of the new population as random bots.
        var newBotCount = botsPerGeneration * RandomBotPercentage / 100;
        _log.TraceFormat("GeneCodeCS.Reproduction`1.BreedGeneration: Generating {0} random bots for this generation.",
                         newBotCount);
        PopulateGenerationWithUniqueBots(generationInformation, generationNumber, newBotCount, maxTreeDepth, optimise);

        // Select two parents and breed two children until this generation is full.
        var n = generationInformation.Count - 1;
        while (generationInformation.Count < botsPerGeneration) {
          var parents = SelectParents(previousGenerationReports);
          _log.TraceFormat("GeneCodeCS.Reproduction`1.BreedGeneration: Breeding bots {0} and {1}", parents[0].Name,
                           parents[1].Name);

          var parent1Tree = parents[0].Tree.Clone();
          var parent2Tree = parents[1].Tree.Clone();

          var children = CrossoverAndMutate(parent1Tree, parent2Tree);
          var child1 = children[0];
          var child2 = children[1];

          if (optimise != null) {
            _log.Trace("GeneCodeCS.Reproduction`1.CreateBot: Optimising bot trees.");
            child1 = optimise(child1);
            Debug.Assert(child1 != null, Resources.ChromosomeOptimisationReturnedNull);
            child2 = optimise(child2);
            Debug.Assert(child2 != null, Resources.ChromosomeOptimisationReturnedNull);
          }

          if (generationInformation.All(b => b.Tree != child1)) {
            var childResult1 = new BotInformation(CreateBotName(generationNumber, ++n), child1, parents[0].Name,
                                                  parents[1].Name);
            _log.TraceFormat("GeneCodeCS.Reproduction`1.BreedGeneration: Adding bot '{0}' to the generation:{2}{1}",
                             childResult1.Name, childResult1.Tree.ToString(), Environment.NewLine);
            generationInformation.Add(childResult1);
          }

          if (generationInformation.All(b => b.Tree != child2)) {
            var childResult2 = new BotInformation(CreateBotName(generationNumber, ++n), child2, parents[1].Name,
                                                  parents[0].Name);
            _log.TraceFormat("GeneCodeCS.Reproduction`1.BreedGeneration: Adding bot {0} to the generation:{2}{1}",
                             childResult2.Name, childResult2.Tree.ToString(), Environment.NewLine);
            generationInformation.Add(childResult2);
          }
        }
      }
      _log.TraceFormat("GeneCodeCS.Reproduction`1.BreedGeneration: Completed breeding generation {0}.", generationNumber);

      return generationInformation;
    }

    /// <summary>
    ///   Creates a single random bot with a specified maximum tree depth.
    /// </summary>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated bot. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    /// <returns> Information describing the generated bot. </returns>
    public BotInformation CreateBot(int maxTreeDepth, ChromosomeOptimiser optimise = null) {
      _log.TraceFormat("GeneCodeCS.Reproduction`1.CreateBot: Creating single bot with maximum tree depth {0}.",
                       maxTreeDepth);
      var tree = CreateRandomExpressionTree(maxTreeDepth);

      if (optimise != null) {
        _log.Trace("GeneCodeCS.Reproduction`1.CreateBot: Optimising bot tree.");
        tree = optimise(tree);
        Debug.Assert(tree != null, Resources.ChromosomeOptimisationReturnedNull);
      }

      var botName = string.Format("Bot{0:X}", tree.GetHashCode());

      _log.TraceFormat("GeneCodeCS.Reproduction`1.CreateBot: Generated bot '{0}':{2}{1}", botName, tree.ToString(),
                       Environment.NewLine);

      return new BotInformation(botName, tree);
    }

    /// <summary>
    ///   Constructs the name of a bot given its type, the generation it is in, and its index in that generation.
    /// </summary>
    /// <param name="generationNumber"> The generation number that the bot is in. </param>
    /// <param name="botIndex"> The index of the bot in its generation. </param>
    /// <returns> A name for the bot. </returns>
    private static string CreateBotName(int generationNumber, int botIndex) {
      if (generationNumber < 0) {
        throw new ArgumentOutOfRangeException("generationNumber", Resources.GenerationNumberValidRange);
      }
      if (botIndex < 0) {
        throw new ArgumentOutOfRangeException("botIndex", Resources.BotIndexValidRange);
      }

      return string.Format(Resources.BotNameStringFormat, typeof(TBot).Name, generationNumber, botIndex);
    }

    /// <summary>
    ///   Constructs a random expression tree of <see cref="T:GeneCodeCS.Genetics.Chromosome" /> classes with the maximum specfied depth..
    /// </summary>
    /// <remarks>
    ///   This function is recursively called via one of the Instantiate* methods.
    /// </remarks>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated bot. </param>
    /// <param name="parent"> The parent <see cref="T:GeneCodeCS.Genetics.Chromosome" /> for the newly constructed tree, or null if this is the root <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . </param>
    /// <returns> The constructed <see cref="T:GeneCodeCS.Genetics.Chromosome" /> tree. </returns>
    private Chromosome CreateRandomExpressionTree(int maxTreeDepth, Chromosome parent = null) {
      _log.TraceFormat(
        "GeneCodeCS.Reproduction`1.CreateRandomExpressionTree: Creating expression tree with maximum tree depth {0} and {1} parent.",
        maxTreeDepth, parent == null ? "null" : "non-null");

      var node = maxTreeDepth > 0 ? GetRandomFunctionOrTerminal() : GetRandomTerminal();

      if (node is BranchGene) {
        return InstantiateBranch(node as BranchGene, maxTreeDepth, parent);
      }
      if (node is SequenceGene) {
        return InstantiateSequence(node as SequenceGene, maxTreeDepth, parent);
      }
      if (node is TerminalGene) {
        return InstantiateTerminal(node as TerminalGene, parent);
      }

      throw new UnknownGenotypeException("An unknown Genotype was encountered.");
    }

    private Chromosome[] CrossoverAndMutate(Chromosome parent1, Chromosome parent2) {
      var parent1Nodes = FlattenExpressionTree(parent1);
      var parent2Nodes = FlattenExpressionTree(parent2);
      // select a random subree from each
      var crossoverPoint1 = _random.Next(0, parent1Nodes.Count);
      var crossoverPoint2 = _random.Next(0, parent2Nodes.Count);

      var child1 = TreeCombine(parent1, parent1Nodes[crossoverPoint1], parent2Nodes[crossoverPoint2]);
      var child2 = TreeCombine(parent2, parent2Nodes[crossoverPoint2], parent1Nodes[crossoverPoint1]);

      return new[] { child1, child2 };
    }

    private List<Chromosome> FlattenExpressionTree(Chromosome tree) {
      var flat = new List<Chromosome> { tree };
      if (tree.Node is BranchExpression) {
        var branch = tree.Node as BranchExpression;
        flat.AddRange(FlattenExpressionTree(branch.TrueBranch));
        flat.AddRange(FlattenExpressionTree(branch.FalseBranch));
      } else if (tree.Node is SequenceExpression) {
        foreach (var sequence in ((SequenceExpression)tree.Node).Expressions) {
          flat.AddRange(FlattenExpressionTree(sequence));
        }
      }
      return flat;
    }

    private object[] GenerateParameters(IEnumerable<ParameterInfo> methodParameters) {
      if (methodParameters == null) {
        throw new ArgumentNullException("methodParameters");
      }
      return
        methodParameters.Select(parameter => Activator.CreateInstance(parameter.ParameterType, _random)).Select(
          parameterInstance => ((dynamic)parameterInstance).Value).Cast<object>().ToArray();
    }

    private IGene GetRandomFunctionOrTerminal() {
      return _nonTerminals.Union(_terminals).OrderBy(x => _random.Next()).First();
    }

    private TerminalGene GetRandomTerminal() {
      return _terminals.OrderBy(x => _random.Next()).First();
    }

    /// <summary>
    ///   Configures the randomiser and analyses the given bot base type in order to ses up the internal fields used by class.
    /// </summary>
    /// <remarks>
    ///   This method is only ever called by the constructor.
    /// </remarks>
    /// <param name="randomSeed"> A value to use to seed the randomiser used within the class. This allows for reproducibility if required. If 0, then the current time is used to seed the randomiser. </param>
    private void Initialise(int randomSeed) {
      RandomiseSeed(randomSeed);

      // Locate all the terminals and branching methods in the base class.
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var botClassMethods = typeof(TBot).GetMethods(flags);
      foreach (var mi in botClassMethods.Where(mi => mi.DeclaringType != typeof(object))) {
        if (mi.ReturnType == typeof(void)) {
          // Terminals are void methods.
          if (mi.Name != "Execute" && mi.Name != "Initialise" && !mi.Name.StartsWith("set_") &&
              !mi.Name.StartsWith("get_")) {
            _terminals.Add(new TerminalGene(mi));
          }
        } else if (mi.ReturnType == typeof(bool)) {
          // Branching methods are bool methods.
          _nonTerminals.Add(new BranchGene(mi));
        }
      }

      // Construct a number of sequence genes too.
      for (var n = 2; n <= 6; ++n) {
        _nonTerminals.Add(new SequenceGene(n));
      }
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="T:GeneCodeCS.Genetics.BranchGene" /> and wraps it in a <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . Since this is a branching gene, "true" and "false" <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> instances will be randomly created too.
    /// </summary>
    /// <param name="branchGene"> The gene to construct an expression for. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated expression, including the expression created by this method. </param>
    /// <param name="parent"> The parent <see cref="T:GeneCodeCS.Genetics.Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . </param>
    /// <returns> A new <see cref="T:GeneCodeCS.Genetics.Chromosome" /> containing the constructed <see
    ///    cref="T:GeneCodeCS.Genetics.BranchExpression" /> . </returns>
    private Chromosome InstantiateBranch(BranchGene branchGene, int maxTreeDepth, Chromosome parent) {
      _log.TraceFormat(
        "GeneCodeCS.Reproduction`1.InstantiateBranch: Creating branch expression for {0} with maximum tree depth {1} and {2} parent.",
        branchGene.MethodInfo.Name, maxTreeDepth, parent == null ? "null" : "non-null");

      var methodParameters = branchGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var newTree = new Chromosome();

      _log.Trace("GeneCodeCS.Reproduction`1.InstantiateBranch: Creating \"true\" Chromosome branch.");
      var trueBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);

      _log.Trace("GeneCodeCS.Reproduction`1.InstantiateBranch: Creating \"false\" Chromosome branch.");
      var falseBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);

      newTree.Node = new BranchExpression(branchGene.MethodInfo, parameters, trueBranch, falseBranch);
      newTree.Parent = parent;
      return newTree;
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="T:GeneCodeCS.Genetics.SequenceGene" /> and wraps it in a <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . Since the new sequence may contain branches, a maximum tree depth must also be specified.
    /// </summary>
    /// <param name="sequenceGene"> The gene to construct an expression for. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated expression, including the expression created by this method. </param>
    /// <param name="parent"> The parent <see cref="T:GeneCodeCS.Genetics.Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . </param>
    /// <returns> A new <see cref="T:GeneCodeCS.Genetics.Chromosome" /> containing the constructed <see
    ///    cref="T:GeneCodeCS.Genetics.SequenceExpression" /> . </returns>
    private Chromosome InstantiateSequence(SequenceGene sequenceGene, int maxTreeDepth, Chromosome parent) {
      _log.TraceFormat(
        "GeneCodeCS.Reproduction`1.InstantiateSequence: Creating sequence expression of length {0} with maximum tree depth {1} and {2} parent.",
        sequenceGene.Length, maxTreeDepth, parent == null ? "null" : "non-null");

      var newTree = new Chromosome { Parent = parent };
      var sequenceCount = sequenceGene.Length;

      var expressions = new Chromosome[sequenceCount];
      for (var n = 0; n < sequenceCount; ++n) {
        _log.TraceFormat("GeneCodeCS.Reproduction`1.InstantiateSequence: Creating Chromosome sequence element {0}.", n);
        expressions[n] = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      }

      newTree.Node = new SequenceExpression(expressions);
      return newTree;
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="T:GeneCodeCS.Genetics.TerminalGene" /> and wraps it in a <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> .
    /// </summary>
    /// <param name="terminalGene"> The gene to construct an expression for. </param>
    /// <param name="parent"> The parent <see cref="T:GeneCodeCS.Genetics.Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="T:GeneCodeCS.Genetics.Chromosome" /> . </param>
    /// <returns> A new <see cref="T:GeneCodeCS.Genetics.Chromosome" /> containing the constructed <see
    ///    cref="T:GeneCodeCS.Genetics.TerminalExpression" /> . </returns>
    private Chromosome InstantiateTerminal(TerminalGene terminalGene, Chromosome parent) {
      _log.TraceFormat(
        "GeneCodeCS.Reproduction`1.InstantiateTerminal: Creating branch expression for {0} with {1} parent.",
        terminalGene.MethodInfo.Name, parent == null ? "null" : "non-null");

      var methodParameters = terminalGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var terminalExpression = new TerminalExpression(terminalGene.MethodInfo, parameters);
      return new Chromosome { Node = terminalExpression, Parent = parent };
    }

    private void PopulateGenerationWithUniqueBots(ICollection<BotInformation> thisGeneration, int generationNumber,
                                                  int limit, int maxTreeDepth, ChromosomeOptimiser optimise) {
      var n = thisGeneration.Count - 1;
      while (thisGeneration.Count < limit) {
        var newTree = CreateRandomExpressionTree(maxTreeDepth);
        if (optimise != null) {
          newTree = optimise(newTree);
        }

        if (thisGeneration.Any(b => b.Tree == newTree)) {
          continue;
        }

        _log.Trace(newTree.ToString());

        var name = CreateBotName(generationNumber, ++n);
        thisGeneration.Add(new BotInformation(name, newTree));
      }
    }

    private void RandomiseSeed(int randomSeed) {
      var ticks = randomSeed == 0 ? Environment.TickCount : randomSeed;
      _log.Info(string.Format("Random seed {0}", ticks));
      _random = new Random(ticks);
    }

    private BotInformation[] SelectParents(List<BotReport> bots) {
      var parents = new BotInformation[2];

      parents[0] = SelectRandomParentBasedOnFitness(bots);
      parents[1] = SelectRandomParentBasedOnFitness(bots);

      return parents;
    }

    private BotInformation SelectRandomParentBasedOnFitness(List<BotReport> lastGeneration) {
      // Make all fitnesses positive.
      var fitnessAdjustment = 1 - lastGeneration.Min(g => g.Fitness);
      var lastGenerationTotalFitness = lastGeneration.Sum(g => g.Fitness + fitnessAdjustment);
      var target = _random.NextDouble() * lastGenerationTotalFitness;
      var currentSumFitness = 0.0d;
      var parent = lastGeneration.SkipWhile(g => {
                                              currentSumFitness += g.Fitness + fitnessAdjustment;
                                              return currentSumFitness < target;
                                            }).First();
      return parent.Bot;
    }

    private Chromosome TreeCombine(Chromosome source, Chromosome cutPoint, Chromosome insertionMaterial) {
      var mTreeCopy = source;

      if (ReferenceEquals(source, cutPoint)) {
        mTreeCopy = Chromosome.ReplaceNodeWithCopy(source, insertionMaterial);
      } else if (mTreeCopy.Node is BranchExpression) {
        var branch = mTreeCopy.Node as BranchExpression;
        branch.TrueBranch = TreeCombine(branch.TrueBranch, cutPoint, insertionMaterial);
        branch.FalseBranch = TreeCombine(branch.FalseBranch, cutPoint, insertionMaterial);
        // Apply mutation
        if (branch.TrueBranch.Node is TerminalExpression) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.TrueBranch = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
        if (branch.FalseBranch.Node is TerminalExpression) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.FalseBranch = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
      } else if (mTreeCopy.Node is SequenceExpression) {
        var sequence = mTreeCopy.Node as SequenceExpression;
        for (var index = 0; index < sequence.Expressions.Length; ++index) {
          sequence.Expressions[index] = TreeCombine(sequence.Expressions[index], cutPoint, insertionMaterial);
        }
        if (sequence.Expressions.Length > 1 && _random.Next(0, 100) < MutationRate) {
          var point = _random.Next(0, sequence.Expressions.Length - 1);
          var temp = sequence.Expressions[point];
          sequence.Expressions[point] = sequence.Expressions[point + 1];
          sequence.Expressions[point + 1] = temp;
        }
      }
      return mTreeCopy;
    }
  }
}