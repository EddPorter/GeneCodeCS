﻿// 
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

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class TerminalTests
  {
    [TestMethod]
    public void Terminal_ToString_Returns_method_name() {
      const string name = "Terminal_ToString_Returns_method_name";
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
      var methods = typeof(TerminalTests).GetMethods(flags);
      var terminal = new Terminal(methods.First(m => m.Name == name));
      var output = terminal.ToString();
      Assert.AreEqual(name + "()", output);
    }
  }
}