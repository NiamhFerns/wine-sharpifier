namespace sharpifier;

class Program
{
    public const string DEFAULT_TRAINING_DATA = "data/default_train";

    Model? model = null;
    State state;
    // This should never be null but it's marked as such because of warning.
    private string? _trainingDataPath;
    string TrainingData
    {
        get => _trainingDataPath ?? DEFAULT_TRAINING_DATA;
        set
        {
            _trainingDataPath = value ?? DEFAULT_TRAINING_DATA;
            model = new(_trainingDataPath);
        }
    }

    static void Main(string[] args)
    {
        Program program = new();
    }


    private Program()
    {
        state = State.START;
        TrainingData = DEFAULT_TRAINING_DATA;
    }
}
