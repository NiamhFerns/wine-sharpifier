namespace sharpifier;

class Bound<T>
{
    public Bound(T lower, T upper)
    {
        this.lower = lower;
        this.upper = upper;
    }

    public T upper { get; set; }
    public T lower { get; set; }
}
