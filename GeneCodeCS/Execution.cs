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
    private readonly ILog _log;

    public Execution(ILog log) {
      if (log == null) {
        throw new ArgumentNullException("log", Resources.NonNullLogClassRequired);
      }

      log.Trace("GeneCodeCS.Execution`1: Constructing class.");

      _log = log;
    }

    public List<TBot> Run(IList<TBot> bots, object parameters) {
      // take each type in the assembly, and invoke its Execute() method
      Parallel.ForEach(bots, a2 => {
                               a2.Initialise(parameters);
                               a2.Execute();
                               _log.Info(string.Format("{0} completed execution.", a2.FitnessReport.Bot.Name));
                             });
      return bots.ToList();
    }
  }
}