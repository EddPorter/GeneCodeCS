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
using GeneCodeCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticProgrammingLib.Test
{
  [TestClass]
  public class SequenceTests
  {
    [TestMethod]
    public void Sequence_ToString_Returns_list_of_method_names() {
      const string name = "Terminal_ToString_Returns_method_name";
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var methods = typeof(TerminalTests).GetMethods(flags);
      var method = methods.First(m => m.Name == name);
      var tree = new[] {
                         new ExpressionTree { Node = new Terminal(method) },
                         new ExpressionTree { Node = new Terminal(method) }
                       };
      var sequence = new Sequence(tree);
      var output = sequence.ToString();
      var expected = string.Format("Sequence:{1}* {0}(){1}* {0}()", name, Environment.NewLine);
      Assert.AreEqual(expected, output);
    }
  }
}