using bytes = org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.interaction;
using org.pdfclown.documents.interchange.metadata;
using org.pdfclown.documents.interaction.viewer;
using files = org.pdfclown.files;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents;
using org.pdfclown.tools;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;



namespace PDFEditor {
    /**
      <summary>Abstract sample.</summary>
    */
    public class PDFEditor {
        #region dynamic
        #region fields
        private string inputPath;
        private string outputPath;
        private files.File file;
        #endregion

        #region interface
        #region public

        public PDFEditor(string inputPath)
            : this(inputPath, inputPath) {
        }

        public PDFEditor(string inputPath, string outPath) {
            this.inputPath = inputPath;
            this.outputPath = outPath;
            this.file = new files.File(inputPath);
        }

        public void HighlightPattern(Regex Pattern, Page page) {
            //fill in later!
        }

        public void HighlightPattern(Regex pattern) {
            TextExtractor textExtractor = new TextExtractor(false, true);
            foreach (Page page in this.file.Document.Pages) {

                Console.WriteLine("\nScanning page " + (page.Index + 1) + "...\n");

                // 2.1. Extract the page text!
                IDictionary<RectangleF?, IList<ITextString>> textStrings = textExtractor.Extract(page);

                string test = TextExtractor.ToString(textStrings);
                //test = test.Replace(" ", string.Empty);

                // 2.2. Find the text pattern matches!
                MatchCollection matches = pattern.Matches(test);
               
                // 2.3. Highlight the text pattern matches!
                textExtractor.Filter(textStrings, new TextHighlighter(page, matches));
            }
            Serialize(file, false);
        }

        public void Populate(string output, StandardType1Font font_type, int font_size) {
            // 1. Add the page to the document!
            Document document = this.file.Document;
            Page page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            // 2. Create a content composer for the page!
            PrimitiveComposer composer = new PrimitiveComposer(page);

            // 3. Inserting contents...
            // Set the font to use!
            composer.SetFont(font_type, font_size);
            // Show the text onto the page!
            /*
              NOTE: PrimitiveComposer's ShowText() method is the most basic way
              to add text to a page -- see BlockComposer for more advanced uses
              (horizontal and vertical alignment, hyphenation, etc.).
            */
            composer.ShowText(output);

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        /**
      <summary>Executes the sample.</summary>
      <returns>Whether the sample has been completed.</returns>
    */
        //public abstract bool Run();
        #endregion

        #region protected
        protected string GetIndentation(int level) {
            return new String(' ', level);
        }

        protected string InputPath {
            get {
                return inputPath;
            }
        }

        protected string OutputPath {
            get {
                return outputPath;
            }
        }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">File to serialize.</param>
        */
        protected void Serialize(files::File file) {
            Serialize(file, true);
        }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">File to serialize.</param>
          <param name="chooseMode">Whether to allow user choice of serialization mode.</param>
        */
        protected void Serialize(files::File file, bool chooseMode) {
            Serialize(file, chooseMode, null, null);
        }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">File to serialize.</param>
          <param name="chooseMode">Whether to allow user choice of serialization mode.</param>
          <param name="title">Document title.</param>
          <param name="subject">Document subject.</param>
        */
        protected void Serialize(files::File file, bool chooseMode, string title, string subject) {
            Serialize(file, GetType().Name, chooseMode, title, subject);
        }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">File to serialize.</param>
          <param name="fileName">Output file name.</param>
          <param name="chooseMode">Whether to allow user choice of serialization mode.</param>
          <param name="title">Document title.</param>
          <param name="subject">Document subject.</param>
        */
        protected void Serialize(files::File file, string fileName, bool chooseMode, string title, string subject) {
            ApplyDocumentSettings(file.Document, title, subject);

            Console.WriteLine();
            files::SerializationModeEnum serializationMode = files::SerializationModeEnum.Incremental;
            if (chooseMode) {
                Console.WriteLine("[0] Standard serialization");
                Console.WriteLine("[1] Incremental update");
                Console.Write("Please select a serialization mode: ");
                try {
                    serializationMode = (files::SerializationModeEnum)Int32.Parse(Console.ReadLine());
                }
                catch {/* Default. */
                }
            }

            string outputFilePath = outputPath + fileName + "." + serializationMode + ".pdf";

            // Save the file!
            /*
              NOTE: You can also save to a generic target stream (see Save() method overloads).
            */
            try {
                file.Save(outputFilePath, serializationMode);
            }
            catch (Exception e) {
                Console.WriteLine("File writing failed: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("\nOutput: " + outputFilePath);
        }
        #endregion

        #region internal
        internal void Initialize(string inputPath, string outputPath) {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
        }
        #endregion

        #region private
        private class TextHighlighter : TextExtractor.IIntervalFilter {
            private IEnumerator matchEnumerator;
            private Page page;

            public TextHighlighter(Page page, MatchCollection matches) {
                this.page = page;
                this.matchEnumerator = matches.GetEnumerator();
            }

            public Interval<int> Current {
                get {
                    Match current = (Match)matchEnumerator.Current;
                    return new Interval<int>(current.Index, current.Index + current.Length);
                }
            }

            object IEnumerator.Current {
                get {
                    return this.Current;
                }
            }

            public void Dispose() {/* NOOP */
            }

            public bool MoveNext() {
                return matchEnumerator.MoveNext();
            }

            public void Process(Interval<int> interval, ITextString match) {
                // Defining the highlight box of the text pattern match...
                IList<Quad> highlightQuads = new List<Quad>();
                {
                    /*
                      NOTE: A text pattern match may be split across multiple contiguous lines,
                      so we have to define a distinct highlight box for each text chunk.
                    */
                    RectangleF? textBox = null;
                    foreach (TextChar textChar in match.TextChars) {
                        RectangleF textCharBox = textChar.Box;
                        if (!textBox.HasValue) {
                            textBox = textCharBox;
                        }
                        else {
                            if (textCharBox.Y > textBox.Value.Bottom) {
                                highlightQuads.Add(Quad.Get(textBox.Value));
                                textBox = textCharBox;
                            }
                            else {
                                textBox = RectangleF.Union(textBox.Value, textCharBox);
                            }
                        }
                    }
                    highlightQuads.Add(Quad.Get(textBox.Value));
                }
                

                // Highlight text pattern by quads one by one.
                foreach (Quad q in highlightQuads) {
                    new TextMarkup(page, TextMarkup.MarkupTypeEnum.Highlight, q);
                }
                //new TextMarkup(page, TextMarkup.MarkupTypeEnum.Highlight, highlightQuads);
            }

            public void Reset() {
                throw new NotSupportedException();
            }
        }

        private void ApplyDocumentSettings(Document document, string title, string subject) {
            if (title == null)
                return;

            // Viewer preferences.
            ViewerPreferences view = new ViewerPreferences(document); // Instantiates viewer preferences inside the document context.
            document.ViewerPreferences = view; // Assigns the viewer preferences object to the viewer preferences function.
            view.DisplayDocTitle = true;

            // Document metadata.
            Information info = new Information(document);
            document.Information = info;
            info.Author = "Stefano Chizzolini";
            info.CreationDate = DateTime.Now;
            info.Creator = GetType().FullName;
            info.Title = "PDF Clown - " + title + " sample";
            info.Subject = "Sample about " + subject + " using PDF Clown";
        }
        #endregion
        #endregion
        #endregion
    }
}