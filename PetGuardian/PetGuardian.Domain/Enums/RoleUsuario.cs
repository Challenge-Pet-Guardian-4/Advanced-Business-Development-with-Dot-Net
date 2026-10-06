namespace PetGuardian.Domain.Enums;

/// <summary>
/// Perfil de acesso. Persistido como string — CHECK (role IN ('ADMIN','COMUM','PREMIUM')).
/// </summary>
public enum RoleUsuario
{
    Comum = 0,
    Premium = 1,
    Admin = 2
}