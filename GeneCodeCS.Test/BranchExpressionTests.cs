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
using System.Reflection.Emit;
using GeneCodeCS.Genetics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class BranchExpressionTests
  {
    [TestMethod]
    public void BranchMethod_ToString_Returns_branched_method_structure() {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var methods = GetType().GetMethods(flags);
      var branchMethod = methods.First(m => m.DeclaringType == GetType() && m.ReturnType == typeof(bool));
      var terminalMethod = methods.First(m => m.DeclaringType == GetType() && m.ReturnType == typeof(void));

      var branch = new BranchExpression(branchMethod, new Chromosome { Node = new TerminalExpression(terminalMethod) },
                                        new Chromosome { Node = new TerminalExpression(terminalMethod) });
      var output = branch.ToString();
      var expected = string.Format("{0}(){1}T {2}(){1}F {2}()", branchMethod.Name, Environment.NewLine,
                                   terminalMethod.Name);
      Assert.AreEqual(expected, output);
    }

    [TestMethod]
    public void EqualsObj_xEqualsNull_returnsFalse() {
      var x = new BranchExpression(new DynamicMethod("Test", typeof(bool), null), new Chromosome(), new Chromosome());
      Assert.IsFalse(x.Equals((object)null));
    }

    [TestMethod]
    public void EqualsObj_xEqualsx_returnsTrue() {
      var x = new BranchExpression(new DynamicMethod("Test", typeof(bool), null), new Chromosome(), new Chromosome());
      Assert.IsTrue(x.Equals((object)x));
    }

    [TestMethod]
    public void EqualsObj_xEqualsy_and_yEqualsx_returns_xEqualsz() {
      var x = new BranchExpression(new DynamicMethod("Test", typeof(bool), null), new Chromosome(), new Chromosome());
      var y = new BranchExpression(new DynamicMethod("Test2", typeof(bool), null), new Chromosome(), new Chromosome());
      var z = new BranchExpression(new DynamicMethod("Test3", typeof(bool), null), new Chromosome(), new Chromosome());
      Assert.IsTrue(!(x.Equals((object)y) && y.Equals((object)z)) || x.Equals((object)z));
    }

    [TestMethod]
    public void EqualsObj_xEqualsy_returns_yEqualsx() {
      var x = new BranchExpression(new DynamicMethod("Test", typeof(bool), null), new Chromosome(), new Chromosome());
      var y = new BranchExpression(new DynamicMethod("Test2", typeof(bool), null), new Chromosome(), new Chromosome());
      Assert.IsTrue(x.Equals((object)y) == y.Equals((object)x));
    }

    public bool TestBranchMethod() {
      return true;
    }
  }
}