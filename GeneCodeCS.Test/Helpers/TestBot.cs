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

namespace GeneCodeCS.Test.Helpers
{
  /// <summary>
  ///   TODO: Update summary.
  /// </summary>
  public class TestBot : BaseBot
  {
    public void DoNothing() {
    }

    public void DoSomething() {
    }

    public override void Execute() {
      IsTrue();
      base.Execute();
    }

    public override void Initialise(object parameters) {
      if (parameters == null) {
        IsFalse();
      }
      throw new NotImplementedException();
    }

    public bool IsFalse() {
      return false;
    }

    public bool IsStupid(TestParameter m) {
      return m.Value == 1;
    }

    public bool IsTrue() {
      return true;
    }

    protected override bool NotFinished() {
      throw new NotImplementedException();
    }

    protected override void RunBotLogic() {
      throw new NotImplementedException();
    }

    protected override void Evaluate() {
      Fitness = 5;
    }
  }
}