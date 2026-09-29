using Xunit;

namespace PetGuardian.UnitTests.Fixtures;

[CollectionDefinition(Name)]
public class UnitTestCollection : ICollectionFixture<TestFixture>
{
    public const string Name = "UnitTestCollection";
}
