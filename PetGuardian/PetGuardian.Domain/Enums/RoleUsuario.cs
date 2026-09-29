namespace PetGuardian.Domain.Enums;

/// <summary>
/// Nível de plano/acesso do usuário.
/// Persistido como string no banco — CHECK (role IN ('COMUM','PREMIUM')).
/// </summary>
public enum RoleUsuario
{
    Comum = 0,
    Premium = 1
}
