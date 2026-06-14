namespace BVRTK.Data.Response;

public class GenericResponse
{
    public bool? Done { get; init; }
    public string? Message { get; init; }
    public List<string>? List { get; init; }
    public Dictionary<string, string>? Dictionary { get; init; }
}