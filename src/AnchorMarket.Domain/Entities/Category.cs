namespace AnchorMarket.Domain.Entities;

/// <summary>A hierarchical category used to organize markets and events for discovery.</summary>
public class Category : BaseEntity
{
    /// <summary>Gets the display name of the category.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the URL-friendly slug for the category.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Gets an optional icon identifier for the category.</summary>
    public string? Icon { get; private set; }

    /// <summary>Null for top-level categories; set for subcategories.</summary>
    public Guid? ParentCategoryId { get; private set; }

    /// <summary>Gets the parent category, or null if this is a top-level category.</summary>
    public Category? ParentCategory { get; private set; }

    /// <summary>Gets the subcategories nested under this category.</summary>
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();

    /// <summary>Gets the markets belonging to this category.</summary>
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    /// <summary>Gets the events belonging to this category.</summary>
    public ICollection<Event> Events { get; private set; } = new List<Event>();

    /// <summary>Creates a new category.</summary>
    /// <param name="name">The display name.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="icon">An optional icon identifier.</param>
    /// <param name="parentCategoryId">Optional parent category ID for subcategories.</param>
    /// <returns>A new <see cref="Category"/> instance.</returns>
    public static Category Create(string name, string slug, string? icon = null, Guid? parentCategoryId = null)
    {
        return new Category
        {
            Name = name,
            Slug = slug,
            Icon = icon,
            ParentCategoryId = parentCategoryId
        };
    }
}
