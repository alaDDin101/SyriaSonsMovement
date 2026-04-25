using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginClientAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> LoginDashboardAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task SubmitJoinRequestAsync(JoinMovementRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberProfileDto?> GetClientProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MemberProfileDto?> UpdateClientProfileAsync(Guid userId, UpdateMemberProfileDto request, CancellationToken cancellationToken = default);
}
