namespace sharpifier;

class Program
{
    static void Main(string[] args)
    {
        Model m = new("data/wine-training");
        try
        {
            if (args.Length > 1)
                m.KValue = Int32.Parse(args[1]);
        }
        catch
        {
            Console.WriteLine("Invalid k value. Proceeding with k = 3.");
        }
        m.Test("data/wine-test");
    }
}
