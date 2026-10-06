namespace PetGuardian.Application.DTOs;

public record SincronizacaoCatalogoResponse(
    int TrilhasRelacionais, int Inseridas, int Atualizadas, long Removidas, DateTime SincronizadoEm);