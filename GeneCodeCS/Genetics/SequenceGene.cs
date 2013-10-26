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
using GeneCodeCS.Properties;

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   The gene definition for a sequence of n-chromosomes.
  /// </summary>
  public class SequenceGene : IGene
  {
    /// <summary>
    ///   Creates a new <see cref="SequenceGene" /> instance to represent chromosome sequences of the specified length.
    /// </summary>
    /// <param name="length"> The length of the sequence. Must be greater than 0. </param>
    public SequenceGene(int length) {
      if (length <= 0) {
        throw new ArgumentOutOfRangeException("length", Resources.SequenceLengthValidRange);
      }

      Length = length;
    }

    /// <summary>
    ///   Gets the represented length of the sequence.
    /// </summary>
    public int Length { get; private set; }
  }
}