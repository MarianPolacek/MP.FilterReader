using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace MP.FilterReader.Tests
{
    public class FilterHelperTests
    {
        const string pdf = "Resources\\text.pdf";
        const string doc = "Resources\\text.doc";
        const string docx = "Resources\\text.docx";
        const string pptx = "Resources\\text.pptx";
        const string htm = "Resources\\text.htm";

        //<summary>
        //This tests assume IFilters are correctly installed
        //</summary>
        //<param name="fileName"></param>
        //<param name="extension"></param>
        [Theory,
        InlineData(".pdf"),
        InlineData(".doc"),
        InlineData(".docx"),
        InlineData(".pptx"),
        InlineData(".htm")]
        public void FilterExists(string extension)
        {
            Assert.Equal(true, FilterHelper.IsFilterAvailable(extension));
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        //, InlineData("Resources\\text.jpg")]
        public void ReadAllLines(string filePath)
        {
            var lines = FilterHelper.ReadAllLines(filePath).Where(x => x.Contains("IFilter")).ToArray();

            Assert.Equal(1, lines.Length);
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void ReadAll(string filePath)
        {
            var text = FilterHelper.RealAll(filePath);

            Assert.Contains("IFilter", text);
        }
    }
}
