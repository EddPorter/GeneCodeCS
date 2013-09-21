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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.CSharp;

namespace GeneCodeCS
{
  internal class BotGenerator<TBot>
  {
    private readonly ILog _log;
    private readonly List<IExpression> _nonTerminals = new List<IExpression>();
    private readonly Random _random;
    private readonly List<Terminal> _terminals = new List<Terminal>();

    public BotGenerator(ILog log) {
      _log = log;

      var ticks = Environment.TickCount;
      _log.Info(string.Format("Random seed {0}", ticks));
      _random = new Random(ticks);

      Initialise();
    }

    internal List<BotReport> CreateAndRunNewGeneration(int generation, List<BotReport> lastGeneration, int population,
                                                       int maxTreeDepth, object parameters,
                                                       Func<ExpressionTree, ExpressionTree> optimise = null) {
      var thisGeneration = CreateNewGeneration(generation, lastGeneration, population, maxTreeDepth, optimise);
      return RunGeneration(generation, parameters, thisGeneration);
    }

    internal BotReport CreateAndRunSingleBot(int maxTreeDepth, object parameters,
                                             Func<ExpressionTree, ExpressionTree> optimise = null) {
      var bot = CreateRandomBot(maxTreeDepth, optimise);
      var generation = new List<BotReport> { bot };
      return RunGeneration(0, parameters, generation).FirstOrDefault();
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

    private static string CreateBotName(int generation, int populus) {
      if (generation < 0 || populus < 0) {
        throw new ArgumentOutOfRangeException();
      }
      var baseName = typeof(TBot).Name;
      var baseNamespace = typeof(TBot).Namespace;
      var name = string.Format("{0}_Gen{1}_Bot{2}", baseName.Replace(string.Format("{0}.", baseNamespace), ""),
                               generation, populus);
      return name;
    }

    private static CodeMemberMethod CreateConstructor() {
      var constructor = new CodeConstructor { Attributes = MemberAttributes.Public };
      constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ILog), "log"));
      constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("log"));
      return constructor;
    }

    private static void GenerateCode(string generatedNamespaceName, CodeCompileUnit codeCompileUnit) {
      var provider = new CSharpCodeProvider();
      using (var sw = new StreamWriter(generatedNamespaceName + ".cs", false)) {
        var tw = new IndentedTextWriter(sw, "  ");
        // Generate source code using the code generator.
        provider.GenerateCodeFromCompileUnit(codeCompileUnit, tw, new CodeGeneratorOptions());
        tw.Close();
      }
    }

    private CodeTypeDeclaration BuildClass(string name, ExpressionTree expressionTree) {
      var codeType = new CodeTypeDeclaration(name);
      codeType.BaseTypes.Add(typeof(TBot).Name);
      codeType.Members.Add(BuildRunBotLogicMethod(expressionTree));
      codeType.Members.Add(CreateConstructor());
      return codeType;
    }

    private CodeCompileUnit BuildCompileUnit(IEnumerable<BotReport> thisGeneration, string generatedNamespaceName) {
      var codeCompileUnit = new CodeCompileUnit();
      codeCompileUnit.ReferencedAssemblies.Add(typeof(TBot).Assembly.FullName);
      codeCompileUnit.ReferencedAssemblies.Add(typeof(ILog).Assembly.FullName);

      var namespaces = new CodeNamespace(generatedNamespaceName);
      namespaces.Imports.Add(new CodeNamespaceImport(typeof(TBot).Namespace));
      namespaces.Imports.Add(new CodeNamespaceImport("System"));
      namespaces.Imports.Add(new CodeNamespaceImport("Common"));

      foreach (var populusClass in thisGeneration.Select(populus => BuildClass(populus.Name, populus.Tree))) {
        namespaces.Types.Add(populusClass);
      }
      codeCompileUnit.Namespaces.Add(namespaces);

      return codeCompileUnit;
    }

    private CodeTypeMember BuildRunBotLogicMethod(ExpressionTree expressionTree) {
      var m = new CodeMemberMethod {
                                     ReturnType = new CodeTypeReference(typeof(void)),
                                     // ReSharper disable BitwiseOperatorOnEnumWihtoutFlags
                                     Attributes = MemberAttributes.Family | MemberAttributes.Override,
                                     // ReSharper restore BitwiseOperatorOnEnumWihtoutFlags
                                     Name = "RunBotLogic"
                                   };

      var statements = GenerateStatements(expressionTree);

      var loopCondition = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "NotFinished",
                                                         new CodeExpression[] { });
      var whileLoop = new CodeIterationStatement(new CodeSnippetStatement(""), loopCondition,
                                                 new CodeSnippetStatement(""), statements);
      m.Statements.Add(whileLoop);
      return m;
    }

    private CompilerResults CompileCode(string generatedNamespaceName) {
      var provider = new CSharpCodeProvider();
      var cp = new CompilerParameters
               { GenerateInMemory = true, GenerateExecutable = false, IncludeDebugInformation = true };
      cp.ReferencedAssemblies.Add("System.dll");
      cp.ReferencedAssemblies.Add("Common.dll");
      cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().ManifestModule.ScopeName);
      var t = typeof(TBot);
      while (t != null && t != typeof(object)) {
        cp.ReferencedAssemblies.Add(Assembly.GetAssembly(t).ManifestModule.ScopeName);
        if (t.IsGenericType) {
          foreach (var gt in t.GetGenericArguments()) {
            cp.ReferencedAssemblies.Add(Assembly.GetAssembly(gt).ManifestModule.ScopeName);
          }
        }
        t = t.BaseType;
      }
      cp.OutputAssembly = generatedNamespaceName + ".dll";

      // Invoke compilation.
      string sourceFile = generatedNamespaceName + ".cs";
      var cr = provider.CompileAssemblyFromFile(cp, sourceFile);

      if (cr.Errors.Count > 0) {
        // Display compilation errors.
        _log.Error(string.Format("Errors building {0} into {1}", sourceFile, cr.PathToAssembly));
        foreach (CompilerError ce in cr.Errors) {
          _log.Trace(string.Format("  {0}\n", ce));
        }
        return null;
      }
      _log.Info(string.Format("Source {0} built successfully.", sourceFile));
      return cr;
    }

    private List<BotReport> CreateNewGeneration(int generationNumber, List<BotReport> lastGeneration, int population,
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

        // Select two parents and breed two children until this generation is
        // full.
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

          if (!thisGeneration.Any(b => b.Tree == child1)) {
            var childResult1 = new BotReport(CreateBotName(generationNumber, ++n), parents[0].Name, parents[1].Name, child1);
            thisGeneration.Add(childResult1);
          }
          if (!thisGeneration.Any(b => b.Tree == child2)) {
            var childResult2 = new BotReport(CreateBotName(generationNumber, ++n), parents[1].Name, parents[0].Name, child2);
            thisGeneration.Add(childResult2);
          }
        }
      }
      return thisGeneration;
    }

    private void PopulateGenerationWithUniqueBots(ICollection<BotReport> thisGeneration, int generationNumber, int limit, int maxTreeDepth, Func<ExpressionTree, ExpressionTree> optimise) {
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
      IExpression node = maxTreeDepth > 0 ? GetRandomFunctionOrTerminal() : GetRandomTerminal();

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
      int crossoverPoint1 = _random.Next(0, parent1Nodes.Count);
      int crossoverPoint2 = _random.Next(0, parent2Nodes.Count);

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

    private CodeExpression[] GenerateParameters(ParameterInfo[] methodParameters, object[] parameterValues) {
      if (methodParameters == null) {
        throw new ArgumentNullException("methodParameters");
      }
      if (parameterValues == null) {
        throw new ArgumentNullException("parameterValues");
      }
      if (methodParameters.Length != parameterValues.Length) {
        throw new InvalidOperationException("Array lengths not equal.");
      }

      var parameterCollection = new CodeExpressionCollection();
      for (int index = 0; index < methodParameters.Length; ++index) {
        var expression =
          new CodeObjectCreateExpression(new CodeTypeReference(methodParameters[index].ParameterType.Name),
                                         new CodePrimitiveExpression(parameterValues[index]));
        parameterCollection.Add(expression);
      }
      var parameters = new CodeExpression[parameterCollection.Count];
      parameterCollection.CopyTo(parameters, 0);
      return parameters;
    }

    private object[] GenerateParameters(IEnumerable<ParameterInfo> methodParameters) {
      if (methodParameters == null) {
        throw new ArgumentNullException("methodParameters");
      }
      return
        methodParameters.Select(parameter => Activator.CreateInstance(parameter.ParameterType, _random)).Select(
          parameterInstance => ((dynamic)parameterInstance).Value).Cast<object>().ToArray();
    }

    private CodeStatement[] GenerateStatements(ExpressionTree expressionTree) {
      var codeStatements = new CodeStatementCollection();
      var node = expressionTree.Node;
      if (node is Branch) {
        var branchMethod = node as Branch;

        var ifCondition = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), branchMethod.MethodInfo.Name,
                                                         GenerateParameters(branchMethod.MethodInfo.GetParameters(),
                                                                            branchMethod.Parameters));
        var ifTrue = GenerateStatements(branchMethod.Left);
        var ifFalse = GenerateStatements(branchMethod.Right);
        var ifStatement = new CodeConditionStatement(ifCondition, ifTrue, ifFalse);
        codeStatements.Add(ifStatement);
      } else if (node is Sequence) {
        foreach (var expression in ((Sequence)node).Expressions) {
          codeStatements.AddRange(GenerateStatements(expression));
        }
      } else if (node is Terminal) {
        var terminal = node as Terminal;
        var expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), terminal.MethodInfo.Name,
                                                        GenerateParameters(terminal.MethodInfo.GetParameters(),
                                                                           terminal.Parameters));
        codeStatements.Add(expression);
      }

      var statementArray = new CodeStatement[codeStatements.Count];
      codeStatements.CopyTo(statementArray, 0);

      return statementArray;
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
      MethodInfo[] botClassMethods = typeof(TBot).GetMethods(flags);
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
      for (int n = 2; n <= 6; ++n) {
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
      for (int n = 0; n < sequenceCount; ++n) {
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

    private List<BotReport> RunGeneration(int generation, object parameters, List<BotReport> thisGeneration) {
      string generatedNamespaceName = string.Concat(typeof(TBot).Namespace,
                                                    generation.ToString(CultureInfo.InvariantCulture));

      var codeCompileUnit = BuildCompileUnit(thisGeneration, generatedNamespaceName);
      GenerateCode(generatedNamespaceName, codeCompileUnit);

      var cr = CompileCode(generatedNamespaceName);

      var a = cr.CompiledAssembly;

      // take each type in the assembly, and invoke its Execute() method
      Type[] types = a.GetTypes();
      Parallel.ForEach(types, t => {
                                var a2 = (BaseBot)Activator.CreateInstance(t, _log);

                                a2.TerminationReport = thisGeneration.Single(g => g.Name == t.Name);
                                a2.Initialise(parameters);
                                a2.Execute();
                                a2.TerminationReport.BotInstance = t;
                                _log.Info(string.Format("{0} completed execution.", t.Name));
                              });

      // return results in order, from best fitness to least fitness
      return thisGeneration.OrderByDescending(g => g.Fitness).ToList();
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
      double currentSumFitness = 0.0d;
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
          if (_random.Next(0, 100) < 5) {
            // TODO: Update how mutation occurs.
            branch.Left = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
        if (branch.Right.Node is Terminal) {
          if (_random.Next(0, 100) < 5) {
            // TODO: Update how mutation occurs.
            branch.Right = InstantiateTerminal(GetRandomTerminal(), mTreeCopy);
          }
        }
      } else if (mTreeCopy.Node is Sequence) {
        var sequence = mTreeCopy.Node as Sequence;
        for (int index = 0; index < sequence.Expressions.Length; ++index) {
          sequence.Expressions[index] = TreeCombine(sequence.Expressions[index], cutPoint, insertionMaterial);
        }
        if (sequence.Expressions.Length > 1 && _random.Next(0, 100) < 5) {
          int point = _random.Next(0, sequence.Expressions.Length - 1);
          var temp = sequence.Expressions[point];
          sequence.Expressions[point] = sequence.Expressions[point + 1];
          sequence.Expressions[point + 1] = temp;
        }
      }
      return mTreeCopy;
    }
  }
}