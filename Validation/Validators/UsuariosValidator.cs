using Common.Models;
using System.Text.RegularExpressions;

namespace Validation.Validators;

public class UsuariosValidator
{
    public bool Validate(Usuario usuario, out string validationMessage) 
    {
        validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(usuario.NomeCompleto))
        {
            validationMessage = "Nome do usuário não pode estar vazio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(usuario.Endereco))
        {
            validationMessage = "Endereço do usuário não pode estar vazio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(usuario.RG))
        {
            validationMessage = "RG do usuário não pode estar vazio.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(usuario.CPF))
        {
            validationMessage = "CPF do usuário não pode estar vazio.";
            return false;
        }

        // Validação básica de CPF (apenas formato)
        if (!Regex.IsMatch(usuario.CPF, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$") &&
            !Regex.IsMatch(usuario.CPF, @"^\d{11}$"))
        {
            validationMessage = "CPF em formato inválido. Use: 000.000.000-00 ou 00000000000";
            return false;
        }

        if (usuario.DataHoraRegistro == default)
        {
            validationMessage = "Data e hora de registro inválido.";
            return false;
        }

        return true;
    }
}
