using System.Collections.Generic;

namespace LinkLite.Helpers
{
    /// <summary>
    /// Represents a Node of a descent only tree structure
    /// where all nodes contain values of the same type.
    /// </summary>
    /// <typeparam name="T">Type of node values</typeparam>
    public class TreeNode<T>
    {
        public T? Value { get; set; }
        public List<TreeNode<T>> Children { get; } = new();
        public TreeNode<T> Add(TreeNode<T> child)
        {
            Children.Add(child);
            return child;
        }
    }
}
