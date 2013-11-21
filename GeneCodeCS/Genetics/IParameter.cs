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
  ///   An interface for specifying parameters to bot methods. All bot methods must use the <code>IParameter</code> interface for parameter types and implement a constructor taking a <code>Random</code> object to generate a value returned through the <code>Value</code> property. An additional constructor taking a parameter of type <code>T</code> must also be provided that simply sets the <code>Value</code> property to this value.
  /// </summary>
  /// <remarks>
  ///   This interface is required so that the generator code can randomly create a value during generation and reuse that same value during code output and execution. With this interface, the library user is in control of both the type of the parameter and the acceptable ranges for it. All usages and assumptions regarding the <code>IParameter</code> type are performed at run-time so misuse will not be detected at the compilation stage.
  /// </remarks>
  /// <typeparam name="T"> The type of the parameter being represented. </typeparam>
  public interface IParameter<T>
  {
    /// <summary>
    ///   Returns the value for the represented parameter.
    /// </summary>
    /// <remarks>
    ///   The accessor is called dynamically using reflection.
    /// </remarks>
    T Value { get; }
  }
}