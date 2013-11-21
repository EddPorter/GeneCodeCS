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
using GeneCodeCS.Genetics;
using GeneCodeCS.Properties;

namespace GeneCodeCS
{
  /// <summary>
  ///   Provides information about a given bot, including name, definition, and heritage, as well as its genetic definition and evaluated fitness.
  /// </summary>
  public sealed class BotInformation<TBot>
  {
    /// <summary>
    ///   Constructs a new <see cref="BotInformation{TBot}" /> class.
    /// </summary>
    /// <param name="name"> The bot name. </param>
    /// <param name="tree"> The expression tree that defines the bot. </param>
    /// <param name="parent1"> The name of the bot's first parent (source). </param>
    /// <param name="parent2"> The name of the bot's second parent (donor). </param>
    public BotInformation(string name, Chromosome tree, string parent1 = null, string parent2 = null) {
      if (string.IsNullOrWhiteSpace(name)) {
        throw new ArgumentNullException("name", Resources.BotNameValidRange);
      }
      if (tree == null) {
        throw new ArgumentNullException("tree", Resources.ChromosomeRequired);
      }

      Name = name;
      Tree = tree;
      Parent1 = parent1 ?? string.Empty;
      Parent2 = parent2 ?? string.Empty;
    }

    /// <summary>
    ///   The compiled bot type, loaded into memory.
    /// </summary>
    /// <remarks>
    ///   Can be null.
    /// </remarks>
    public TBot Bot { get; internal set; }

    /// <summary>
    ///   If an exception is thrown during the execution of <see cref="Bot" /> , it is stored here.
    /// </summary>
    /// <remarks>
    ///   Can be null.
    /// </remarks>
    public Exception ExecutionException { get; internal set; }

    /// <summary>
    ///   Gets the bot's name.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public string Name { get; private set; }

    /// <summary>
    ///   Gets the name of the bot's first parent (source).
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public string Parent1 { get; private set; }

    /// <summary>
    ///   Gets the name of the bot's second parent (donor).
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public string Parent2 { get; private set; }

    /// <summary>
    ///   Gets the genetic tree for the bot.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public Chromosome Tree { get; private set; }
  }
}