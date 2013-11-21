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
using System.Reflection.Emit;
using GeneCodeCS.Genetics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class TerminalExpressionTests
  {
    [TestMethod]
    public void TerminalExpression_Constructor_with_default_parameters_Creates_object() {
      var methodInfo = GetType().GetMethod("TestMethod");
      var terminalExpression = new TerminalExpression(methodInfo, new object[] { 5 });
      Assert.IsNotNull(terminalExpression);
    }

    [TestMethod]
    public void TerminalExpression_Constructor_with_excess_parameters_Creates_object() {
      var methodInfo = new DynamicMethod("TestMethod", typeof(void), null);
      var terminalExpression = new TerminalExpression(methodInfo, new object[] { 5 });
      Assert.IsNotNull(terminalExpression);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TerminalExpression_Constructor_with_unmatched_parameters_Throws_exception() {
      var methodInfo = new DynamicMethod("TestMethod", typeof(void), new[] { typeof(TestParameter) });
      new TerminalExpression(methodInfo);
    }

    [TestMethod]
    public void TerminalExpression_EqualsObj_xEqualsNull_returnsFalse() {
      var x = new TerminalExpression(new DynamicMethod("Test", typeof(void), null));
      Assert.IsFalse(x.Equals((object)null));
    }

    [TestMethod]
    public void TerminalExpression_EqualsObj_xEqualsx_returnsTrue() {
      var x = new TerminalExpression(new DynamicMethod("Test", typeof(void), null));
      Assert.IsTrue(x.Equals((object)x));
    }

    [TestMethod]
    public void TerminalExpression_EqualsObj_xEqualsy_and_yEqualsx_returns_xEqualsz() {
      var x = new TerminalExpression(new DynamicMethod("Test", typeof(void), null));
      var y = new TerminalExpression(new DynamicMethod("Test2", typeof(void), null));
      var z = new TerminalExpression(new DynamicMethod("Test3", typeof(void), null));
      Assert.IsTrue(!(x.Equals((object)y) && y.Equals((object)z)) || x.Equals((object)z));
    }

    [TestMethod]
    public void TerminalExpression_EqualsObj_xEqualsy_returns_yEqualsx() {
      var x = new TerminalExpression(new DynamicMethod("Test", typeof(void), null));
      var y = new TerminalExpression(new DynamicMethod("Test2", typeof(void), null));
      Assert.IsTrue(x.Equals((object)y) == y.Equals((object)x));
    }

    [TestMethod]
    public void TerminalExpression_ToString_Returns_method_name() {
      var methodInfo = new DynamicMethod("TestMethod", typeof(void), null);
      var terminal = new TerminalExpression(methodInfo);
      var output = terminal.ToString();
      Assert.AreEqual(string.Format("{0}()", methodInfo.Name), output);
    }

    [TestMethod]
    public void TerminalExpression_ToString_Returns_method_name_with_parameters() {
      var methodInfo = new DynamicMethod("TestMethod", typeof(void), new[] { typeof(TestParameter) });
      var terminal = new TerminalExpression(methodInfo, new object[] { 5 });
      var output = terminal.ToString();
      Assert.AreEqual(string.Format("{0}(5)", methodInfo.Name), output);
    }

    [TestMethod]
    public void TerminalExpression_ToString_Returns_method_name_without_default_parameters() {
      var methodInfo = GetType().GetMethod("TestMethod");
      var terminal = new TerminalExpression(methodInfo, new object[] { 5 });
      var output = terminal.ToString();
      Assert.AreEqual(string.Format("{0}(5)", methodInfo.Name), output);
    }

    [TestMethod]
    public void TerminalExpression_ToString_Returns_method_name_without_excess_parameters() {
      var methodInfo = new DynamicMethod("TestMethod", typeof(void), null);
      var terminal = new TerminalExpression(methodInfo, new object[] { 5 });
      var output = terminal.ToString();
      Assert.AreEqual(string.Format("{0}()", methodInfo.Name), output);
    }

    public void TestMethod(TestParameter value, int number = 6) {
    }

    #region Nested type: TestParameter

    public class TestParameter : IParameter<int>
    {
      #region IParameter<int> Members

      public int Value {
        get { return 5; }
      }

      #endregion
    }

    #endregion
  }
}