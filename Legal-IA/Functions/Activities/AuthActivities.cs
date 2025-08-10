using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

public class RegisterUserActivity(IUserService userService, ILogger<RegisterUserActivity> logger)
{
    [Function("RegisterUserActivity")]
    public async Task<AuthResponse> Run([ActivityTrigger] RegisterUserRequest request)
    {
        logger.LogInformation($"[RegisterUserActivity] Activity started for email: {request.Email}");
        try
        {
            logger.LogInformation("RegisterUserActivity started for email: {Email}", request.Email);
            // Check if user already exists
            var existingUser = await userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                logger.LogWarning("User already exists with email: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "User already exists." };
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            // Generate email verification token and expiration
            var verificationToken = Guid.NewGuid().ToString();
            var tokenExpiresAt = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

            var user = new CreateUserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                DNI = request.DNI,
                CIF = request.CIF,
                BusinessName = request.BusinessName,
                Address = request.Address,
                PostalCode = request.PostalCode,
                City = request.City,
                Province = request.Province,
                Phone = request.Phone,
                Password = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = tokenExpiresAt
            };
            var createdUser = await userService.CreateUserAsync(user);

            // Send email verification
            var notificationService = new NotificationService(logger);
            await notificationService.SendEmailVerificationAsync(user.Email, user.FirstName, verificationToken);

            logger.LogInformation("User registered successfully: {Email}", user.Email);
            logger.LogInformation($"[RegisterUserActivity] Activity completed for email: {request.Email}");
            return new AuthResponse { Success = true, Data = new { userId = createdUser.Id } };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RegisterUserActivity for email: {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Registration failed." };
        }
    }
}

public class LoginUserActivity(IUserService userService, ILogger<LoginUserActivity> logger, JwtService jwtService)
{
    [Function("LoginUserActivity")]
    public async Task<AuthResponse> Run([ActivityTrigger] LoginUserRequest request)
    {
        logger.LogInformation($"[LoginUserActivity] Activity started for email: {request.Email}");
        try
        {
            logger.LogInformation("LoginUserActivity started for email: {Email}", request.Email);
            var user = await userService.GetUserEntityByEmailAsync(request.Email);
            if (user == null || string.IsNullOrWhiteSpace(user.Password))
            {
                logger.LogWarning("User not found or password missing for email: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Invalid credentials." };
            }

            logger.LogInformation("User logged in: {Email} with password: {Password}", user.Email, "********");
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                logger.LogWarning("Invalid password for email: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Invalid credentials." };
            }

            // Generate JWT
            var token = jwtService.GenerateToken(user);
            logger.LogInformation("Login successful for email: {Email}", request.Email);
            logger.LogInformation($"[LoginUserActivity] Activity completed for email: {request.Email}");
            return new AuthResponse { Success = true, Data = new { token } };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in LoginUserActivity for email: {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Login failed." };
        }
    }
}

public class VerifyUserEmailActivity(IUserService userService, ILogger<VerifyUserEmailActivity> logger)
{
    [Function("VerifyUserEmailActivity")]
    public async Task<AuthResponse> Run([ActivityTrigger] string token)
    {
        logger.LogInformation($"[VerifyUserEmailActivity] Started for token: {token}");
        if (string.IsNullOrEmpty(token))
            return new AuthResponse { Success = false, Message = "Invalid verification token." };

        var user = await userService.GetUserByVerificationTokenAsync(token);
        if (user == null)
            return new AuthResponse { Success = false, Message = "Invalid or expired verification token." };

        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return new AuthResponse { Success = false, Message = "Verification token has expired." };

        user.IsActive = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        await userService.UpdateUserAsync(user);

        logger.LogInformation($"[VerifyUserEmailActivity] User {user.Email} verified successfully.");
        return new AuthResponse { Success = true, Message = "Email verified successfully." };
    }
}
