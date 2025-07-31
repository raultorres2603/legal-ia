using System.Net.Mail;
using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions.Activities;

/// <summary>
/// User-related activity functions
/// </summary>
public class UserActivities(ILogger<UserActivities> logger, IUserService userService)
{
    private readonly ILogger<UserActivities> _logger = logger;

    [Function("ValidateUserActivity")]
    public async Task ValidateUserActivity([ActivityTrigger] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email is required");

        if (string.IsNullOrWhiteSpace(request.DNI))
            throw new ValidationException("DNI is required");

        if (string.IsNullOrWhiteSpace(request.CIF))
            throw new ValidationException("CIF is required");

        // Check if user already exists
        if (await userService.UserExistsByEmailAsync(request.Email))
            throw new ValidationException("User with this email already exists");

        if (await userService.UserExistsByDNIAsync(request.DNI))
            throw new ValidationException("User with this DNI already exists");
    }

    [Function("CreateUserActivity")]
    public async Task<UserResponse> CreateUserActivity([ActivityTrigger] CreateUserRequest request)
    {
        return await userService.CreateUserAsync(request);
    }

    [Function("ValidateUserUpdateActivity")]
    public Task ValidateUserUpdateActivity([ActivityTrigger] UpdateUserRequest request)
    {
        // Add validation logic for update requests
        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            throw new ValidationException("Invalid email format");

        return Task.CompletedTask;
    }

    [Function("UpdateUserActivity")]
    public async Task<UserResponse?> UpdateUserActivity([ActivityTrigger] dynamic input)
    {
        var userId = Guid.Parse(input.UserId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(input.UpdateRequest.ToString());

        return await userService.UpdateUserAsync(userId, updateRequest!);
    }

    [Function("VerifyUserExistsActivity")]
    public async Task VerifyUserExistsActivity([ActivityTrigger] Guid userId)
    {
        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
            throw new ValidationException($"User {userId} not found");
    }

    // Helper methods
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
