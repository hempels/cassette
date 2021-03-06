﻿using Should;
using Xunit;
using Moq;
using System;

namespace Cassette.Stylesheets
{
    public class StylesheetHtmlRenderer_Tests
    {
        [Fact]
        public void GivenBundle_WhenRender_ThenHtmlLinkReturned()
        {
            var bundle = new StylesheetBundle("~/tests");
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL")
                .Verifiable();

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>");

            urlGenerator.VerifyAll();
        }

        [Fact]
        public void GivenBundleWithMedia_WhenRender_ThenHtmlLinkWithMediaAttributeReturned()
        {
            var bundle = new StylesheetBundle("~/tests")
            {
                Media = "MEDIA"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>");
        }

        [Fact]
        public void GivenStylesheetBundleWithCondition_WhenRender_ThenHtmlConditionalCommentWrapsLink()
        {
            var bundle = new StylesheetBundle("~/test")
            {
                Condition = "CONDITION"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine + 
                "<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine + 
                "<![endif]-->"
            );
        }
    }
}