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
  ///   The definition of the Branch gene, which allows a boolean split in code execution depending on the result of evaluating the gene's method.
  /// </summary>
  internal class BranchGene : IGene
  {
    public BranchGene(MethodInfo mi) {
      if (mi == null) {
        throw new ArgumentNullException("mi");
      }
      if (mi.ReturnType != typeof(bool)) {
        throw new ArgumentException(Resources.MethodMustReturnBool, "mi");
      }
      // TODO: Test that each parameter inherits from some form of IParameter.
      // Or get rid of the generic parameters in IParameter. Are they even needed for correct working?
      if (
        mi.GetParameters().Any(
          p =>
          !p.ParameterType.GetInterfaces().Any(
            x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IParameter<>)))) {
        throw new ArgumentException(Resources.MethodParametersMustDeriveFromIParameter, "mi");
      }

      MethodInfo = mi;
    }

    /// <summary>
    ///   Gets the method used by this <see cref="T:GeneCodeCS.Genetics.BranchGene" /> object. It must return a boolean value and have parameters derived from <see
    ///    cref="T:GeneCodeCS.Genetics.IParameter`1" /> .
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public MethodInfo MethodInfo { get; private set; }
  }
}