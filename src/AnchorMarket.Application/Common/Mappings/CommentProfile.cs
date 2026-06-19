using AnchorMarket.Application.Features.Comments.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Comment"/> and <see cref="CommentDto"/>.</summary>
public class CommentProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public CommentProfile()
    {
        CreateMap<Comment, CommentDto>();
    }
}
