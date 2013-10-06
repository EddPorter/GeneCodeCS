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
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class runs generated bot code and evaluates its performance against a given criteria.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  internal sealed class Execution<TBot> where TBot : BaseBot
  {
    /// <summary>
    ///   Used to log status information.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    private readonly ILog _log;

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.Execution`1" /> class.
    /// </summary>
    /// <param name="log"> An instance of an <see cref="T:Common.Logging.ILog" /> interface. This is used to log the status of the execution process. </param>
    public Execution(ILog log) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Execution`1.ctor: Constructing class.");

      _log = log;
    }

    /// <summary>
    ///   Executes a collection of bots in parallel and updates them with their calculated fitness values.
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
    ///   Executes a bot by first initialising it and then calling its <c>Execute</c> method. This will update the bot's internal fitness report.
    /// </summary>
    /// <param name="bot"> The bot to execute. </param>
    /// <param name="parameters"> A custom value that is passed to the bot's <c>Initialise</c> method. </param>
    public void Run(BotInformation<TBot> bot, object parameters) {
      if (bot == null) {
        throw new ArgumentNullException("bot", Resources.ValidBotRequired);
      }

      try {
        bot.Bot.Initialise(parameters);
        bot.Bot.Execute();
        _log.InfoFormat("Bot '{0}' completed execution with fitness {1}.", bot.Name, bot.Fitness);
      } catch (Exception ex) {
        bot.Fitness = int.MinValue;
        _log.WarnFormat("Bot '{0}' threw an exception: {1}{2}{3}", bot.Name, ex, Environment.NewLine, ex.StackTrace);
      }
    }
  }
}