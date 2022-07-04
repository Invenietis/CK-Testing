using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CK.Testing.Tests
{

    [TestFixture]
    public class ClosestSUTProjectFolderTests
    {
        [Test]
        public void find_next_to_the_test()
        {
            var directories = new[]
            {
                "X:/S/Tests/P.Tests",
                "X:/S/Tests/P"
            };
            FindClosestSUTProject( "X:/S/Tests/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( "X:/S/Tests/P" );
        }

        [Test]
        public void find_above_the_test()
        {
            var directories = new[]
            {
                "X:/S/Tests/P.Tests",
                "X:/S/P"
            };
            FindClosestSUTProject( "X:/S/Tests/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( "X:/S/P" );
        }

        [Test]
        public void a_parent_folder_cannot_be_the_SUT()
        {
            var directories = new[]
            {
                "X:/S/P/Tests/P.Tests",
            };
            FindClosestSUTProject( "X:/S/P/Tests/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( new NormalizedPath() );
        }

        [Test]
        public void find_above_even_if_test_is_in_a_sub_path()
        {
            var directories = new[]
            {
                "X:/S/Tests/Sub/P.Tests",
                "X:/S/P"
            };
            FindClosestSUTProject( "X:/S/Tests/Sub/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( "X:/S/P" );
        }

        [Test]
        public void find_above_with_a_same_sub_path()
        {
            var directories = new[]
            {
                "X:/S/Tests/Sub/P.Tests",
                "X:/S/Sub/P",
                // Won't find this one:
                "X:/S/P"
            };
            FindClosestSUTProject( "X:/S/Tests/Sub/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( "X:/S/Sub/P" );
        }

        [Test]
        public void find_above_with_a_same_sub_path_in_a_root_Tests_folder()
        {
            var directories = new[]
            {
                "X:/S/Tests/Lot/Tests/Sub/P.Tests",
                "X:/S/Tests/Lot/Sub/P",
                // Won't find these ones.
                "X:/S/Tests/Lot/P",
                "X:/S/Tests/Sub/P",
                "X:/S/Tests/P"
            };
            FindClosestSUTProject( "X:/S/Tests/Lot/Tests/Sub/P.Tests", p => directories.AsSpan().Contains( p ) ).Should().Be( "X:/S/Tests/Lot/Sub/P" );
        }

        static NormalizedPath FindClosestSUTProject( NormalizedPath testProjectFolder, Func<NormalizedPath,bool> exists )
        {
            var candidates = BasicTestHelper.GetClosestSUTProjectCandidatePaths( "X:/S", testProjectFolder ).ToArray();
            return candidates.Where( exists ).FirstOrDefault();
        }


    }
}
