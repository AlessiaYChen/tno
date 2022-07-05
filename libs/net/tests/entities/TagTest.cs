namespace TNO.Test.Entities;

using System.Diagnostics.CodeAnalysis;

[Trait("category", "unit")]
[Trait("group", "entities")]
[ExcludeFromCodeCoverage]
public class TagTest
{
    [Fact]
    public void Constructor1()
    {
        // Arrange
        // Act
        var item = new TNO.Entities.Tag("test", "test");

        // Assert
        Assert.Equal("test", item.Id);
        Assert.Equal("test", item.Name);
        Assert.Equal("", item.Description);
        Assert.True(item.IsEnabled);
        Assert.Equal(0, item.SortOrder);

        Assert.Equal("", item.CreatedBy);
        Assert.Equal(Guid.Empty, item.CreatedById);
        Assert.Equal(DateTime.MinValue, item.CreatedOn);
        Assert.Equal("", item.UpdatedBy);
        Assert.Equal(Guid.Empty, item.UpdatedById);
        Assert.Equal(DateTime.MinValue, item.UpdatedOn);
        Assert.Equal(0, item.Version);

        Assert.Empty(item.Contents);
        Assert.Empty(item.ContentsManyToMany);
    }
}