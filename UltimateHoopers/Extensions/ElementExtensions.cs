using Microsoft.Maui.Controls;

namespace UltimateHoopers.Extensions
{
    /// <summary>
    /// Extension methods for MAUI UI elements
    /// </summary>
    public static class ElementExtensions
    {
        /// <summary>
        /// Gets the absolute bounds of a visual element relative to the application window
        /// </summary>
        /// <param name="element">The element to calculate bounds for</param>
        /// <returns>A tuple containing (X, Y, Width, Height) coordinates</returns>
        public static (double X, double Y, double Width, double Height) GetAbsoluteBounds(this VisualElement element)
        {
            if (element == null)
                return (0, 0, 0, 0);

            // Start with the element's bounds
            double x = element.X;
            double y = element.Y;
            double width = element.Width;
            double height = element.Height;

            // Navigate up the visual tree, transforming the coordinates
            var parent = element.Parent as VisualElement;
            while (parent != null)
            {
                // Translate by parent's position
                x += parent.X;
                y += parent.Y;

                parent = parent.Parent as VisualElement;
            }

            return (x, y, width, height);
        }

        /// <summary>
        /// Determines if an element is visible within the viewport of a scrollable container
        /// </summary>
        /// <param name="element">The element to check visibility for</param>
        /// <param name="container">The scrollable container (like CollectionView)</param>
        /// <param name="threshold">Visibility threshold (0.0 to 1.0) - how much of the element must be visible</param>
        /// <returns>True if element is considered visible</returns>
        public static bool IsVisibleInViewport(this VisualElement element, VisualElement container, double threshold = 0.5)
        {
            try
            {
                if (element == null || container == null)
                    return false;

                // Get bounds of element and container
                var elementBounds = element.GetAbsoluteBounds();
                var containerBounds = container.GetAbsoluteBounds();

                // Calculate top and bottom positions
                double elementTop = elementBounds.Y;
                double elementBottom = elementBounds.Y + elementBounds.Height;
                double containerTop = containerBounds.Y;
                double containerBottom = containerBounds.Y + containerBounds.Height;

                // Add margin to improve detection during scrolling
                double margin = 100;
                containerTop -= margin;
                containerBottom += margin;

                // Calculate how much of the element is visible
                double visibleTop = Math.Max(elementTop, containerTop);
                double visibleBottom = Math.Min(elementBottom, containerBottom);
                double visibleHeight = Math.Max(0, visibleBottom - visibleTop);

                // Calculate visibility percentage
                double visibilityPercentage = 0.0;
                if (elementBounds.Height > 0)
                {
                    visibilityPercentage = visibleHeight / elementBounds.Height;
                }

                // Return true if visibility percentage exceeds threshold
                return visibilityPercentage >= threshold;
            }
            catch (Exception ex)
            {
                Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() => {
                    System.Diagnostics.Debug.WriteLine($"Error checking element visibility: {ex.Message}");
                });
                return false;
            }
        }

        /// <summary>
        /// Find all visual elements of a specified type within a parent element
        /// </summary>
        /// <typeparam name="T">The type of elements to find</typeparam>
        /// <param name="parent">The parent element to search within</param>
        /// <returns>A collection of found elements</returns>
        public static IEnumerable<T> FindVisualChildren<T>(this Element parent) where T : VisualElement
        {
            var results = new List<T>();

            if (parent == null)
                return results;

            // Check if the parent itself is of the desired type
            if (parent is T foundElement)
            {
                results.Add(foundElement);
            }

            // Recursively search logical children
            foreach (var child in parent.LogicalChildren)
            {
                if (child is Element childElement)
                {
                    results.AddRange(FindVisualChildren<T>(childElement));
                }
            }

            return results;
        }

        /// <summary>
        /// Finds a visual element by its name (StyleId)
        /// </summary>
        /// <typeparam name="T">The type of element to find</typeparam>
        /// <param name="parent">The parent element to search within</param>
        /// <param name="name">The StyleId to look for</param>
        /// <returns>The found element, or null if not found</returns>
        public static T FindVisualChildByName<T>(this Element parent, string name) where T : VisualElement
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            // Check if the parent itself matches
            if (parent is T foundElement && foundElement.StyleId == name)
            {
                return foundElement;
            }

            // Search logical children
            foreach (var child in parent.LogicalChildren)
            {
                if (child is Element childElement)
                {
                    var result = FindVisualChildByName<T>(childElement, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all visual elements with a specific name (StyleId)
        /// </summary>
        /// <typeparam name="T">The type of elements to find</typeparam>
        /// <param name="parent">The parent element to search within</param>
        /// <param name="name">The StyleId to look for</param>
        /// <returns>Collection of found elements</returns>
        public static IEnumerable<T> FindVisualChildrenByName<T>(this Element parent, string name) where T : VisualElement
        {
            var results = new List<T>();

            if (parent == null || string.IsNullOrEmpty(name))
                return results;

            // Check if the parent itself matches
            if (parent is T foundElement && foundElement.StyleId == name)
            {
                results.Add(foundElement);
            }

            // Search logical children
            foreach (var child in parent.LogicalChildren)
            {
                if (child is Element childElement)
                {
                    results.AddRange(FindVisualChildrenByName<T>(childElement, name));
                }
            }

            return results;
        }
    }
}