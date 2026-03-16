using System;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using TinyLanguageScanner;
class Program

{

    static void Main(string[] args)

    {

        Console.WriteLine("Tiny Language Scanner");

        Console.WriteLine("1) Run sample code");

        Console.WriteLine("2) Enter code manually");

        Console.WriteLine("3) Load from file");

        Console.WriteLine("0) Exit");



        while (true)

        {

            Console.Write("\nChoice: ");

            string choice = Console.ReadLine()?.Trim() ?? "";



            if (choice == "0") break;

            else if (choice == "1") RunSample();

            else if (choice == "2") RunInteractive();

            else if (choice == "3") RunFromFile();

            else Console.WriteLine("Invalid.");

        }

    }



    static void RunSample()

    {

        string code = @"/* computes factorial */

int main()

{

int x;

read x;

if x > 0 then

int fact := 1;

repeat

fact := fact * x;

x := x - 1;

until x = 0

write fact;

end

return 0;

}";

        Scan(code);

    }



    static void RunInteractive()

    {

        Console.WriteLine("Enter code, then type END on a new line:");

        var sb = new System.Text.StringBuilder();

        string line;

        while ((line = Console.ReadLine()) != null)

        {

            if (line.Trim().ToUpper() == "END") break;

            sb.AppendLine(line);

        }

        Scan(sb.ToString());

    }



    static void RunFromFile()

    {

        Console.Write("File path: ");

        string path = Console.ReadLine()?.Trim() ?? "";

        if (!File.Exists(path)) { Console.WriteLine("File not found."); return; }

        Scan(File.ReadAllText(path));

    }



    static void Scan(string source)

    {

        var scanner = new Scanner();

        var tokens = scanner.Tokenize(source);



        Console.WriteLine("\nLine Token Type Value");

        Console.WriteLine(new string('-', 60));



        foreach (var tok in tokens)

        {

            string val = tok.Value.Length > 30 ? tok.Value[..30] + "..." : tok.Value;

            Console.WriteLine($"{tok.Line,-9}{tok.Type,-22}{val}");

        }



        Console.WriteLine(new string('-', 60));

        Console.WriteLine($"Total tokens: {tokens.Count}");



        var errors = tokens.Where(t => t.Type == TokenType.UNKNOWN).ToList();

        if (errors.Any())

        {

            Console.WriteLine("Errors:");

            foreach (var e in errors)

                Console.WriteLine($" Line {e.Line}: unknown token '{e.Value}'");

        }

        else
        {
            Console.WriteLine("no errors found");
        }
    }
}