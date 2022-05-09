using System.Net;
using Application.Errors;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users;

public class Register
{
    public class RegisterUserCommand:IRequest<User>
    {
        public string  Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }}
    public class CommandValidator: AbstractValidator<RegisterUserCommand>
    {
        public CommandValidator()
        {
            RuleFor((x => x.Name)).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
        
    }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, User>
    {
        private readonly DataContext _context;

        public RegisterUserCommandHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Email.Trim().ToLower().Equals(request.Email.Trim().ToLower()), cancellationToken);
           if (user != null)
               throw new RestException(HttpStatusCode.Conflict, $"User With email {request.Email} already exists");
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim().ToLower(),
            Password = hashedPassword
        };
        await _context.Users.AddAsync(user, cancellationToken);
        if (await _context.SaveChangesAsync(cancellationToken) > 0) return user;

        throw new Exception("Could not add new user");

        }
    }
}