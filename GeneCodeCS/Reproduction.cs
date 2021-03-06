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
using System.ComponentModel;
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
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see cref="BaseBot" /> . Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="IParameter{T}" /> . See the <see cref="BaseBot" /> documentation for restrictions and requirements of implementing this class type. </typeparam>
  /// <remarks>
  ///   This class makes use of the <see cref="Random" /> class and so should not be considered cryptographically secure.
  /// </remarks>
  public sealed class Reproduction<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///   A list of non-terminal genes, formed from the branch methods found in the <typeparamref name="TBot" /> type and constructed sequence genes.
    /// </summary>
    /// <remarks>
    ///   Not null. Can contain zero elements.
    /// </remarks>
    private readonly List<IGene> _nonTerminals = new List<IGene>();

    /// <summary>
    ///   A list of the terminal methods found in the <typeparamref name="TBot" /> type.
    /// </summary>
    /// <remarks>
    ///   Not null. Must contain at least one element.
    /// </remarks>
    private readonly List<TerminalGene> _terminals = new List<TerminalGene>();

    /// <summary>
    ///   The crossover rate (chance) to use during breeding (0-100). Defaults to 90.
    /// </summary>
    private int _crossoverRate = 90;

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
    ///   The percentage of the bots to generate randomly in each generation after the first (0-100). Defaults to 20.
    /// </summary>
    private int _randomBotPercentage = 20;

    /// <summary>
    ///   Initialises a new instance of the <see cref="Reproduction{TBot}" /> class. The provided <typeparamref name="TBot" /> type is inspected and the qualifying terminal and non-terminal methods are extracted and stored.
    /// </summary>
    /// <param name="randomSeed"> A value to use to seed the randomiser used within the class. This allows for reproducibility if required. If 0, then the current time is used to seed the randomiser. </param>
    public Reproduction(int randomSeed = 0) {
      InitialisationMethod = Initialisation.Full;
      _log.Trace("ctor: Constructing class.");

      InitialisationMethod = Initialisation.Grow;
      SelectionMethod = Selection.Tournament;

      Initialise(randomSeed);
    }

    /// <summary>
    ///   Gets or sets the crossover rate (chance) to use during breeding (0-100). Defaults to 90.
    /// </summary>
    public int CrossoverRate {
      get { return _crossoverRate; }
      set {
        if (_crossoverRate < 0 || _crossoverRate > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.CrossoverRateValidRange);
        }
        _log.TraceFormat("get_CrossoverRate: Crossover rate set to {0}.", value);
        _crossoverRate = value;
      }
    }

    /// <summary>
    ///   Gets and sets the tree creation method to use when generating new bots.
    /// </summary>
    public Initialisation InitialisationMethod { get; set; }

    /// <summary>
    ///   Gets or sets the mutation rate (chance) to use during breeding (0-100). Defaults to 5.
    /// </summary>
    public int MutationRate {
      get { return _mutationRate; }
      set {
        if (_mutationRate < 0 || _mutationRate > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.MutationRateValidRange);
        }
        _log.TraceFormat("get_MutationRate: Mutation rate set to {0}.", value);
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
        _log.TraceFormat("get_RandomBotPercentage: Random bot percentage set to {0}.", value);
        _randomBotPercentage = value;
      }
    }

    /// <summary>
    ///   Gets or sets the method to use for selecting bots to breed from a population.
    /// </summary>
    public Selection SelectionMethod { get; set; }

    /// <summary>
    ///   Breeds a generation of <paramref name="botsPerGeneration" /> bots. If the <paramref name="generationNumber" /> is 0 then copies any bots provided in <paramref
    ///    name="previousGenerationReports" /> and creates the remaining number randomly. If this is not the first generation, the generation is formed from the top 10% of bots from the last generation, 10% of randomly created bots. The remaining 80% are created by breeding (swapping chromosome expression trees at random points and introducing mutations) the previous generation. This is done such bots from the previous generation are more likely to be parents the greater their fitness.
    /// </summary>
    /// <param name="generationNumber"> The number of the generation to breed. Used for creating the bot class names. If the value is 0 then the <paramref
    ///    name="previousGenerationReports" /> are copied and the remainder of the generation is created randomly. </param>
    /// <param name="botsPerGeneration"> The number of bots to create or breed in each generation. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation in the initial generation. </param>
    /// <param name="previousGenerationReports"> The report for the previous generation or null if this is the first generation. Can also be used to pre-seed (fully or partially) the first generation if <paramref
    ///    name="generationNumber" /> is 0. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    /// <returns> A list of information about the new generation of bots. </returns>
    public List<BotInformation<TBot>> BreedGeneration(int generationNumber, int botsPerGeneration, int maxTreeDepth,
                                                      List<BotInformation<TBot>> previousGenerationReports,
                                                      Func<Chromosome, Chromosome> optimise = null) {
      _log.TraceFormat("BreedGeneration: Breeding generation {0} with {1} bots per generation and tree depth of {2}.",
                       generationNumber, botsPerGeneration, maxTreeDepth);

      if (generationNumber < 0) {
        throw new ArgumentOutOfRangeException("generationNumber", Resources.GenerationNumberValidRange);
      }
      if (botsPerGeneration < 1) {
        throw new ArgumentOutOfRangeException("botsPerGeneration", Resources.BotsPerGenerationValidRange);
      }
      if (maxTreeDepth < 0) {
        throw new ArgumentOutOfRangeException("maxTreeDepth", Resources.MaxTreeDepthValidRange);
      }
      if (generationNumber > 0 && previousGenerationReports == null) {
        throw new ArgumentNullException("previousGenerationReports", Resources.PreviousGenerationReportsRequired);
      }

      var start = DateTime.Now;

      var generationInformation = new List<BotInformation<TBot>>();
      if (generationNumber == 0) {
        // First generation, no breeding is required.
        if (previousGenerationReports != null) {
          _log.InfoFormat("Pre-seeding initial generation with {0} bots.", previousGenerationReports.Count);
          generationInformation.AddRange(previousGenerationReports);
        }
        var botsToAdd = Math.Max(botsPerGeneration - generationInformation.Count, 0);
        PopulateGenerationWithRandomBots(botsToAdd, generationInformation, generationNumber, maxTreeDepth, optimise);
      } else {
        // Select two parents and breed a child until this generation is full.
        var n = generationInformation.Count - 1;
        while (generationInformation.Count < botsPerGeneration) {
          var parents = SelectParents(previousGenerationReports);
          _log.TraceFormat("BreedGeneration: Breeding bots {0} and {1}", parents[0].Name, parents[1].Name);

          var operation = _random.Next(100);
          var parent1Tree = parents[0].Tree.Clone();
          Chromosome parent2Tree;
          string parent2Name;

          if (operation < MutationRate) {
            _log.Trace("BreedGeneration: Generating new random bot for headless chicken breeding.");
            parent2Tree = CreateRandomExpressionTree(maxTreeDepth);
            parent2Name = string.Format("ChickenBot_{0:x}", parent2Tree.GetHashCode());
          } else if (operation < MutationRate + CrossoverRate) {
            parent2Tree = parents[1].Tree.Clone();
            parent2Name = parents[1].Name;
          } else {
            if (generationInformation.Any(b => b.Tree == parents[0].Tree)) {
              continue;
            }
            _log.TraceFormat("BreedGeneration: Copying bot {0} verbatim from previous generation.", parents[0].Name);
            generationInformation.Add(parents[0]);
            continue;
          }
          var child = CrossoverAndMutate(parent1Tree, parent2Tree);

          if (optimise != null) {
            _log.Trace("CreateBot: Optimising bot trees.");
            child = optimise(child);
            Debug.Assert(child != null, Resources.ChromosomeOptimisationReturnedNull);
          }

          if (generationInformation.Any(b => b.Tree == child)) {
            continue;
          }

          var childResult = new BotInformation<TBot>(CreateBotName(generationNumber, ++n), child, parents[0].Name,
                                                     parent2Name);
          _log.TraceFormat("BreedGeneration: Adding bot '{0}' to the generation:{2}{1}", childResult.Name,
                           childResult.Tree.ToString(), Environment.NewLine);
          generationInformation.Add(childResult);
        }
      }
      var end = DateTime.Now;
      _log.TraceFormat("BreedGeneration: Completed breeding generation {0}. Time taken: {1:T}.", generationNumber,
                       end - start);

      return generationInformation;
    }

    /// <summary>
    ///   Creates a single random bot with a specified maximum tree depth.
    /// </summary>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated bot. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    /// <returns> Information describing the generated bot. </returns>
    public BotInformation<TBot> CreateBot(int maxTreeDepth, Func<Chromosome, Chromosome> optimise = null) {
      _log.TraceFormat("CreateBot: Creating single bot with maximum tree depth {0}.", maxTreeDepth);

      if (maxTreeDepth < 0) {
        throw new ArgumentOutOfRangeException("maxTreeDepth", Resources.MaxTreeDepthValidRange);
      }

      var tree = CreateRandomExpressionTree(maxTreeDepth);

      if (optimise != null) {
        _log.Trace("CreateBot: Optimising bot tree.");
        tree = optimise(tree);
        Debug.Assert(tree != null, Resources.ChromosomeOptimisationReturnedNull);
      }

      var botName = string.Format("Bot{0:X}", tree.GetHashCode());

      _log.TraceFormat("CreateBot: Generated bot '{0}':{2}{1}", botName, tree.ToString(), Environment.NewLine);

      return new BotInformation<TBot>(botName, tree);
    }

    /// <summary>
    ///   Constructs the name of a bot given its type, the generation it is in, and its index in that generation.
    /// </summary>
    /// <param name="generationNumber"> The generation number that the bot is in. </param>
    /// <param name="botIndex"> The index of the bot in its generation. </param>
    /// <returns> A name for the bot. </returns>
    private static string CreateBotName(int generationNumber, int botIndex) {
      Debug.Assert(generationNumber >= 0, Resources.GenerationNumberValidRange);
      Debug.Assert(botIndex >= 0, Resources.BotIndexValidRange);

      return string.Format(Resources.BotNameStringFormat, typeof(TBot).Name, generationNumber, botIndex);
    }

    /// <summary>
    ///   Constructs an array containing a pointer to each gene expression in the original <see cref="Chromosome" /> tree. The construction is done using depth-first search.
    /// </summary>
    /// <param name="tree"> The tree to flatten. </param>
    /// <returns> The flattened tree. Since each element is a pointer to the original tree, the branching structures and their children can be accessed directly from the relevant entries in the array. </returns>
    private static List<Chromosome> FlattenExpressionTree(Chromosome tree) {
      Debug.Assert(tree != null, "A non-null tree is required.");

      var flat = new List<Chromosome> { tree };

      if (tree.Node is TerminalExpression) {
        return flat;
      }

      var branchExpression = tree.Node as BranchExpression;
      if (branchExpression != null) {
        flat.AddRange(FlattenExpressionTree(branchExpression.TrueBranch));
        flat.AddRange(FlattenExpressionTree(branchExpression.FalseBranch));
        return flat;
      }

      var sequenceExpression = tree.Node as SequenceExpression;
      if (sequenceExpression != null) {
        foreach (var sequence in sequenceExpression.Expressions) {
          flat.AddRange(FlattenExpressionTree(sequence));
        }
        return flat;
      }

      throw new UnknownGenotypeException("An unknown Genotype was encountered.");
    }

    /// <summary>
    ///   Constructs a random expression tree of <see cref="Chromosome" /> classes with the maximum specfied depth..
    /// </summary>
    /// <remarks>
    ///   This function is recursively called via one of the Instantiate* methods.
    /// </remarks>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated bot. </param>
    /// <param name="parent"> The parent <see cref="Chromosome" /> for the newly constructed tree, or null if this is the root <see
    ///    cref="Chromosome" /> . </param>
    /// <returns> The constructed <see cref="Chromosome" /> tree. </returns>
    private Chromosome CreateRandomExpressionTree(int maxTreeDepth, Chromosome parent = null) {
      _log.TraceFormat(
        "CreateRandomExpressionTree: Creating expression tree with maximum tree depth {0} and {1} parent.", maxTreeDepth,
        parent == null ? "null" : "non-null");

      Debug.Assert(maxTreeDepth >= 0, Resources.MaxTreeDepthValidRange);

      IGene node;
      if (maxTreeDepth <= 0) {
        node = GetRandomTerminal();
      } else {
        switch (InitialisationMethod) {
          case Initialisation.Full:
            node = GetRandomFunction();
            break;

          case Initialisation.Grow:
            node = GetRandomFunctionOrTerminal();
            break;

          default:
            throw new InvalidEnumArgumentException("InitialisationMethod", (int)InitialisationMethod,
                                                   typeof(Initialisation));
        }
      }

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

    /// <summary>
    ///   Takes two cloned <see cref="Chromosome" /> instances, picks two random points on their expression trees, and switches the sub-trees around. Mutations are also introduced at this stage with a configurable percentage chance.
    /// </summary>
    /// <param name="chromosome1"> The first chromosome to combine. </param>
    /// <param name="chromosome2"> The second chromosome to combine. </param>
    /// <returns> An array of the two combined <see cref="Chromosome" /> classes. </returns>
    private Chromosome CrossoverAndMutate(Chromosome chromosome1, Chromosome chromosome2) {
      _log.Trace("CrossoverAndMutate: Beginning crossover of chromosomes.");

      Debug.Assert(chromosome1 != null, "A non-null chromosome must be specified.");
      Debug.Assert(chromosome2 != null, "A non-null chromosome must be specified.");

      var chromosome1Nodes = FlattenExpressionTree(chromosome1);
      var chromosome2Nodes = FlattenExpressionTree(chromosome2);

      // Select a random subree from each.
      var crossoverPoint1 = _random.Next(0, chromosome1Nodes.Count);
      var crossoverPoint2 = _random.Next(0, chromosome2Nodes.Count);

      return TreeCombine(chromosome1, chromosome1Nodes[crossoverPoint1], chromosome2Nodes[crossoverPoint2]);
    }

    /// <summary>
    ///   Initialises parameters to a method call. An instance of the parameter type is first created passing a <see
    ///    cref="Random" /> object. The generated value is extracted and returned in the parameter array.
    /// </summary>
    /// <param name="methodParameters"> The parameters to generate values for. </param>
    /// <returns> An array of generated parameter values. </returns>
    private object[] GenerateParameters(IEnumerable<ParameterInfo> methodParameters) {
      _log.Trace("GenerateParameters: Generating method parameters.");

      Debug.Assert(methodParameters != null, "A non-null set of method parameters must be specified.");

      return
        methodParameters.Select(parameter => Activator.CreateInstance(parameter.ParameterType, _random)).Select(
          parameterInstance => ((dynamic)parameterInstance).Value).Cast<object>().ToArray();
    }

    /// <summary>
    ///   Returns a random <see cref="IGene" /> from the list (based on <typeparamref name="TBot" /> ) of functions calculated when the class was first instantiated.
    /// </summary>
    /// <returns> A randomly selected <see cref="IGene" /> instance. </returns>
    private IGene GetRandomFunction() { return _nonTerminals.OrderBy(x => _random.Next()).First(); }

    /// <summary>
    ///   Returns a random <see cref="IGene" /> from the list (based on <typeparamref name="TBot" /> ) calculated when the class was first instantiated.
    /// </summary>
    /// <returns> A randomly selected <see cref="IGene" /> instance. </returns>
    private IGene GetRandomFunctionOrTerminal() { return _nonTerminals.Union(_terminals).OrderBy(x => _random.Next()).First(); }

    /// <summary>
    ///   Returns a random <see cref="TerminalGene" /> from the list (based on <typeparamref name="TBot" /> ) calculated when the class was first instantiated.
    /// </summary>
    /// <returns> A randomly selected <see cref="TerminalGene" /> instance. </returns>
    private TerminalGene GetRandomTerminal() { return _terminals.OrderBy(x => _random.Next()).First(); }

    /// <summary>
    ///   Configures the randomiser and analyses the given bot base type in order to ses up the internal fields used by class.
    /// </summary>
    /// <remarks>
    ///   This method is only ever called by the constructor.
    /// </remarks>
    /// <param name="randomSeed"> A value to use to seed the randomiser used within the class. This allows for reproducibility if required. If 0, then the current time is used to seed the randomiser. </param>
    private void Initialise(int randomSeed) {
      _log.TraceFormat("Initialise: Initialising the Reproduction class with bot type {0}.", typeof(TBot).Name);

      RandomiseSeed(randomSeed);

      // Locate all the terminals and branching methods in the base class.
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var botClassMethods = typeof(TBot).GetMethods(flags);
      // Can be parallelised. Need to change List data structures to thread-safe ones.
      foreach (var mi in botClassMethods.Where(mi => mi.DeclaringType != typeof(object))) {
        if (mi.ReturnType == typeof(void)) {
          // Terminals are void methods.
          if (mi.Name != "Execute" && mi.Name != "Initialise" && !mi.Name.StartsWith("set_") &&
              !mi.Name.StartsWith("get_")) {
            // TODO: Check valid parameter types.
            _log.TraceFormat("Initialise: Found terminal gene {0} ({1} parameters).", mi.Name, mi.GetParameters().Length);
            _terminals.Add(new TerminalGene(mi));
          }
        } else if (mi.ReturnType == typeof(bool)) {
          // Branching methods are bool methods.
          // TODO: Check valid parameter types.
          _log.TraceFormat("Initialise: Found branch gene {0} ({1} parameters).", mi.Name, mi.GetParameters().Length);
          _nonTerminals.Add(new BranchGene(mi));
        }
      }

      Debug.Assert(_terminals.Count > 0, Resources.TerminalMethodsValidRange);

      // Construct a number of sequence genes too.
      for (var n = 2; n <= 6; ++n) {
        _nonTerminals.Add(new SequenceGene(n));
      }

      _log.TraceFormat("Initialise: Found {0} terminal and {1} non-terminal genes.", _terminals.Count,
                       _nonTerminals.Count);
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="BranchGene" /> and wraps it in a <see cref="Chromosome" /> . Since this is a branching gene, "true" and "false" <see
    ///    cref="Chromosome" /> instances will be randomly created too.
    /// </summary>
    /// <param name="branchGene"> The gene to construct an expression for. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated expression, including the expression created by this method. </param>
    /// <param name="parent"> The parent <see cref="Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="Chromosome" /> . </param>
    /// <returns> A new <see cref="Chromosome" /> containing the constructed <see cref="BranchExpression" /> . </returns>
    private Chromosome InstantiateBranch(BranchGene branchGene, int maxTreeDepth, Chromosome parent) {
      Debug.Assert(branchGene != null, "A non-null branch gene must be specified.");
      Debug.Assert(maxTreeDepth >= 0, Resources.MaxTreeDepthValidRange);

      _log.TraceFormat(
        "InstantiateBranch: Creating branch expression for {0} with maximum tree depth {1} and {2} parent.",
        branchGene.MethodInfo.Name, maxTreeDepth, parent == null ? "null" : "non-null");

      var methodParameters = branchGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var newTree = new Chromosome();

      _log.Trace("InstantiateBranch: Creating \"true\" Chromosome branch.");
      var trueBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);

      _log.Trace("InstantiateBranch: Creating \"false\" Chromosome branch.");
      var falseBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);

      newTree.Node = new BranchExpression(branchGene.MethodInfo, parameters, trueBranch, falseBranch);
      newTree.Parent = parent;
      return newTree;
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="SequenceGene" /> and wraps it in a <see cref="Chromosome" /> . Since the new sequence may contain branches, a maximum tree depth must also be specified.
    /// </summary>
    /// <param name="sequenceGene"> The gene to construct an expression for. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow in the generated expression, including the expression created by this method. </param>
    /// <param name="parent"> The parent <see cref="Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="Chromosome" /> . </param>
    /// <returns> A new <see cref="Chromosome" /> containing the constructed <see cref="SequenceExpression" /> . </returns>
    private Chromosome InstantiateSequence(SequenceGene sequenceGene, int maxTreeDepth, Chromosome parent) {
      Debug.Assert(sequenceGene != null, "A non-null sequence gene must be specified.");
      Debug.Assert(maxTreeDepth >= 0, Resources.MaxTreeDepthValidRange);

      _log.TraceFormat(
        "InstantiateSequence: Creating sequence expression of length {0} with maximum tree depth {1} and {2} parent.",
        sequenceGene.Length, maxTreeDepth, parent == null ? "null" : "non-null");

      var newTree = new Chromosome { Parent = parent };
      var sequenceCount = sequenceGene.Length;

      var expressions = new Chromosome[sequenceCount];
      for (var n = 0; n < sequenceCount; ++n) {
        _log.TraceFormat("InstantiateSequence: Creating Chromosome sequence element {0}.", n);
        expressions[n] = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      }

      newTree.Node = new SequenceExpression(expressions);
      return newTree;
    }

    /// <summary>
    ///   Creates an expression instance of the specified <see cref="TerminalGene" /> and wraps it in a <see cref="Chromosome" /> .
    /// </summary>
    /// <param name="terminalGene"> The gene to construct an expression for. </param>
    /// <param name="parent"> The parent <see cref="Chromosome" /> for the newly constructed gene expression, or null if this is the root <see
    ///    cref="Chromosome" /> . </param>
    /// <returns> A new <see cref="Chromosome" /> containing the constructed <see cref="TerminalExpression" /> . </returns>
    private Chromosome InstantiateTerminal(TerminalGene terminalGene, Chromosome parent) {
      Debug.Assert(terminalGene != null, "A non-null terminal gene must be specified.");

      _log.TraceFormat("InstantiateTerminal: Creating branch expression for {0} with {1} parent.",
                       terminalGene.MethodInfo.Name, parent == null ? "null" : "non-null");

      var methodParameters = terminalGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var terminalExpression = new TerminalExpression(terminalGene.MethodInfo, parameters);
      return new Chromosome { Node = terminalExpression, Parent = parent };
    }

    /// <summary>
    ///   Adds a specified number of random bots to a generation.
    /// </summary>
    /// <param name="botsToAdd"> The number of bots to add to the generation. </param>
    /// <param name="generation"> The generation to append to. </param>
    /// <param name="generationNumber"> The number of the generation to create the bots in. Used for creating the bot class names. </param>
    /// <param name="maxTreeDepth"> The maximum tree depth to allow during randomised bot generation. </param>
    /// <param name="optimise"> A method that can optionally be provided to optimise the created expression tree, removing redundant code and consolidating statements. </param>
    private void PopulateGenerationWithRandomBots(int botsToAdd, ICollection<BotInformation<TBot>> generation,
                                                  int generationNumber, int maxTreeDepth,
                                                  Func<Chromosome, Chromosome> optimise) {
      _log.TraceFormat("PopulateGenerationWithRandomBots: Adding {0} bots to generation {1} with max tree depth {2}.",
                       botsToAdd, generationNumber, maxTreeDepth);

      if (botsToAdd == 0) {
        _log.Trace("PopulateGenerationWithRandomBots: No bots to add.");
        return;
      }

      Debug.Assert(botsToAdd > 0, "The number of bots to add must be non-negative.");
      Debug.Assert(generation != null, "A valid generation must be specified to append to.");
      Debug.Assert(generationNumber >= 0, Resources.GenerationNumberValidRange);
      Debug.Assert(maxTreeDepth >= 0, Resources.MaxTreeDepthValidRange);

      var targetBotCount = generation.Count + botsToAdd;
      while (generation.Count < targetBotCount) {
        var newTree = CreateRandomExpressionTree(maxTreeDepth);
        if (optimise != null) {
          _log.Trace("PopulateGenerationWithRandomBots: Optimising bot tree.");
          newTree = optimise(newTree);
          Debug.Assert(newTree != null, Resources.ChromosomeOptimisationReturnedNull);
        }

        if (generation.Any(b => b.Tree == newTree)) {
          // This is a potential infinite loop if the base bot model is simple, e.g. one terminal with no parameters.
          // TODO: Add a maximum iteration counter before throwing an exception.
          continue;
        }

        var name = CreateBotName(generationNumber, generation.Count);

        _log.TraceFormat("PopulateGenerationWithRandomBots: Adding bot '{0}' to the generation:{2}{1}", name,
                         newTree.ToString(), Environment.NewLine);
        generation.Add(new BotInformation<TBot>(name, newTree));
      }
    }

    /// <summary>
    ///   Sets the random seed value for the class. Can be user-provided.
    /// </summary>
    /// <param name="randomSeed"> The value to seed the random class with. If 0, then a time-based seed is used. </param>
    private void RandomiseSeed(int randomSeed) {
      var ticks = randomSeed == 0 ? Environment.TickCount : randomSeed;
      _log.InfoFormat("Random seed set to {0} ({1})", ticks, randomSeed == 0 ? "Time-based" : "User-provided");
      _random = new Random(ticks);
    }

    /// <summary>
    ///   Chooses two parent bots randomly from a list, weighted by their fitness values.
    /// </summary>
    /// <param name="bots"> The list of bots to pick from. </param>
    /// <returns> An array of information about the two selected bots. </returns>
    private BotInformation<TBot>[] SelectParents(ICollection<BotInformation<TBot>> bots) {
      Debug.Assert(bots != null, "A collection of bots must be specified.");

      var parents = new BotInformation<TBot>[2];
      switch (SelectionMethod) {
        case Selection.FitnessProportionate:
          parents[0] = SelectRandomBotWeightedByFitness(bots);
          parents[1] = SelectRandomBotWeightedByFitness(bots);
          return parents;
        case Selection.Tournament:
          parents[0] = SelectRandomBotByTournament(bots);
          parents[1] = SelectRandomBotByTournament(bots);
          return parents;
        default:
          throw new InvalidEnumArgumentException("SelectionMethod", (int)SelectionMethod, typeof(Selection));
      }
    }

    private BotInformation<TBot> SelectRandomBotByTournament(ICollection<BotInformation<TBot>> bots) {
      Debug.Assert(bots != null && bots.Count > 0, "A collection of at least one bot must be specified.");

      var tournamentSize = Math.Max(bots.Count / 50, 1);
      return bots.OrderBy(b => _random.Next()).Take(tournamentSize).OrderByDescending(b => b.Bot.Fitness).First();
    }

    /// <summary>
    ///   Pick a bot randomly from a list , weighted by their fitness values.
    /// </summary>
    /// <param name="bots"> The list of bots to pick from. </param>
    /// <returns> Information about the chosen bot. </returns>
    private BotInformation<TBot> SelectRandomBotWeightedByFitness(ICollection<BotInformation<TBot>> bots) {
      Debug.Assert(bots != null && bots.Count > 0, "A collection of at least one bot must be specified.");

      // Make all fitnesses positive.
      var fitnessAdjustment = 1 - bots.Min(g => g.Bot.Fitness);

      var totalSumFitness = bots.Sum(g => g.Bot.Fitness + fitnessAdjustment);
      var targetFitness = _random.NextDouble() * totalSumFitness;

      var currentSumFitness = 0.0d;
      var parent = bots.SkipWhile(g => {
        currentSumFitness += g.Bot.Fitness + fitnessAdjustment;
        return currentSumFitness < targetFitness;
      }).First();
      return parent;
    }

    /// <summary>
    ///   Replaces a node in a <see cref="Chromosome" /> tree with another tree.
    /// </summary>
    /// <param name="source"> The tree to modify. </param>
    /// <param name="cutPoint"> The point on the <paramref name="source" /> tree to remove. </param>
    /// <param name="insertionMaterial"> The new tree to replace <paramref name="cutPoint" /> with in <paramref name="source" /> . </param>
    /// <returns> The updated <paramref name="source" /> tree. </returns>
    private Chromosome TreeCombine(Chromosome source, Chromosome cutPoint, Chromosome insertionMaterial) {
      if (ReferenceEquals(source, cutPoint)) {
        return new Chromosome { Node = insertionMaterial.Node, Parent = source.Parent };
      }

      var terminal = source.Node as TerminalExpression;
      if (terminal != null) {
        return source;
      }

      var branch = source.Node as BranchExpression;
      if (branch != null) {
        branch.TrueBranch = TreeCombine(branch.TrueBranch, cutPoint, insertionMaterial);
        branch.FalseBranch = TreeCombine(branch.FalseBranch, cutPoint, insertionMaterial);
        // Apply mutation
        if (branch.TrueBranch.Node is TerminalExpression) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.TrueBranch = InstantiateTerminal(GetRandomTerminal(), source);
          }
        }
        if (branch.FalseBranch.Node is TerminalExpression) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.FalseBranch = InstantiateTerminal(GetRandomTerminal(), source);
          }
        }
        return source;
      }

      var sequence = source.Node as SequenceExpression;
      if (sequence != null) {
        for (var index = 0; index < sequence.Expressions.Length; ++index) {
          sequence.Expressions[index] = TreeCombine(sequence.Expressions[index], cutPoint, insertionMaterial);
        }
        if (sequence.Expressions.Length > 1 && _random.Next(0, 100) < MutationRate) {
          var point = _random.Next(0, sequence.Expressions.Length - 1);
          var temp = sequence.Expressions[point];
          sequence.Expressions[point] = sequence.Expressions[point + 1];
          sequence.Expressions[point + 1] = temp;
        }
        return source;
      }

      throw new UnknownGenotypeException("An unknown Genotype was encountered.");
    }
  }
}