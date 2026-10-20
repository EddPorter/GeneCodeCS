GeneCodeCS
==========

A genetic programming library for performing breeding and natural selection on code bots.

Usage
-----

Requires .NET 8. Build and test with `dotnet test`.

Instantiate a new object of `Population<>` passing a custom sub-class of `BaseBot`. The sub-class must be concrete and have a public parameterless constructor. Define public methods in the sub-class for:
* Functions - these return bool;
* Terminals - there return void.
An optimisation function can optionally be passed to the `Population<>` constructor. When a new `Chromosome` is created, it can be reduced and optimised (for example, by removing any impossible branches). Care must be taken that this doesn't reduce the possible search space of the tree.

Parameters can be passed to these public methods, though all must derive from IParameter<T> and have two construtors:
* the first takes an instance of `Random` (though it need not be used) and is used to fix the value for the parameter. The framework will read it from the Value property and use it to recreate the ephemeral "random" constant within the execution.
* the second takes a value of type T, which should simply be stored to be returned through the Value property.
These properties allow you define acceptable ranges for each parameter, i.e. by scaling up the return from `Random` appropriately.

Each bot will have Initialise called, then Execute, and then Evaluate. The bot should provide implementations for all of these:
* Initialise - takes a custom `object` that should be used to pass the relevant tuning parameters and objects to access the underlying data;
* Execute - this should set up the data access and call `RunBotLogic` as required. This in turn will call back into the Functions and Terminals defined as public functions;
* Evaluate - this must set the Fitness property of the bot post-execution.

To run the genetic program, call `SimulateGenerations`.


Bots are compiled in memory from expression trees to delegates, so nothing is written to disk and no assemblies are loaded that cannot be collected.
