namespace AiSdk.Infrastructure;

public class AiAnyOf<T1, T2> : IAiAnyOf
{
    public object? Value { get; set; }

    public AiAnyOf(T1 value) => Value = value;
    public AiAnyOf(T2 value) => Value = value;

    public T1? AsT1 => Value is T1 t1 ? t1 : default;
    public T2? AsT2 => Value is T2 t2 ? t2 : default;

    public static implicit operator AiAnyOf<T1, T2>(T1 val) => new(val);
    public static implicit operator AiAnyOf<T1, T2>(T2 val) => new(val);
}
