using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Extensions;

namespace MP.FilterReader.Tests
{
    public class FilterReaderTests
    {
        const string pdf = "Resources\\text.pdf";
        const string doc = "Resources\\text.doc";
        const string docx = "Resources\\text.docx";
        const string pptx = "Resources\\text.pptx";
        const string htm = "Resources\\text.htm";

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void OpenFileDirectly(string fileName)
        {
            using (var reader = new FilterReader(fileName))
            {
            }
        }

        [Theory,
        InlineData(pdf, ".pdf"),
        InlineData(doc, ".doc"),
        InlineData(docx, ".docx"),
        InlineData(pptx, ".pptx"),
        InlineData(htm, ".htm")]
        public void OpenStream(string fileName, string extension)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new FilterReader(stream, extension))
            {
            }
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void ReadToEnd(string fileName)
        {
            using (var reader = new FilterReader(fileName))
            {
                var fileContent = string.Empty;
                Assert.DoesNotThrow(() =>
                {
                    fileContent = reader.ReadToEnd();
                });

                Assert.True(fileContent.Contains("IFilter"));
            }
        }

        [Theory,
        InlineData(pdf, ".pdf"),
        InlineData(doc, ".doc"),
        InlineData(docx, ".docx"),
        InlineData(pptx, ".pptx"),
        InlineData(htm, ".htm")]
        public void ReadToEndStream(string fileName, string extension)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new FilterReader(stream, extension))
            {
                var fileContent = string.Empty;
                Assert.DoesNotThrow(() =>
                {
                    fileContent = reader.ReadToEnd();
                });

                Assert.True(fileContent.Contains("IFilter"));
            }
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void Peek(string fileName)
        {
            using (var reader = new FilterReader(fileName))
            {
                var peaked1 = reader.Peek();
                var peaked2 = reader.Peek();
                var first = reader.Read();
                var second = reader.Read();

                // peek should not move index
                Assert.Equal(peaked1, peaked2);
                Assert.Equal(peaked1, first);
                Assert.NotEqual(peaked1, second);
                Assert.NotEqual(first, second);
            }
        }

        [Theory,
        InlineData(pdf, ".pdf"),
        InlineData(doc, ".doc"),
        InlineData(docx, ".docx"),
        InlineData(pptx, ".pptx"),
        InlineData(htm, ".htm")]
        public void PeekStream(string fileName, string extension)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new FilterReader(stream, extension))
            {
                var peaked1 = reader.Peek();
                var peaked2 = reader.Peek();
                var first = reader.Read();
                var second = reader.Read();

                // peek should not move index
                Assert.Equal(peaked1, peaked2);
                Assert.Equal(peaked1, first);
                Assert.NotEqual(peaked1, second);
                Assert.NotEqual(first, second);
            }
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void ReadLine(string fileName)
        {
            List<string> lines = new List<string>();

            using (var reader = new FilterReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            Assert.NotEqual(0, lines.Count);
        }

        [Theory,
        InlineData(pdf),
        InlineData(doc),
        InlineData(docx),
        InlineData(pptx),
        InlineData(htm)]
        public void ReadBlock(string fileName)
        {
            using (var reader = new FilterReader(fileName))
            {
                var bufferSize = 2064;
                var buffer = new char[bufferSize];
                var totalRead = reader.ReadBlock(buffer, 0, bufferSize);

                Assert.NotEqual(0, totalRead);
            }
        }
    }
}