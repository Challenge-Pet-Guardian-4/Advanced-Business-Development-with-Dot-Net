using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

/// <summary>Casos de uso de endereço.</summary>
public interface IEnderecoService
{
    IReadOnlyList<EnderecoResponse> GetAll();
    EnderecoResponse? GetById(Guid id);
    EnderecoResponse Create(EnderecoRequest request);
    EnderecoResponse? Update(Guid id, EnderecoRequest request);
    bool Delete(Guid id);
}