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
using System.Reflection;
using Common.Logging;
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class generates new bot expression trees.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  internal sealed class BotGenerator<TBot> where TBot : BaseBot
  {
    private readonly ILog _log;
    private readonly List<IExpression> _nonTerminals = new List<IExpression>();
    private readonly Random _random;
    private readonly List<Terminal> _terminals = new List<Terminal>();

    public BotGenerator(ILog log, int randomSeed = 0) {
      _log = log;
      MutationRate = 5;

      var ticks = randomSeed == 0 ? Environment.TickCount : randomSeed;
      _log.Info(string.Format("Random seed {0}", ticks));
      _random = new Random(ticks);

      Initialise();
    }

    public int MutationRate { get; set; }

    internal List<BotReport> CreateNewGeneration(int generationNumber, List<BotReport> lastGeneration, int population,
                                                 int maxTreeDepth, Func<ExpressionTree, ExpressionTree> optimise = null) {
      var thisGeneration = new List<BotReport>();
      if (generationNumber == 0) {
        if (lastGeneration != null) {
          thisGeneration.AddRange(lastGeneration);
        }
        PopulateGenerationWithUniqueBots(thisGeneration, generationNumber, population, maxTreeDepth, optimise);
      } else {
        // Take the top 10% of bots and copy them verbatim.
        var tenPercent = population / 10;
        thisGeneration.AddRange(lastGeneration.Take(tenPercent));

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
            var childResult1 = new BotReport(CreateBotName(generationNumber, ++n), parents[0].Name, parents[1].Name,
                                             child1);
            thisGeneration.Add(childResult1);
          }
          if (thisGeneration.All(b => b.Tree != child2)) {
            var childResult2 = new BotReport(CreateBotName(generationNumber, ++n), parents[1].Name, parents[0].Name,
                                             child2);
            thisGeneration.Add(childResult2);
          }
        }
      }
      return thisGeneration;
    }

    internal BotReport CreateRandomBot(int maxTreeDepth, Func<ExpressionTree, ExpressionTree> optimise = null) {
      var tree = CreateRandomExpressionTree(maxTreeDepth);
      if (optimise != null) {
        _log.Trace("Optimising bot tree.");
        tree = optimise(tree);
      }
      var botName = string.Format("Bot{0:X}", tree.GetHashCode());
      _log.Info(botName);
      _log.Info(tree.ToString());

      return new BotReport(botName, "", "", tree);
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

    private void PopulateGenerationWithUniqueBots(ICollection<BotReport> thisGeneration, int generationNumber, int limit,
                                                  int maxTreeDepth, Func<ExpressionTree, ExpressionTree> optimise) {
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
        thisGeneration.Add(new BotReport(name, "", "", newTree));
      }
    }

    private ExpressionTree CreateRandomExpressionTree(int maxTreeDepth, ExpressionTree parent = null) {
      var node = maxTreeDepth > 0 ? GetRandomFunctionOrTerminal() : GetRandomTerminal();

      if (node is Branch) {
        return InstantiateBranch(maxTreeDepth, node, parent);
      }
      if (node is Sequence) {
        return InstantiateSequence(maxTreeDepth, node, parent);
      }
      if (node is Terminal) {
        return InstantiateTerminal(node, parent);
      }

      throw new InvalidOperationException();
    }

    private ExpressionTree[] CrossoverAndMutate(ExpressionTree parent1, ExpressionTree parent2) {
      var parent1Nodes = FlattenExpressionTree(parent1);
      var parent2Nodes = FlattenExpressionTree(parent2);
      // select a random subree from each
      var crossoverPoint1 = _random.Next(0, parent1Nodes.Count);
      var crossoverPoint2 = _random.Next(0, parent2Nodes.Count);

      var child1 = TreeCombine(parent1, parent1Nodes[crossoverPoint1], parent2Nodes[crossoverPoint2]);
      var child2 = TreeCombine(parent2, parent2Nodes[crossoverPoint2], parent1Nodes[crossoverPoint1]);

      return new[] { child1, child2 };
    }

    private List<ExpressionTree> FlattenExpressionTree(ExpressionTree tree) {
      var flat = new List<ExpressionTree> { tree };
      if (tree.Node is Branch) {
        var branch = tree.Node as Branch;
        flat.AddRange(FlattenExpressionTree(branch.Left));
        flat.AddRange(FlattenExpressionTree(branch.Right));
      } else if (tree.Node is Sequence) {
        foreach (var sequence in ((Sequence)tree.Node).Expressions) {
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

    private IExpression GetRandomFunctionOrTerminal() {
      return _nonTerminals.Union(_terminals).OrderBy(x => _random.Next()).First();
    }

    private Terminal GetRandomTerminal() {
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
          _terminals.Add(new Terminal(mi));
        }
        // Branching methods are bool methods.
        if (mi.DeclaringType != typeof(object) && mi.ReturnType == typeof(bool) && mi.IsPublic) {
          _nonTerminals.Add(new Branch(mi));
        }
      }
      //var totalExpressions = nonTerminals.Count + terminals.Count;
      for (var n = 2; n <= 6; ++n) {
        _nonTerminals.Add(new Sequence(new ExpressionTree[n]));
      }
    }

    private ExpressionTree InstantiateBranch(int maxTreeDepth, IExpression node, ExpressionTree parent) {
      var branchNode = (Branch)node;
      var methodParameters = branchNode.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var branch = Activator.CreateInstance(typeof(Branch), branchNode.MethodInfo, parameters) as Branch;

      var newTree = new ExpressionTree { Node = branch, Parent = parent };
      Debug.Assert(branch != null, "branch != null");
      branch.Left = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      branch.Right = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      return newTree;
    }

    private ExpressionTree InstantiateSequence(int maxTreeDepth, IExpression node, ExpressionTree parent) {
      var newTree = new ExpressionTree { Parent = parent };
      var sequenceCount = ((Sequence)node).Expressions.Length;
      var expressions = new ExpressionTree[sequenceCount];
      for (var n = 0; n < sequenceCount; ++n) {
        expressions[n] = CreateRandomExpressionTree(maxTreeDepth - 1, newTree);
      }
      var sequence = new Sequence(expressions);
      newTree.Node = sequence;
      return newTree;
    }

    private ExpressionTree InstantiateTerminal(IExpression node, ExpressionTree parent) {
      var terminalNode = (Terminal)node;
      var methodParameters = terminalNode.MethodInfo.GetParameters();
      var parameters = GenerateParameters(methodParameters);

      var terminal = Activator.CreateInstance(typeof(Terminal), terminalNode.MethodInfo, parameters) as Terminal;

      return new ExpressionTree { Node = terminal, Parent = parent };
    }

    private BotReport[] SelectParents(List<BotReport> lastGeneration) {
      var parents = new BotReport[2];

      parents[0] = SelectRandomParentBasedOnFitness(lastGeneration);
      parents[1] = SelectRandomParentBasedOnFitness(lastGeneration);

      return parents;
    }

    private BotReport SelectRandomParentBasedOnFitness(List<BotReport> lastGeneration) {
      // Make all fitnesses positive.
      var fitnessAdjustment = 1 - lastGeneration.Min(g => g.Fitness);
      var lastGenerationTotalFitness = lastGeneration.Sum(g => g.Fitness + fitnessAdjustment);
      var target = _random.NextDouble() * lastGenerationTotalFitness;
      var currentSumFitness = 0.0d;
      var parent = lastGeneration.SkipWhile(g => {
                                              currentSumFitness += g.Fitness + fitnessAdjustment;
                                              return currentSumFitness < target;
                                            }).First();
      return parent;
    }

    private ExpressionTree TreeCombine(ExpressionTree source, ExpressionTree cutPoint, ExpressionTree insertionMaterial) {
      var mTreeCopy = source;

      if (ReferenceEquals(source, cutPoint)) {
        mTreeCopy = ExpressionTree.ReplaceNodeWithCopy(source, insertionMaterial);
      } else if (mTreeCopy.Node is Branch) {
        var branch = mTreeCopy.Node as Branch;
        branch.Left = TreeCombine(branch.Left, cutPoint, insertionMaterial);
        branch.Right = TreeCombine(branch.Right, cutPoint, insertionMaterial);
        // Apply mutation
        if (branch.Left.Node is Terminal) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.Left = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
        if (branch.Right.Node is Terminal) {
          if (_random.Next(0, 100) < MutationRate) {
            branch.Right = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
      } else if (mTreeCopy.Node is Sequence) {
        var sequence = mTreeCopy.Node as Sequence;
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