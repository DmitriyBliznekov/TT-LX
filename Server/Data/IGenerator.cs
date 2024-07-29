namespace Server.Data;

public interface IGenerator<out T>
{
    T Generate();

    IEnumerable<T> GenerateSet(int number);
}