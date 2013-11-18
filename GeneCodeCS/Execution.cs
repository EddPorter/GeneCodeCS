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
using System.Threading.Tasks;
using Common.Logging;
using GeneCodeCS.Genetics;
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class runs generated bot code and evaluates its performance against a given criteria.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. It must inherit from <see cref="BaseBot" /> . Its public void-type (actions) and bool-type (decision points) methods will be used to construct the genes for the bot; their parameters must derive from <see
  ///    cref="IParameter{T}" /> . See the <see cref="BaseBot" /> documentation for restrictions and requirements of implementing this class type. </typeparam>
  internal sealed class Execution<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///   Initialises a new instance of the <see cref="Execution{TBot}" /> class.
    /// </summary>
    public Execution() { _log.Trace("ctor: Constructing class."); }

    /// <summary>
    ///   Executes a collection of bots in parallel.
    /// </summary>
    /// <param name="bots"> The bots to execute. </param>
    /// <param name="parameters"> A custom value that is passed to each bot's <c>Initialise</c> method. </param>
    public void Run(IEnumerable<BotInformation<TBot>> bots, object parameters) {
      if (bots == null) {
        throw new ArgumentNullException("bots", Resources.ValidBotCollectionRequired);
      }

      Parallel.ForEach(bots, bot => Run(bot, parameters));
    }

    /// <summary>
    ///   Executes a bot by first initialising it and then calling its <c>Execute</c> method.
    /// </summary>
    /// <param name="bot"> The bot to execute. If an uncaught exception is thrown during execution, it is stored in the bot information class. </param>
    /// <param name="parameters"> A custom value that is passed to the bot's <c>Initialise</c> method. </param>
    public void Run(BotInformation<TBot> bot, object parameters) {
      if (bot == null) {
        throw new ArgumentNullException("bot", Resources.ValidBotRequired);
      }

      try {
        bot.Bot.Initialise(parameters);
        bot.Bot.Execute();
        bot.ExecutionException = null;
        _log.InfoFormat("Bot '{0}' completed execution with fitness {1}.", bot.Name, bot.Bot.Fitness);
      } catch (Exception ex) {
        bot.ExecutionException = ex;
        _log.WarnFormat("Bot '{0}' threw an exception: {1}{2}{3}", bot.Name, ex, Environment.NewLine, ex.StackTrace);
      }
    }
  }
}