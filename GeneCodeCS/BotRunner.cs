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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace GeneCodeCS
{
  /// <summary>
  ///   This class runs generated bot code and evaluates its performance against a given criteria.
  /// </summary>
  /// <typeparam name="TBot"> The bot class type to breed. Must inherit from BaseBot. </typeparam>
  internal sealed class BotRunner<TBot> where TBot : BaseBot
  {
    private readonly ILog _log;

    public BotRunner(ILog log) {
      _log = log;
    }

    public List<BotReport> RunGeneration(IEnumerable<TBot> bots, object parameters,
                                         IEnumerable<BotReport> thisGeneration) {
      // TODO: Sort this out as it is messed up. Why do we need `thisGeneration` and `bots`?

      // take each type in the assembly, and invoke its Execute() method
      Parallel.ForEach(bots, a2 => {
                               a2.TerminationReport = thisGeneration.Single(g => g.Name == a2.TerminationReport.Name);
                               a2.Initialise(parameters);
                               a2.Execute();
                               a2.TerminationReport.BotInstance = a2.GetType();
                               _log.Info(string.Format("{0} completed execution.", a2.TerminationReport.Name));
                             });

      // return results in order, from best fitness to least fitness
      return thisGeneration.OrderByDescending(g => g.Fitness).ToList();
    }
  }
}