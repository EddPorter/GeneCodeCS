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
  ///   This class generates new bot expression trees.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  public sealed class Reproduction<TBot> where TBot : BaseBot
  {
    #region Delegates

    public delegate Chromosome ChromosomeOptimiser(Chromosome c);

    #endregion

    private readonly ILog _log;
    private readonly List<IGene> _nonTerminals = new List<IGene>();
    private readonly Random _random;
    private readonly List<TerminalGene> _terminals = new List<TerminalGene>();

    public Reproduction(ILog log, int randomSeed = 0) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Reproduction`1: Constructing class.");

      _log = log;
      MutationRate = 5;

      var ticks = randomSeed == 0 ? Environment.TickCount : randomSeed;
      _log.Info(string.Format("Random seed {0}", ticks));
      _random = new Random(ticks);

      Initialise();
    }

    public int MutationRate { get; set; }

    internal List<BotInformation> BreedGeneration(int generationNumber, List<BotReport> lastGeneration,
                                                      int population, int maxTreeDepth,
                                                      ChromosomeOptimiser optimise = null) {
      var thisGeneration = new List<BotInformation>();
      if (generationNumber == 0) {
        if (lastGeneration != null) {
          thisGeneration.AddRange(lastGeneration.Select(report => report.Bot));
        }
        PopulateGenerationWithUniqueBots(thisGeneration, generationNumber, population, maxTreeDepth, optimise);
      } else {
        // Take the top 10% of bots and copy them verbatim.
        var tenPercent = population / 10;
        thisGeneration.AddRange(lastGeneration.Take(tenPercent).Select(report => report.Bot));

        // Generate 10% of the new population as random bots.
        PopulateGenerationWithUniqueBots(thisGeneration, generationNumber, 2 * tenPercent, maxTreeDepth, optimise);

        // Select two parents and breed two children until this generation is full.
        var n = thisGeneration.Count - 1;
        while (thisGeneration.Count < population) {
          var parents = SelectParents(lastGeneration);
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

          if (thisGeneration.All(b => b.Tree != child1)) {
            var childResult1 = new BotInformation(CreateBotName(generationNumber, ++n), child1, parents[0].Name,
                                                  parents[1].Name);
            thisGeneration.Add(childResult1);
          }
          if (thisGeneration.All(b => b.Tree != child2)) {
            var childResult2 = new BotInformation(CreateBotName(generationNumber, ++n), child2, parents[1].Name,
                                                  parents[0].Name);
            thisGeneration.Add(childResult2);
          }
        }
      }
      return thisGeneration;
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

      return new[] {child1, child2};
    }

    private List<Chromosome> FlattenExpressionTree(Chromosome tree) {
      var flat = new List<Chromosome> {tree};
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

    private void Initialise() {
      // Locate all the terminals and branching methods in the base class.
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var botClassMethods = typeof(TBot).GetMethods(flags);
      foreach (var mi in botClassMethods) {
        // Terminals are void methods.
        if (mi.DeclaringType != typeof(object) && mi.ReturnType == typeof(void) && mi.IsPublic && mi.Name != "Execute" &&
            mi.Name != "Initialise" && !mi.Name.StartsWith("set_") && !mi.Name.StartsWith("get_")) {
          _terminals.Add(new TerminalGene(mi));
        }
        // Branching methods are bool methods.
        if (mi.DeclaringType != typeof(object) && mi.ReturnType == typeof(bool) && mi.IsPublic) {
          _nonTerminals.Add(new BranchGene(mi));
        }
      }
      //var totalExpressions = nonTerminals.Count + terminals.Count;
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
      var newTree = new Chromosome {Parent = parent};
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

      return new Chromosome {Node = terminalInstance, Parent = parent};
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