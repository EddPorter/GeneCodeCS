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
using System.Linq.Expressions;
using System.Reflection;
using Common.Logging;
using GeneCodeCS.Genetics;
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class converts Chromosomes into executable delegates by building and compiling expression trees in memory.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see cref="BaseBot" /> and have a public parameterless constructor. Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="IParameter{T}" /> . See the <see cref="BaseBot" /> documentation for restrictions and requirements of implementing this class type. </typeparam>
  internal sealed class Compilation<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log = LogManager.GetLogger(typeof(Compilation<TBot>));

    /// <summary>
    ///   Initialises a new instance of the <see cref="Compilation{TBot}" /> class.
    /// </summary>
    public Compilation() { _log.Trace("ctor: Constructing class."); }

    /// <summary>
    ///   Compiles a bot's chromosome and creates the bot instance that will run it.
    /// </summary>
    /// <param name="bot"> The bot to compile. The bot instance is added in-place to this object. </param>
    /// <returns> If the compilation was successful, returns <c>true</c> ; otherwise returns <c>false</c> . </returns>
    public bool CompileBot(BotInformation<TBot> bot) {
      if (bot == null || string.IsNullOrWhiteSpace(bot.Name) || bot.Tree == null) {
        throw new ArgumentNullException("bot", Resources.ValidBotRequired);
      }

      return CompileBots(new List<BotInformation<TBot>> { bot });
    }

    /// <summary>
    ///   Compiles the chromosome of each bot into a delegate and creates the bot instances that will run them.
    /// </summary>
    /// <param name="bots"> The bots to compile. The bot instances are added in-place to the bot objects in this list. </param>
    /// <returns> If the compilation was successful, returns <c>true</c> ; otherwise returns <c>false</c> . </returns>
    public bool CompileBots(IList<BotInformation<TBot>> bots) {
      if (bots == null || bots.Count == 0 || bots.Any(b => string.IsNullOrWhiteSpace(b.Name) || b.Tree == null)) {
        throw new ArgumentNullException("bots", Resources.ValidBotCollectionRequired);
      }

      _log.TraceFormat("CompileBots: Compiling {0} bots.", bots.Count);

      foreach (var bot in bots) {
        try {
          var logic = BuildBotLogic(bot.Tree).Compile();
          var instance = Activator.CreateInstance<TBot>();
          instance.SetLogic(() => logic(instance));
          bot.Bot = instance;
        } catch (Exception ex) {
          _log.ErrorFormat("Errors compiling bot '{0}': {1}", ex, bot.Name, ex.Message);
          return false;
        }
      }
      return true;
    }

    /// <summary>
    ///   Builds the bot's main loop: <c>while (bot.NotFinished()) { /* execute the Chromosome instructions */ }</c> .
    /// </summary>
    /// <param name="chromosome"> The instruction tree to convert. </param>
    /// <returns> A lambda that runs the instructions against a bot instance. </returns>
    private static Expression<Action<TBot>> BuildBotLogic(Chromosome chromosome) {
      Debug.Assert(chromosome != null, "A valid chromosome must be provided.");

      var bot = Expression.Parameter(typeof(TBot), "bot");
      var exit = Expression.Label("exit");
      var loop = Expression.Loop(Expression.IfThenElse(Expression.Call(bot, BaseBot.NotFinishedMethod),
                                                       BuildGeneExpression(bot, chromosome), Expression.Break(exit)), exit);
      return Expression.Lambda<Action<TBot>>(loop, bot);
    }

    /// <summary>
    ///   Converts the genes within the <paramref name="chromosome" /> into an expression.
    /// </summary>
    /// <param name="bot"> The parameter expression holding the bot instance on which to invoke the gene methods. </param>
    /// <param name="chromosome"> The chromosome to interpret. </param>
    /// <returns> An expression of type <c>void</c> . </returns>
    private static Expression BuildGeneExpression(ParameterExpression bot, Chromosome chromosome) {
      Debug.Assert(chromosome != null, "A valid chromosome must be provided.");
      Debug.Assert(chromosome.Node != null, "The chromosome must contain a valid genotype.");

      var node = chromosome.Node;

      var branch = node as BranchExpression;
      if (branch != null) {
        // if (bot.Method(params)) { /* true branch */ } else { /* false branch */ }
        var condition = BuildMethodCall(bot, branch.MethodInfo, branch.Parameters);
        return Expression.IfThenElse(condition, BuildGeneExpression(bot, branch.TrueBranch),
                                     BuildGeneExpression(bot, branch.FalseBranch));
      }

      var sequence = node as SequenceExpression;
      if (sequence != null) {
        var statements = sequence.Expressions.Select(e => BuildGeneExpression(bot, e)).ToList();
        return statements.Count == 0 ? (Expression)Expression.Empty() : Expression.Block(typeof(void), statements);
      }

      var terminal = node as TerminalExpression;
      if (terminal != null) {
        // bot.Method(params);
        return Expression.Block(typeof(void), BuildMethodCall(bot, terminal.MethodInfo, terminal.Parameters));
      }

      throw new UnknownGenotypeException("An unknown Genotype was encountered.");
    }

    /// <summary>
    ///   Builds a call of a gene's method on the bot, constructing each <see cref="IParameter{T}" /> from its stored value.
    /// </summary>
    private static MethodCallExpression BuildMethodCall(Expression bot, MethodInfo method, IList<object> values) {
      Debug.Assert(method != null, "A valid method must be provided.");
      Debug.Assert(values != null, "A valid list of parameter values must be provided");

      var parameters = method.GetParameters();
      var arguments = new Expression[parameters.Length];
      for (var index = 0; index < parameters.Length; ++index) {
        arguments[index] = index < values.Count
                             ? BuildParameter(parameters[index].ParameterType, values[index])
                             : Expression.Constant(parameters[index].DefaultValue, parameters[index].ParameterType);
      }
      return Expression.Call(bot, method, arguments);
    }

    /// <summary>
    ///   Builds an expression that creates an <see cref="IParameter{T}" /> of the given type, passing it the stored value.
    /// </summary>
    private static Expression BuildParameter(Type parameterType, object value) {
      // The value constructor is the single-argument constructor accepting the stored value (not the Random one).
      var constructor = parameterType.GetConstructors()
                                     .Where(c => c.GetParameters().Length == 1)
                                     .FirstOrDefault(c => c.GetParameters()[0].ParameterType.IsInstanceOfType(value));
      if (constructor == null) {
        throw new BotCompileException(string.Format("{0} has no public constructor accepting a {1}.",
                                                    parameterType.Name, value == null ? "null" : value.GetType().Name));
      }
      return Expression.New(constructor, Expression.Constant(value, constructor.GetParameters()[0].ParameterType));
    }
  }
}
