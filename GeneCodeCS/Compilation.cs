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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;
using GeneCodeCS.Genetics;
using GeneCodeCS.Properties;
using Microsoft.CSharp;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class converts Chromosomes into C# code and then compiles them into an assembly.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot and have a constructor taking a single <see
  ///    cref="Common.Logging.ILog" /> parameter. </typeparam>
  internal sealed class Compilation<TBot> where TBot : BaseBot
  {
    // TODO: Allow specification of source code and DLL output destinations.

    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log;

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.Compilation`1" /> class.
    /// </summary>
    /// <param name="log"> An instance of an <see cref="T:Common.Logging.ILog" /> interface. This is used to log the status of the compilation process. </param>
    public Compilation(ILog log) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Compilation`1.ctor: Constructing class.");

      _log = log;
    }

    /// <summary>
    ///   Converts a bot into C# code and compiles it into an assembly. The code and assembly file names are constructed from the <typeparamref
    ///    name="TBot" /> class name and the <paramref name="generationNumber" /> .
    /// </summary>
    /// <param name="bot"> The bot to compile. </param>
    /// <param name="generationNumber"> The generation number of the bot. This is used to create the file name and namespace for the compiled bot. </param>
    /// <returns> The in-memory bot code. </returns>
    public TBot CompileBot(BotInformation bot, int generationNumber) {
      return CompileBots(new List<BotInformation> { bot }, generationNumber).First();
    }

    /// <summary>
    ///   Takes a list of bots, converts them into C# code, and compiles into an assembly. The code and assembly file names are constructed from the <typeparamref
    ///    name="TBot" /> class name and the <paramref name="generationNumber" /> .
    /// </summary>
    /// <param name="bots"> The bots to compile. </param>
    /// <param name="generationNumber"> The generation number of the bots. This is used to create the file names and namespace for the compiled bots. </param>
    /// <returns> A list of the compiled bot code in memory. </returns>
    public IList<TBot> CompileBots(IList<BotInformation> bots, int generationNumber) {
      if (bots == null || bots.Count == 0) {
        throw new ArgumentNullException("bots", Resources.ValidBotCollectionRequired);
      }
      if (generationNumber < 0) {
        throw new ArgumentNullException("generationNumber", Resources.GenerationNumberValidRange);
      }

      _log.TraceFormat("GeneCodeCS.Compilation`1.CompileBots: Compiling {0} bots in generation {1}.", bots.Count,
                       generationNumber);

      var namespaceName = string.Format("{0}{1:N}", typeof(TBot).Namespace, generationNumber);
      _log.InfoFormat("Compiling into namespace {0}.", namespaceName);

      _log.Trace("GeneCodeCS.Compilation`1.CompileBots: Generating code.");
      var codeCompileUnit = BuildCompileUnit(bots, namespaceName);
      GenerateCode(namespaceName, codeCompileUnit);

      _log.Trace("GeneCodeCS.Compilation`1.CompileBots: Compiling code.");
      var compiledBots = CompileCode(namespaceName);
      if (compiledBots == null) {
        return new List<TBot>();
      }

      _log.Trace("GeneCodeCS.Compilation`1.CompileBots: Creating bots from assembly.");
      var types = compiledBots.CompiledAssembly.GetTypes();
      return types.Select(t => {
                            var bot = (TBot)Activator.CreateInstance(t, _log);
                            bot.FitnessReport = new BotReport(bots.Single(bi => bi.Name == t.Name));
                            return bot;
                          }).ToList();
    }

    /// <summary>
    ///   Builds the class container for the bot. Essentially this is the bot itself: it contains the constructor and an implementation of the bot's <paramref
    ///    name="chromosome" /> . The <typeparamref name="TBot" /> type is used as the base class.
    /// </summary>
    /// <param name="name"> The name of the class to create. </param>
    /// <param name="chromosome"> The <see cref="T:GeneCodeCS.Genetics.Chromosome" /> to encapsulate in the class's <c>RunBotLogic</c> method. </param>
    /// <returns> The class declaration. </returns>
    private static CodeTypeDeclaration BuildClass(string name, Chromosome chromosome) {
      Debug.Assert(!string.IsNullOrWhiteSpace(name), "A name for the class must be specified.");
      Debug.Assert(chromosome != null, "A valid Chromosome must be provided to build into a class.");

      var codeType = new CodeTypeDeclaration(name);
      codeType.BaseTypes.Add(typeof(TBot).Name);
      codeType.Members.Add(BuildConstructor());
      codeType.Members.Add(BuildRunBotLogicMethod(chromosome));
      return codeType;
    }

    /// <summary>
    ///   Converts bots into code compilation units within a specified <paramref name="namespace" /> .
    /// </summary>
    /// <param name="bots"> The bots to compile. </param>
    /// <param name="namespace"> The namespace in which to place the code. </param>
    /// <returns> The compilation unit for the bots. </returns>
    private static CodeCompileUnit BuildCompileUnit(IEnumerable<BotInformation> bots, string @namespace) {
      Debug.Assert(bots != null, Resources.ValidBotCollectionRequired);
      Debug.Assert(!string.IsNullOrWhiteSpace(@namespace), "A valid namespace name must be provided.");

      var codeCompileUnit = new CodeCompileUnit();
      codeCompileUnit.ReferencedAssemblies.Add(typeof(TBot).Assembly.FullName);
      codeCompileUnit.ReferencedAssemblies.Add(typeof(ILog).Assembly.FullName);

      // namespace Namespace {
      var namespaces = new CodeNamespace(@namespace);
      //   using TBot.Namespace;
      namespaces.Imports.Add(new CodeNamespaceImport(typeof(TBot).Namespace));
      //   using Common.Logging;
      namespaces.Imports.Add(new CodeNamespaceImport("Common.Logging"));

      foreach (var botType in bots.Select(b => BuildClass(b.Name, b.Tree))) {
        // public class Bot {
        namespaces.Types.Add(botType);
        // }
      }
      // }

      codeCompileUnit.Namespaces.Add(namespaces);
      return codeCompileUnit;
    }

    /// <summary>
    ///   Builds the constructor for the bot. It will take a single argument of type <see name="T:Common.Logging.ILog" /> and pass it to the base class.
    /// </summary>
    /// <returns> The code member for the constructor method. </returns>
    private static CodeMemberMethod BuildConstructor() {
      var constructor = new CodeConstructor { Attributes = MemberAttributes.Public };
      constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ILog), "log"));
      constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("log"));
      return constructor;
    }

    /// <summary>
    ///   Converts the genes within the <paramref name="chromosome" /> into code statements.
    /// </summary>
    /// <param name="chromosome"> The chromosome to interpret. </param>
    /// <returns> An array of code statements. </returns>
    private static CodeStatement[] BuildGeneStatements(Chromosome chromosome) {
      Debug.Assert(chromosome != null, "A valid chromosome must be provided.");
      Debug.Assert(chromosome.Node != null, "The chromosome must contain a valid genotype.");

      var codeStatements = new CodeStatementCollection();
      var node = chromosome.Node;

      // The while loop acts as a control statement that can be broken out of once the correct expression type has
      // finished processing. If the bottom of the while loop is reached, then an unknown IGene implementation has been
      // encountered, so an exception is thrown. This method, whilst the same as, means a goto statement doesn't need
      // to be used, nor does evaluation need to continue once the correct IGene implementation has been processed.
      while (true) {
        var branch = node as BranchExpression;
        if (branch != null) {
          // if (this.Method(params)) {
          var ifCondition = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), branch.MethodInfo.Name,
                                                           BuildParametersArray(branch.MethodInfo.GetParameters(),
                                                                                branch.Parameters));
          //   // execute true branch
          var ifTrue = BuildGeneStatements(branch.TrueBranch);
          // } else {
          //   // execute false branch
          var ifFalse = BuildGeneStatements(branch.FalseBranch);
          // }

          var ifStatement = new CodeConditionStatement(ifCondition, ifTrue, ifFalse);
          codeStatements.Add(ifStatement);
          break;
        }

        var sequence = node as SequenceExpression;
        if (sequence != null) {
          foreach (var expression in sequence.Expressions) {
            codeStatements.AddRange(BuildGeneStatements(expression));
          }
          break;
        }

        var terminal = node as TerminalExpression;
        if (terminal != null) {
          // this.Method(params);
          var expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), terminal.MethodInfo.Name,
                                                          BuildParametersArray(terminal.MethodInfo.GetParameters(),
                                                                               terminal.Parameters));
          codeStatements.Add(expression);
          break;
        }

        throw new UnknownGenotypeException("An unknown Genotype was encountered.");
      }

      var statementArray = new CodeStatement[codeStatements.Count];
      codeStatements.CopyTo(statementArray, 0);
      return statementArray;
    }

    /// <summary>
    ///   Converts an array of parameter <see name="T:System.Reflection.ParameterInfo" /> and corresponding entries in an array of <paramref
    ///    name="values" /> into an array of <see cref="T:System.CodeDom.CodeExpression" /> s that can be passed to a <see
    ///    cref="T:System.CodeDom.CodeMethodInvokeExpression" /> .
    /// </summary>
    /// <remarks>
    ///   The arrays <paramref name="parameterInfo" /> and <paramref name="values" /> must contain the same number of entries.
    /// </remarks>
    /// <param name="parameterInfo"> An array of information describing the parameters. </param>
    /// <param name="values"> The values that each parameter will take. </param>
    /// <returns> An array of code expressions. </returns>
    private static CodeExpression[] BuildParametersArray(IList<ParameterInfo> parameterInfo, IList<object> values) {
      Debug.Assert(parameterInfo != null, "A valid list of parameters must be provided.");
      Debug.Assert(values != null, "A valid list of parameter values must be provided");
      Debug.Assert(parameterInfo.Count == values.Count, "Array lengths not equal.");

      var parameterCollection = new CodeExpressionCollection();
      for (var index = 0; index < parameterInfo.Count; ++index) {
        var expression = new CodeObjectCreateExpression(new CodeTypeReference(parameterInfo[index].ParameterType.Name),
                                                        new CodePrimitiveExpression(values[index]));
        parameterCollection.Add(expression);
      }

      var parameters = new CodeExpression[parameterCollection.Count];
      parameterCollection.CopyTo(parameters, 0);
      return parameters;
    }

    /// <summary>
    ///   Builds the <c>RunBotLogic</c> method that will run the instructions encoded in the specified <paramref
    ///    name="chromosome" /> .
    /// </summary>
    /// <param name="chromosome"> The instruction tree to convert to code. </param>
    /// <returns> The code member for the RunBotLogic method. </returns>
    private static CodeTypeMember BuildRunBotLogicMethod(Chromosome chromosome) {
      Debug.Assert(chromosome != null, "A valid chromosome must be provided.");

      // protected override void RunBotLogic() {
      var method = new CodeMemberMethod {
                                          ReturnType = new CodeTypeReference(typeof(void)),
                                          Attributes = (MemberAttributes.Family + (int)MemberAttributes.Override),
                                          Name = "RunBotLogic"
                                        };

      //   for ( ; this.NotFinished(); ) {
      //     // execute the Chromosome instructions
      //   }
      var noop = new CodeSnippetStatement("");
      var notFinishedCall = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "NotFinished");
      var whileLoop = new CodeIterationStatement(noop, notFinishedCall, noop, BuildGeneStatements(chromosome));

      method.Statements.Add(whileLoop);
      return method;

      // }
    }

    /// <summary>
    ///   Writes the built namespace to C# code. The file is output in the working directory with the name <c>&lt;filename>.cs</c> .
    /// </summary>
    /// <param name="filename"> The name of the </param>
    /// <param name="codeUnit"> The code unit to write to C#. </param>
    private static void GenerateCode(string filename, CodeCompileUnit codeUnit) {
      Debug.Assert(!string.IsNullOrWhiteSpace(filename), "A valid destination filename must be provided.");
      Debug.Assert(codeUnit != null, "A valid code unit must be provieded.");

      const bool noAppend = false;

      var sw = new StreamWriter(string.Format("{0}.cs", filename), noAppend);
      using (var itw = new IndentedTextWriter(sw, "  ")) {
        using (var codeProvider = new CSharpCodeProvider()) {
          codeProvider.GenerateCodeFromCompileUnit(codeUnit, itw, new CodeGeneratorOptions());
        }
      }
    }

    /// <summary>
    ///   Compiles a generated C# bot code file into a DLL.
    /// </summary>
    /// <param name="filename"> The C# bot code file name. </param>
    /// <returns> The results of the compilation, including a reference to the compiled assembly. </returns>
    private CompilerResults CompileCode(string filename) {
      Debug.Assert(!string.IsNullOrWhiteSpace(filename), "A valid source filename must be provided.");

      var provider = new CSharpCodeProvider();
      var options = new CompilerParameters
                    { GenerateInMemory = true, GenerateExecutable = false, IncludeDebugInformation = true };
      options.ReferencedAssemblies.Add("System.dll");
      options.ReferencedAssemblies.Add("Common.dll");
      options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().ManifestModule.ScopeName);

      // Ensure that the assembly in which the provided TBot type is declared is included, along with any classes from
      // which it inherits (which may included generics).
      var botType = typeof(TBot);
      while (botType != null && botType != typeof(object)) {
        options.ReferencedAssemblies.Add(Assembly.GetAssembly(botType).ManifestModule.ScopeName);
        if (botType.IsGenericType) {
          foreach (var gt in botType.GetGenericArguments()) {
            options.ReferencedAssemblies.Add(Assembly.GetAssembly(gt).ManifestModule.ScopeName);
          }
        }
        botType = botType.BaseType;
      }
      options.OutputAssembly = string.Format("{0}.dll", filename);

      // Invoke compilation.
      var sourceFile = string.Format("{0}.cs", filename);
      Debug.Assert(File.Exists(sourceFile), "The code file must exist.");
      var results = provider.CompileAssemblyFromFile(options, sourceFile);

      if (results.Errors.Count <= 0) {
        _log.InfoFormat("Source {0} built successfully.", sourceFile);
        return results;
      }

      // Display compilation errors.
      _log.ErrorFormat("Errors building {0} into {1}", sourceFile, results.PathToAssembly);
      var errors = new StringBuilder();
      errors.AppendLine("Build encountered the following erros:");
      foreach (CompilerError ce in results.Errors) {
        errors.AppendLine("  {0}, ce");
      }
      _log.TraceFormat("GeneCodeCS.Compilation`1.CompileCode: {0}", errors);
      return null;
    }
  }
}