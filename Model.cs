namespace sharpifier;

public class Model
{
    // Holds all the data for a single instance.
    private record Instance(double[] inputFeatures, int classification) { }

    public string Name { get; init; }
    public int KValue { get; set; } = 3;
    private readonly List<Model.Instance> trainingData;
    private readonly int featureCount;
    private List<Bound<Double>> featureBounds;
    private List<Model.Instance>? testingData;

    public Model(string path, bool ShowAll = false, string Name = "Unnamed Model")
    {
        this.Name = Name;
        trainingData = retrieveInstances(loadData(path));

        // Grab feature count and check all features are correct length.
        featureCount = trainingData[0].inputFeatures.Length;
        trainingData.ForEach(instance =>
        {
            if (instance.inputFeatures.Length != featureCount)
                throw new ArgumentException(
                    "You have an instance with an incorrect feature count./nFirst feature count: "
                        + featureCount
                        + "\nCurrent feature has count: "
                        + instance.inputFeatures.Length
                );
        });
        featureBounds = new();
        calculateBounds();
    }

    // Loads in a file of data ignoring the column labels and returns as a
    // List<string> of the lines.
    private List<string> loadData(string path)
    {
        try
        {
            List<string> data = new();
            StreamReader sr = new(path);
            var inStr = sr.ReadLine();
            while (inStr != null)
            {
                data.Add(inStr);
                inStr = sr.ReadLine();
            }
            sr.Close();
            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to read in data from \"{path}\". Error: {e}");
        }
        return new();
    }

    // Reads in all instances.
    private List<Instance> retrieveInstances(List<string> rawInstances)
    {
        // Split into features with the last "feature" being the classification.
        List<Instance> instances = new();
        rawInstances
            .Skip(1)
            .ToList()
            .ForEach(instance =>
            {
                instance.Trim();
                var features = instance
                    .Split(" ")
                    .Select(feature => Double.Parse(feature))
                    .ToArray();

                int classification = (int)(features[^1]);
                instances.Add(new Instance(features[0..^1], classification));
            });

        return instances;
    }

    // Given a list of training instances, return a List of bounds for each
    // feature.
    private void calculateBounds()
    {
        featureBounds.Clear();
        for (int i = 0; i < featureCount; i++)
            featureBounds.Add(new(double.PositiveInfinity, 0.0D));

        foreach (var instance in trainingData)
        {
            // This relies on your feature count being correct!
            // Doing this with .Zip() would be better but I didn't have time to figure
            // out why it was borked.
            for (int i = 0; i < featureCount; i++)
            {
                featureBounds[i].lower = Double.Min(
                    featureBounds[i].lower,
                    instance.inputFeatures[i]
                );
                featureBounds[i].upper = Double.Max(
                    featureBounds[i].upper,
                    instance.inputFeatures[i]
                );
            }
        }
    }

    // Finds the euclidean distance between an instance of n features and returns
    // it normalised against the bounds of that feature.
    private double findDistance(Instance p, Instance q)
    {
        return Math.Sqrt(
            p.inputFeatures
                .Zip(q.inputFeatures)
                .Zip(featureBounds)
                .Select(item =>
                {
                    var a = item.First.First; // Feature n of Instance p.
                    var b = item.First.Second; // Feature n of isntance q.
                    var min = item.Second.lower; // Lower bound for feature.
                    var max = item.Second.upper; // Uper bound for feature.

                    // Get euclidean distance between feature n of instances p, q.
                    return Math.Pow((a - b), 2.0) / Math.Pow((max - min), 2.0);
                })
                .Sum()
        );
    }

    // Classifies an instances based on the distance between itself and k nearest
    // neighbours.
    private int classifyInstance(Instance instance)
    {
        // We need a List (so it's sortable) of targets & their relevant distance
        // sorted by lowest distance.
        var distances = trainingData
            .Select(target =>
            {
                return new KeyValuePair<Instance, double>(target, findDistance(instance, target));
            })
            .OrderBy(target => target.Value)
            .ToArray();

        Dictionary<int, int> counts = new();
        foreach (var entry in distances[..KValue])
        { // Much suffering lies in this
            // single line of code.
            var c = entry.Key.classification;
            counts[c] = counts.GetValueOrDefault(entry.Key.classification) + 1;
        }

        return counts.OrderBy(entry => entry.Value).ToArray().Last().Key;
    }

    // Tests this model based on current kValue and featureBounds.
    public void Test(string path)
    {
        testingData = retrieveInstances(loadData(path));
        Console.WriteLine($"Test results for {Name}\nk = {KValue}\n");

        var predictions = testingData
            .Select(instance =>
            {
                return new Tuple<Instance, int>(instance, classifyInstance(instance));
            })
            .ToArray();

        foreach (var prediction in predictions)
        {
            Console.WriteLine(
                $"Predicted class: {prediction.Item2} | Actual class: {prediction.Item1.classification}"
            );
        }

        var successCount = predictions
            .Where(prediction => prediction.Item1.classification == prediction.Item2)
            .ToArray()
            .Length;

        var predictionAccuracy = (double)successCount / (double)predictions.Length;

        Console.WriteLine(
            $"\n{successCount}/{predictions.Length} Correct --- PREDICTION ACCURACY: {predictionAccuracy}"
        );
    }
}
