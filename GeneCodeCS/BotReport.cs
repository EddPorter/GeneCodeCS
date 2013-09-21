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

namespace GeneCodeCS
{
  public class BotReport
  {
    public BotReport(string name, string parent1, string parent2, ExpressionTree tree) {
      Name = name;
      Parent1 = parent1;
      Parent2 = parent2;
      Tree = tree;
    }

    public int Fitness { get; private set; }

    public object Information { get; private set; }

    public string Name { get; private set; }

    public string Parent1 { get; private set; }

    public string Parent2 { get; private set; }

    public ExpressionTree Tree { get; private set; }

    public Type BotInstance { get; internal set; }
  }
}