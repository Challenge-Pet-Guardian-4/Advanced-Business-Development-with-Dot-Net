using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

/// <summary>Serviços de consulta (leitura paginada), separados dos serviços de comando.</summary>
public interface IPetQueryService      { PagedResponse<PetResponse>       Search(PageQuery query, PetFilter filter); }
public interface ITarefaQueryService   { PagedResponse<TarefaResponse>    Search(PageQuery query, TarefaFilter filter); }
public interface IUsuarioQueryService  { PagedResponse<UsuarioResponse>   Search(PageQuery query, UsuarioFilter filter); }
public interface IHistoricoQueryService{ PagedResponse<HistoricoResponse> Search(PageQuery query, HistoricoFilter filter); }