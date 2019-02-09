namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// Flag for component search utilities that specify
    /// in which gameObjects to search for components. <br/>
    /// Specifying anything other than <see cref="Search.InWholeHierarchy"/>
    /// causes the lookups to search until either the scene root is
    /// reached or there are no more children.<br/>
    /// When specifying <see cref="Search.InWholeHierarchy"/>, the parents
    /// are always searched first for performance reasons, as
    /// the hierarchy of parents can be traversed linearly, while
    /// searching children results in a depth-first-search.
    /// </summary>
    /// <seealso cref="ComponentQueryExtensions"/>
    public enum Search
    {
        /// <inheritdoc cref="Search"/>
        InObjectOnly,
        /// <inheritdoc cref="Search"/>
        InChildren,
        /// <inheritdoc cref="Search"/>
        InParents,
        /// <inheritdoc cref="Search"/>
        InWholeHierarchy
    }
}