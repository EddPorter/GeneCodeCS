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
using GeneCodeCS.Genetics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class SequenceExpressionTests
  {
    [TestMethod]
    public void EqualsObj_xEqualsNull_returnsFalse() {
      var x = new SequenceExpression(new Chromosome[2]);
      Assert.IsFalse(x.Equals((object)null));
    }

    [TestMethod]
    public void EqualsObj_xEqualsx_returnsTrue() {
      var x = new SequenceExpression(new Chromosome[2]);
      Assert.IsTrue(x.Equals((object)x));
    }

    [TestMethod]
    public void EqualsObj_xEqualsy_and_yEqualsx_returns_xEqualsz() {
      var x = new SequenceExpression(new Chromosome[2]);
      var y = new SequenceExpression(new Chromosome[2]);
      var z = new SequenceExpression(new Chromosome[2]);
      Assert.IsTrue(!(x.Equals((object)y) && y.Equals((object)z)) || x.Equals((object)z));
    }

    [TestMethod]
    public void EqualsObj_xEqualsy_returns_yEqualsx() {
      var x = new SequenceExpression(new Chromosome[2]);
      var y = new SequenceExpression(new Chromosome[2]);
      Assert.IsTrue(x.Equals((object)y) == y.Equals((object)x));
    }

    [TestMethod]
    public void Sequence_ToString_Returns_list_of_method_names() {
      const string name = "Terminal_ToString_Returns_method_name";
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var methods = typeof(TerminalExpressionTests).GetMethods(flags);
      var method = methods.First(m => m.Name == name);
      var tree = new[] {
                         new Chromosome { Node = new TerminalExpression(method) },
                         new Chromosome { Node = new TerminalExpression(method) }
                       };
      var sequence = new SequenceExpression(tree);
      var output = sequence.ToString();
      var expected = string.Format("[{1}* {0}(){1}* {0}(){1}]", name, Environment.NewLine);
      Assert.AreEqual(expected, output);
    }
  }
}