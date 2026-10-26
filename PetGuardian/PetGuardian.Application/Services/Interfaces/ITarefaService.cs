using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface ITarefaService
{
    PagedResponse<TarefaResponse> Search(PageQuery query, TarefaFilter filter);
    TarefaResponse? GetById(Guid id);
    IReadOnlyList<TarefaResponse> GetByPetId(Guid petId);
    IReadOnlyList<TarefaResponse> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<TarefaResponse> GetByStatusId(Guid statusId);
    TarefaResponse Create(TarefaRequest request);
    TarefaResponse? Update(Guid id, TarefaUpdateRequest request);
    TarefaResponse Concluir(Guid tarefaId, Guid usuarioId);
    bool Delete(Guid id);
}
