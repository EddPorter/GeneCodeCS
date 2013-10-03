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

namespace GeneCodeCS
{
  /// <summary>
  /// Contains information about the fitness of a bot.
  /// </summary>
  public sealed class BotReport
  {
    /// <summary>
    /// Initialises a new <see cref="T:GeneCodeCS.BotReport"/> instance.
    /// </summary>
    /// <param name="bot">Information about the bot.</param>
    public BotReport(BotInformation bot) {
      Information = bot;
      Fitness = int.MinValue;
    }

    /// <summary>
    /// Gets custom information that can be set during bot execution.
    /// </summary>
    public object CustomInformation { get; internal set; }

    /// <summary>
    /// Gets the calculated fitness value set during bot execution.
    /// </summary>
    public int Fitness { get; internal set; }

    /// <summary>
    /// Gets information about the bot this report represents.
    /// </summary>
    public BotInformation Information { get; private set; }
  }
}