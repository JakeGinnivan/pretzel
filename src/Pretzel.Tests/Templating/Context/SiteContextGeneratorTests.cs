﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class SiteContextGeneratorTests
    {
        private readonly SiteContextGenerator generator;
        private readonly MockFileSystem fileSystem;

        public SiteContextGeneratorTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            generator = new SiteContextGenerator(fileSystem);
        }

        [Fact]
        public void site_context_generator_finds_posts()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void site_context_generator_processes_page_markdown()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void posts_do_not_include_pages()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(0, siteContext.Posts.Count);
        }

        [Fact]
        public void posts_without_front_matter_get_processed()
        {

            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void posts_without_front_matter_uses_convention_to_render_folder()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            var outputPath = string.Format("/{0}/{1}", DateTime.Now.ToString("yyyy/MM/dd"), "SomeFile.html");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void posts_without_front_matter_and_override_config_renders_folder()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData("permalink: /blog/:year/:month/:day/:title.html"));

            var outputPath = string.Format("/blog/{0}/{1}", DateTime.Now.ToString("yyyy/MM/dd"), "SomeFile.html");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void pages_without_front_matter_do_not_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.IsType<NonProcessedPage>(siteContext.Pages[0]);
        }

        [Fact]
        public void pages_with_front_matter_do_not_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.IsType<Page>(siteContext.Pages[0]);
        }
        
        [Fact]
        public void site_context_includes_pages_in_same_folder()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile2.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");
            
            // assert
            Assert.Equal(2, siteContext.Pages[0].DirectoryPages.ToArray().Length);
        }

        [Fact]
        public void site_context_does_not_cache_page()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));
            generator.BuildContext(@"C:\TestSite");
            fileSystem.RemoveFile(@"C:\TestSite\SubFolder\SomeFile.md");
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# AnotherTitle")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");
            

            // assert
            Assert.True(siteContext.Pages[0].Content.Contains("AnotherTitle"), "Site context should not cache output");
        }

        private static string ToPageContent(string content)
        {
            return @"---
title: Title
---" + content;
        }
    }
}