using System;
using System.Text.RegularExpressions;


namespace PDFEditor {
    class Demo {
        static void Main(string[] args) {
            //input path of pdf
            string input_path = "C:\\Users\\Julia\\Desktop\\sample.pdf";
            
            //pattern to find
            string regex_pattern = "A straightforward approach [^.]+";
            Regex pattern = new Regex(regex_pattern, RegexOptions.IgnoreCase);

            //highlight matching pattern in pdf
            PDFEditor editor = new PDFEditor(input_path, input_path);
            editor.HighlightPattern(pattern);

            Console.Write("herro");
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }
    }
}