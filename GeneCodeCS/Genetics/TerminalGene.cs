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
using System.Linq;
using System.Reflection;
using GeneCodeCS.Properties;

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   The gene definition for a termianl gene, i.e. action taker.
  /// </summary>
  internal class TerminalGene : IGene
  {
    /// <summary>
    ///   Creates a new <see cref="TerminalGene" /> instance to represent terminal node methods in the chromosomnal tree.
    /// </summary>
    /// <param name="mi"> The action method this gene represents. </param>
    public TerminalGene(MethodInfo mi) {
      if (mi == null) {
        throw new ArgumentNullException("mi", Resources.MethodInfoRequired);
      }
      if (mi.ReturnType != typeof(void)) {
        throw new ArgumentException(Resources.MethodMustReturnVoid, "mi");
      }
      // Check that all parameters derive from IParameter<> or at least have a default value.
      if (
        mi.GetParameters().Any(
          p =>
          !p.ParameterType.GetInterfaces().Any(
            x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IParameter<>)) && p.DefaultValue == null)) {
        throw new ArgumentException(Resources.MethodParametersMustDeriveFromIParameter, "mi");
      }

      MethodInfo = mi;
    }

    /// <summary>
    ///   Gets the method info representing this terminal.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public MethodInfo MethodInfo { get; private set; }
  }
}