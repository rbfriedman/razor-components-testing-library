﻿using System;
using System.Linq;
using AngleSharp;
using AngleSharp.Diffing.Core;
using AngleSharp.Dom;
using Egil.RazorComponents.Testing.Extensions;
using Egil.RazorComponents.Testing.Diffing;
using Xunit;
using Xunit.Sdk;

namespace Egil.RazorComponents.Testing.Asserting
{
    /// <summary>
    /// A set of addition diff assert extensions 
    /// </summary>
    public static class ShouldBeAdditionAssertExtensions
    {
        /// <summary>
        /// Verifies that the <paramref name="actualChange"/> <see cref="IDiff"/> is an addition,
        /// i.e. that one or more nodes have been added, and verifies that the additions are equal
        /// to the markup specified in the <paramref name="expectedChange"/> input.
        /// </summary>
        /// <param name="actualChange">The change to verify</param>
        /// <param name="expectedChange">The expected additions to verify against</param>
        /// <param name="userMessage">A custom user message to display in case the verification fails.</param>
        public static void ShouldBeAddition(this IDiff actualChange, string expectedChange, string? userMessage = null)
        {
            if (actualChange is null) throw new ArgumentNullException(nameof(actualChange));
            if (expectedChange is null) throw new ArgumentNullException(nameof(expectedChange));

            var actual = Assert.IsType<UnexpectedNodeDiff>(actualChange);
            
            INodeList expected;
            if(actual.Test.Node.GetHtmlParser() is { } parser)
            {
                expected = parser.Parse(expectedChange);
            }
            else
            {
                using var newParser = new TestHtmlParser();
                expected = newParser.Parse(expectedChange);
            }

            ShouldBeAddition(actualChange, expected, userMessage);
        }

        /// <summary>
        /// Verifies that the <paramref name="actualChange"/> <see cref="IDiff"/> is an addition,
        /// i.e. that one or more nodes have been added, and verifies that the additions are equal
        /// to the rendered markup from the <paramref name="expectedChange"/> <see cref="IRenderedFragment"/>.
        /// </summary>
        /// <param name="actualChange">The change to verify</param>
        /// <param name="expectedChange">The expected additions to verify against</param>
        /// <param name="userMessage">A custom user message to display in case the verification fails.</param>
        public static void ShouldBeAddition(this IDiff actualChange, IRenderedFragment expectedChange, string? userMessage = null)
        {
            if (expectedChange is null) throw new ArgumentNullException(nameof(expectedChange));
            ShouldBeAddition(actualChange, expectedChange.Nodes, userMessage);
        }

        /// <summary>
        /// Verifies that the <paramref name="actualChange"/> <see cref="IDiff"/> is an addition,
        /// i.e. that one or more nodes have been added, and verifies that the additions are equal
        /// to the <paramref name="expectedChange"/> nodes.
        /// </summary>
        /// <param name="actualChange">The change to verify</param>
        /// <param name="expectedChange">The expected additions to verify against</param>
        /// <param name="userMessage">A custom user message to display in case the verification fails.</param>
        public static void ShouldBeAddition(this IDiff actualChange, INodeList expectedChange, string? userMessage = null)
        {
            if (actualChange is null) throw new ArgumentNullException(nameof(actualChange));
            if (expectedChange is null) throw new ArgumentNullException(nameof(expectedChange));

            var actual = Assert.IsType<UnexpectedNodeDiff>(actualChange);
            var comparer = actual.Test.Node.GetHtmlComparer() ?? new HtmlComparer();

            var diffs = comparer.Compare(expectedChange, actual.Test.Node.AsEnumerable()).ToList();

            if (diffs.Count != 0)
            {
                throw new HtmlEqualException(diffs, expectedChange, actual.Test.Node, userMessage);
            }
        }
    }
}
