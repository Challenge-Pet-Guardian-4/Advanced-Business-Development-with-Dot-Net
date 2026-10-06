namespace PetGuardian.Application.Common;

/// <summary>Link HATEOAS (href / rel / method).</summary>
public record Link(string Href, string Rel, string Method);