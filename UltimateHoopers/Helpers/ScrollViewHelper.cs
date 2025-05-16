using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper class for managing auto-playing videos in scrollable views
    /// </summary>
    public static class ScrollViewHelper
    {
        /// <summary>
        /// Determines if an element is currently visible within the viewport of a scrollable container
        /// </summary>
        /// <param name="element">The element to check for visibility</param>
        /// <param name="container">The scrollable container</param>
        /// <param name="threshold">The percentage of the element that must be visible (0.0 to 1.0)</param>
        /// <returns>True if the element is visible according to the threshold, otherwise false</returns>
        public static bool IsElementVisible(VisualElement element, ScrollView container, double threshold = 0.5)
        {
            try
            {
                if (element == null || container == null)
                    return false;

                // Get element bounds relative to container
                var elementBounds = element.Bounds;
                var containerBounds = container.Bounds;

                // Calculate visible area of the element
                var elementTop = elementBounds.Y;
                var elementBottom = elementBounds.Y + elementBounds.Height;
                var containerTop = container.ScrollY;
                var containerBottom = container.ScrollY + containerBounds.Height;

                // Calculate how much of the element is visible
                var visibleTop = Math.Max(elementTop, containerTop);
                var visibleBottom = Math.Min(elementBottom, containerBottom);
                var visibleHeight = visibleBottom - visibleTop;

                // Calculate visibility percentage
                var visibilityPercentage = 0.0;
                if (elementBounds.Height > 0)
                {
                    visibilityPercentage = visibleHeight / elementBounds.Height;
                }

                // Return true if visibility percentage exceeds threshold
                return visibilityPercentage >= threshold;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking element visibility: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all elements of a specific type that are currently visible in a CollectionView
        /// </summary>
        /// <typeparam name="T">The type of elements to find</typeparam>
        /// <param name="collectionView">The CollectionView to search within</param>
        /// <param name="threshold">The percentage of the element that must be visible (0.0 to 1.0)</param>
        /// <returns>A list of visible elements of type T</returns>
        public static IEnumerable<T> GetVisibleElements<T>(CollectionView collectionView, double threshold = 0.5)
            where T : VisualElement
        {
            try
            {
                if (collectionView == null)
                    return Enumerable.Empty<T>();

                // Get all elements of the specified type
                var elements = FindVisualChildren<T>(collectionView);

                // Filter to only visible elements
                var visibleElements = new List<T>();

                foreach (var element in elements)
                {

                    // In MAUI, we don't have a direct way to get the scroll position of a CollectionView
                    // So we'll use a different approach without relying on ScrollY

                    // Get element's absolute position - this is an estimation
                    double elementY = 0;
                    VisualElement current = element;

                    // Walk up the visual tree to calculate approximate position
                    while (current != null && current != collectionView)
                    {
                        elementY += current.Y;
                        current = current.Parent as VisualElement;
                    }

                    // We don't have access to the scroll position directly
                    // so we'll consider the element's position relative to the collection view's viewport

                    // Element's height
                    var elementHeight = element.Height;

                    // CollectionView's viewport height
                    var collectionViewHeight = collectionView.Height;

                    // Calculate how much of the element is visible
                    var visibleTop = Math.Max(0, elementY);
                    var visibleBottom = Math.Min(collectionViewHeight, elementY + elementHeight);
                    var visibleHeight = visibleBottom - visibleTop;

                    // Calculate visibility percentage
                    var visibilityPercentage = 0.0;
                    if (elementHeight > 0)
                    {
                        visibilityPercentage = visibleHeight / elementHeight;
                    }

                    // Add to result if visibility percentage exceeds threshold
                    if (visibilityPercentage >= threshold)
                    {
                        visibleElements.Add(element);
                    }
                }

                return visibleElements;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting visible elements: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Recursively finds all visual children of a specific type
        /// </summary>
        /// <typeparam name="T">The type of elements to find</typeparam>
        /// <param name="parent">The parent element to search within</param>
        /// <returns>A list of found elements of type T</returns>
        public static IEnumerable<T> FindVisualChildren<T>(Element parent) where T : VisualElement
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
        /// Find a specific named element within a parent element
        /// </summary>
        /// <typeparam name="T">The type of element to find</typeparam>
        /// <param name="parent">The parent element to search within</param>
        /// <param name="name">The name (StyleId) of the element to find</param>
        /// <returns>The found element or null</returns>
        public static T FindVisualChildByName<T>(this Element parent, string name) where T : VisualElement
        {
            if (parent == null)
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
    }
}