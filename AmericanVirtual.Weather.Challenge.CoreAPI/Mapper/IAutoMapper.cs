namespace AmericanVirtual.Weather.Challenge.CoreAPI.Mapper
{
    public interface IAutoMapper
    {
        TOutput Map<TInput, TOutput>(TInput input);

        TOutput Map<TInput, TOutput>(TInput input, TOutput output);

        IEnumerable<TOutput> Map<TInput, TOutput>(IEnumerable<TInput> input);
    }
}
