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

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   The expression of a gene in a chromosome. This is an actual instance of a gene rather than just the definition (see <see
  ///    cref="IGene" /> ). It contributes to the life (running) of the bot to which it belongs.
  /// </summary>
  public interface IGeneExpression
  {
    /// <summary>
    ///   Creates a new, independent instance of the <see cref="IGeneExpression" /> , which may contain other <see
    ///    cref="IGeneExpression" /> instances.
    /// </summary>
    /// <returns> A new, memory-independent copy of this <see cref="IGeneExpression" /> . </returns>
    IGeneExpression Clone();
  }
}