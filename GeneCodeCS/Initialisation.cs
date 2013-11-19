namespace GeneCodeCS
{
  /// <summary>
  ///   Defines the methods that can be used to generate the initial bot trees.
  /// </summary>
  public enum Initialisation
  {
    /// <summary>
    ///   Create fully populated trees with each internal node assigned a function.
    /// </summary>
    Full,

    /// <summary>
    ///   Create trees using either functions or terminals randomly selected.
    /// </summary>
    Grow
  }
}