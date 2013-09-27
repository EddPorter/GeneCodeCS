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
    /// <remarks>Not null.</remarks>
    private readonly ILog _log;

    /// <summary>
    ///   A list of non-terminal genes, formed from the branch methods found in the <typeparamref name="TBot" /> type and constructed sequence genes.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly List<IGene> _nonTerminals = new List<IGene>();

    /// <summary>
    ///   A list of the terminal methods found in the <typeparamref name="TBot" /> type.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private readonly List<TerminalGene> _terminals = new List<TerminalGene>();

    /// <summary>
    ///   The mutation rate (chance) to use during breeding (0-100). Defaults to 5.
    /// </summary>
    private int _mutationRate = 5;

    /// <summary>
    ///   A class for generating random numbers.
    /// </summary>
    /// <remarks>Not null.</remarks>
    private Random _random;

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.Reproduction`1" /> class. The provided <typeparamref
    ///    name="TBot" /> type is inspected and the qualifying terminal and non-terminal methods are extracted and stored.
    /// </summary>
    /// <param name="log"> An instance of an <see cref="T:Common.Logging.ILog" /> interface. This is used to log the status of the population during simulation. </param>
    /// <param name="randomSeed"> A value to use to seed the randomiser used within the class. This allows for reproducibility if required. If 0, then the current time is used to seed the randomiser. </param>
    internal Reproduction(ILog log, int randomSeed = 0) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Reproduction`1.ctor: Constructing class.");

      _log = log;

      Initialise(randomSeed);
    }

    /// <summary>
    ///   Gets or sets the mutation rate (chance) to use during breeding (0-100). Defaults to 5.
    /// </summary>
    internal int MutationRate {
      get { return _mutationRate; }
      set {
        if (_mutationRate < 0 || _mutationRate > 100) {
          throw new ArgumentOutOfRangeException("value", value, Resources.MutationRateAllowedRange);
        }
        _log.TraceFormat("GeneCodeCS.Reproduction`1.get_MutationRate: Mutation rate set to {0}.", value);
        _mutationRate = value;
      }
    }

    internal List<BotInformation> BreedGeneration(int generationNumber, List<BotReport> latestGenerationReports,
                                                  int botsPerGeneration, int maxTreeDepth,
                                                  ChromosomeOptimiser optimise = null) {
      var generationInformation = new List<BotInformation>();
      if (generationNumber == 0) {
        // First generation, no breeding is required.
        if (latestGenerationReports != null) {
          generationInformation.AddRange(latestGenerationReports.Select(report => report.Bot));
        }
        PopulateGenerationWithUniqueBots(generationInformation, generationNumber, botsPerGeneration, maxTreeDepth,
                                         optimise);
      } else {
        // Take the top 10% of bots and copy them verbatim.
        var tenPercent = botsPerGeneration / 10;
        generationInformation.AddRange(latestGenerationReports.Take(tenPercent).Select(report => report.Bot));

        // Generate 10% of the new population as random bots.
        PopulateGenerationWithUniqueBots(generationInformation, generationNumber, 2 * tenPercent, maxTreeDepth, optimise);

        // Select two parents and breed two children until this generation is full.
        var n = generationInformation.Count - 1;
        while (generationInformation.Count < botsPerGeneration) {
          var parents = SelectParents(latestGenerationReports);

          var parent1Tree = parents[0].Tree.Clone();

          var parent2Tree = parents[1].Tree.Clone();

          _log.Trace(string.Format("Breeding bots {0} and {1}", parents[0].Name, parents[1].Name));

          var children = CrossoverAndMutate(parent1Tree, parent2Tree);

          var child1 = children[0];

          var child2 = children[1];

          if (optimise != null) {
            child1 = optimise(child1);
            child2 = optimise(child2);
          }

          if (generationInformation.All(b => b.Tree != child1)) {
            var childResult1 = new BotInformation(CreateBotName(generationNumber, ++n), child1, parents[0].Name,
                                                  parents[1].Name);

            generationInformation.Add(childResult1);
          }

          if (generationInformation.All(b => b.Tree != child2)) {
            var childResult2 = new BotInformation(CreateBotName(generationNumber, ++n), child2, parents[1].Name,
                                                  parents[0].Name);

            generationInformation.Add(childResult2);
          }
        }
      }
      return generationInformation;
    }

    internal BotInformation CreateBot(int maxTreeDepth, ChromosomeOptimiser optimise = null) {
      var tree = CreateRandomExpressionTree(maxTreeDepth);
      if (optimise != null) {
        _log.Trace("Optimising bot tree.");
        tree = optimise(tree);
      }
      var botName = string.Format("Bot{0:X}", tree.GetHashCode());
      _log.Info(botName);
      _log.Info(tree.ToString());

      return new BotInformation(botName, tree);
    }

    /// <summary>
    ///   Constructs the name of a bot given its type, the generation it is in and its index in that generation.
    /// </summary>
    /// <param name="generation"> The generation number that the bot is in. </param>
    /// <param name="botIndex"> The index of the bot in its generation. </param>
    /// <returns> A name for the bot. </returns>
    private static string CreateBotName(int generation, int botIndex) {
      if (generation < 0) {
        throw new ArgumentOutOfRangeException("generation");
      }
      if (botIndex < 0) {
        throw new ArgumentOutOfRangeException("botIndex");
      }

      return string.Format(Resources.BotNameStringFormat, typeof(TBot).Name, generation, botIndex);
    }

    private Chromosome CreateRandomExpressionTree(int maxTreeDepth, Chromosome parent = null) {
      var node = maxTreeDepth > 0 ? GetRandomFunctionOrTerminal() : GetRandomTerminal();

      if (node is BranchGene) {
        return InstantiateBranch(maxTreeDepth, node as BranchGene, parent);
      }
      if (node is SequenceGene) {
        return InstantiateSequence(maxTreeDepth, node as SequenceGene, parent);
      }
      if (node is TerminalGene) {
        return InstantiateTerminal(node as TerminalGene, parent);
      }

      throw new InvalidOperationException();
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

    private Chromosome InstantiateBranch(int maxTreeDepth, BranchGene branchGene, Chromosome parent) {
      var methodParameters = branchGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var newTree = new Chromosome();

      var trueBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      var falseBranch = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);

      var branchInstance = new BranchExpression(branchGene.MethodInfo, parameters, trueBranch, falseBranch);
      newTree.Node = branchInstance;
      newTree.Parent = parent;

      return newTree;
    }

    private Chromosome InstantiateSequence(int maxTreeDepth, SequenceGene sequenceGene, Chromosome parent) {
      var newTree = new Chromosome { Parent = parent };
      var sequenceCount = sequenceGene.Length;
      var expressions = new Chromosome[sequenceCount];
      for (var n = 0; n < sequenceCount; ++n) {
        expressions[n] = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      }
      var sequenceInstance = new SequenceExpression(expressions);
      newTree.Node = sequenceInstance;
      return newTree;
    }

    private Chromosome InstantiateTerminal(TerminalGene terminalGene, Chromosome parent) {
      var methodParameters = terminalGene.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var terminalInstance = new TerminalExpression(terminalGene.MethodInfo, parameters);

      return new Chromosome { Node = terminalInstance, Parent = parent };
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