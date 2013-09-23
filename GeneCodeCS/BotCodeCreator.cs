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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Microsoft.CSharp;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class converts bot expression trees into code.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  internal sealed class BotCodeCreator<TBot> where TBot : BaseBot
  {
    private readonly ILog _log;

    public BotCodeCreator(ILog log) {
      _log = log;
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
      var sourceFile = generatedNamespaceName + ".cs";
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

    private static CodeExpression[] GenerateParameters(ParameterInfo[] methodParameters, object[] parameterValues) {
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
      for (var index = 0; index < methodParameters.Length; ++index) {
        var expression =
          new CodeObjectCreateExpression(new CodeTypeReference(methodParameters[index].ParameterType.Name),
                                         new CodePrimitiveExpression(parameterValues[index]));
        parameterCollection.Add(expression);
      }
      var parameters = new CodeExpression[parameterCollection.Count];
      parameterCollection.CopyTo(parameters, 0);
      return parameters;
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

    public IEnumerable<TBot> CreateBotCode(int generation, IEnumerable<BotReport> thisGeneration) {
      var generatedNamespaceName = string.Concat(typeof(TBot).Namespace,
                                                 generation.ToString(CultureInfo.InvariantCulture));

      var codeCompileUnit = BuildCompileUnit(thisGeneration, generatedNamespaceName);
      GenerateCode(generatedNamespaceName, codeCompileUnit);

      var cr = CompileCode(generatedNamespaceName);

      var a = cr.CompiledAssembly;

      var types = a.GetTypes();
      return types.Select(t => (TBot)Activator.CreateInstance(t, _log));
    }
  }
}