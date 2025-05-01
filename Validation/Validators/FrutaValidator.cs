using Common.Models;

namespace Validation.Validators;

public class FrutaValidator
{
    public bool Validate(Fruta fruta, out string validationMessage)
    { 
       validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(fruta.Nome))
        {
            validationMessage = "Nome da fruta não pode estar vazio.";
            return false;
        }


        if (string.IsNullOrWhiteSpace(fruta.Descricao))
        {
            validationMessage = "Descrição da fruta não pode estar vazio.";
            return false;
        }


        if (fruta.DataHoraSolicitacao == default)
        {
            validationMessage = "Data e hora de solicitação inválida.";
            return false;
        }

        return true;
    }
}
