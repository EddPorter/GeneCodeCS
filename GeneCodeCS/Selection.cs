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
  ///   Defines the method to use for selection of bots to breed to create the next generation.
  /// </summary>
  public enum Selection
  {
    /// <summary>
    ///   Picks bots more often in proportion to their calculated fitness.
    /// </summary>
    FitnessProportionate,

    /// <summary>
    ///   Plays a selection of bots against each other, picking those with high fitnesses.
    /// </summary>
    Tournament
  }
}