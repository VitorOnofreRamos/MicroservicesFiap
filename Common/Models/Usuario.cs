namespace Common.Models;

public class Usuario
{
    public string NomeCompleto { get; set; }
    public string Endereco { get; set; }
    public string RG { get; set; }
    public string CPF { get; set; }
    public DateTime DataHoraRegistro { get; set; }

    public override string ToString()
    {
        return $"Nome: {NomeCompleto}\nEndereço: {Endereco}\nRG: {RG}\nCPF: {CPF}\nData/Hora Registro: {DataHoraRegistro}";
    }
}
