namespace Common.Models;

public class Fruta
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public DateTime DataHoraSolicitacao { get; set; }

    public override string ToString()
    {
        return $"Fruta: {Nome}\nDescrição: {Descricao}\nData/Hora: {DataHoraSolicitacao}";
    }
}
