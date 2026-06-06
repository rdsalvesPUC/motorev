using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Services;

public class ClienteService
{
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public ClienteService() { } // Construtor para Moq

    public ClienteService(AppDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public virtual async Task<ClienteResponse> RegisterAsync(RegisterClienteRequest request)
    {
        var cpf = NormalizeCpf(request.Cpf);
        var telefone = NormalizeTelefone(request.Telefone);

        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            throw new DuplicateDataException($"O email {request.Email} já está em uso.");
        }

        if (await _context.Clientes.AnyAsync(c => c.Cpf == cpf))
        {
            throw new DuplicateDataException($"O CPF {request.Cpf} já está em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new Usuario { UserName = request.Email, Email = request.Email, PhoneNumber = telefone };
            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded) throw new RegistrationException(identityResult.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Cliente);

            if (!roleResult.Succeeded) throw new RegistrationException(roleResult.Errors);

            var cliente = request.Adapt<Cliente>();
            cliente.UsuarioId = user.Id;
            cliente.Cpf = cpf;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return cliente.Adapt<ClienteResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<ClienteResponse> GetByUserIdAsync(string userId)
    {
        var cliente = await _context.Clientes
            .Where(c => c.UsuarioId == userId)
            .ProjectToType<ClienteResponse>()
            .FirstOrDefaultAsync();

        return cliente ?? throw new NotFoundException($"Cliente não encontrado.");
    }

    public virtual async Task<ClientePerfilResponse> GetPerfilByUserIdAsync(string userId)
    {
        var cliente = await GetClienteWithUsuarioAsync(userId);
        return ToPerfilResponse(cliente);
    }

    public virtual async Task<ClientePerfilResponse> UpdateDadosPessoaisAsync(
        string userId,
        ClienteDadosPessoaisRequest request)
    {
        var cpf = NormalizeCpf(request.Cpf);
        var telefone = NormalizeTelefone(request.Telefone);
        var cliente = await GetClienteWithUsuarioAsync(userId);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != cliente.UsuarioId)
        {
            throw new DuplicateDataException($"O email {request.Email} já está em uso.");
        }

        if (await _context.Clientes.AnyAsync(c => c.Cpf == cpf && c.Id != cliente.Id))
        {
            throw new DuplicateDataException($"O CPF {request.Cpf} já está em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            cliente.Nome = request.Nome;
            cliente.Cpf = cpf;

            if (!string.Equals(cliente.Usuario.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(cliente.Usuario, request.Email);
                if (!emailResult.Succeeded) throw new RegistrationException(emailResult.Errors);

                var userNameResult = await _userManager.SetUserNameAsync(cliente.Usuario, request.Email);
                if (!userNameResult.Succeeded) throw new RegistrationException(userNameResult.Errors);

                cliente.Usuario.Email = request.Email;
                cliente.Usuario.UserName = request.Email;
            }

            var phoneResult = await _userManager.SetPhoneNumberAsync(cliente.Usuario, telefone);
            if (!phoneResult.Succeeded) throw new RegistrationException(phoneResult.Errors);
            cliente.Usuario.PhoneNumber = telefone;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ToPerfilResponse(cliente);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<ClientePerfilResponse> UpdateEnderecoAsync(
        string userId,
        ClienteEnderecoRequest request)
    {
        var cliente = await GetClienteWithUsuarioAsync(userId);

        cliente.Cep = NormalizeNullableDigits(request.Cep, "CEP", 8);
        cliente.Logradouro = NormalizeNullableText(request.Logradouro);
        cliente.Numero = NormalizeNullableText(request.Numero);
        cliente.Complemento = NormalizeNullableText(request.Complemento);
        cliente.Bairro = NormalizeNullableText(request.Bairro);
        cliente.Cidade = NormalizeNullableText(request.Cidade);
        cliente.Uf = NormalizeUf(request.Uf);

        await _context.SaveChangesAsync();
        return ToPerfilResponse(cliente);
    }

    public virtual async Task AlterarSenhaAsync(string userId, ClienteAlterarSenhaRequest request)
    {
        if (request.NovaSenha != request.ConfirmarNovaSenha)
        {
            throw new ValidationException("A confirmação da nova senha não confere.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Usuário não encontrado.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.SenhaAtual, request.NovaSenha);
        if (!result.Succeeded)
        {
            throw new RegistrationException(result.Errors);
        }
    }

    private async Task<Cliente> GetClienteWithUsuarioAsync(string userId)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        return cliente ?? throw new NotFoundException("Cliente não encontrado.");
    }

    private static ClientePerfilResponse ToPerfilResponse(Cliente cliente)
    {
        return new ClientePerfilResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Usuario.Email ?? string.Empty,
            cliente.Cpf ?? string.Empty,
            cliente.Usuario.PhoneNumber,
            new ClienteEnderecoResponse(
                cliente.Cep,
                cliente.Logradouro,
                cliente.Numero,
                cliente.Complemento,
                cliente.Bairro,
                cliente.Cidade,
                cliente.Uf));
    }

    private static string NormalizeCpf(string cpf)
    {
        var digits = NormalizeDigits(cpf);
        if (!IsValidCpf(digits))
        {
            throw new ValidationException("CPF inválido.");
        }

        return digits;
    }

    private static string NormalizeTelefone(string telefone)
    {
        var digits = NormalizeDigits(telefone);
        if (digits.Length is < 10 or > 11)
        {
            throw new ValidationException("Telefone inválido.");
        }

        return digits;
    }

    private static string? NormalizeNullableDigits(string? value, string fieldName, int expectedLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var digits = NormalizeDigits(value);
        if (digits.Length != expectedLength)
        {
            throw new ValidationException($"{fieldName} inválido.");
        }

        return digits;
    }

    private static string? NormalizeUf(string? uf)
    {
        if (string.IsNullOrWhiteSpace(uf)) return null;

        var normalized = uf.Trim().ToUpperInvariant();
        if (normalized.Length != 2)
        {
            throw new ValidationException("UF inválida.");
        }

        return normalized;
    }

    private static string? NormalizeNullableText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeDigits(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = cpf.Select(c => c - '0').ToArray();
        var firstDigit = CalculateCpfDigit(numbers, 9);
        var secondDigit = CalculateCpfDigit(numbers, 10);

        return numbers[9] == firstDigit && numbers[10] == secondDigit;
    }

    private static int CalculateCpfDigit(int[] numbers, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += numbers[i] * (length + 1 - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
