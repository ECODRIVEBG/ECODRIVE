namespace EcoDriveTcc.Repository.Contracts
{
    public interface IHistPgtoRepository
    {
        decimal Recarregar(int idCliente, decimal valor);
    }
}