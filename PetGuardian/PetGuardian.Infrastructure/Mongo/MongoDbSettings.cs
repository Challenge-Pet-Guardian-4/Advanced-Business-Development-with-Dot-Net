namespace PetGuardian.Infrastructure.Mongo;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "petguardian_nosql";
    public string TrilhasCollection { get; set; } = "trilhas_educativas";
    public int ServerSelectionTimeoutSeconds { get; set; } = 5;
}