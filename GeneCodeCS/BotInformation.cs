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

namespace GeneCodeCS
{
  public sealed class BotInformation
  {
    public BotInformation(string name, Chromosome tree, string parent1 = null, string parent2 = null) {
      if (string.IsNullOrEmpty(name)) {
        throw new ArgumentNullException("name");
      }
      if (tree == null) {
        throw new ArgumentNullException("tree");
      }

      Name = name;
      Tree = tree;
      Parent1 = parent1 ?? string.Empty;
      Parent2 = parent2 ?? string.Empty;
    }

    /// <remarks>Not null.</remarks>
    public string Name { get; private set; }

    /// <remarks>Not null.</remarks>
    public string Parent1 { get; private set; }

    /// <remarks>Not null.</remarks>
    public string Parent2 { get; private set; }

    /// <remarks>Not null.</remarks>
    public Chromosome Tree { get; private set; }
  }
}