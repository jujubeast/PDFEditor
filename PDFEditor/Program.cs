using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;

using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents;
using org.pdfclown.files;
using org.pdfclown.tools;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.util.math;
using org.pdfclown.util.math.geom;



namespace PDFEditor {
    class ProgramDemo {
        static void Main(string[] args) {
            string input_path = "C:\\Users\\Julia\\Desktop\\sample2.pdf";

            Regex pattern = new Regex("while touch sensing [^.]+", RegexOptions.IgnoreCase);

            PDFEditor p1 = new PDFEditor(input_path, "");
            p1.HighlightPattern(pattern);
            Console.Write("herro");
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }
    }
}