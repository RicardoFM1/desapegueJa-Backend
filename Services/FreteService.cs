using BackendDesapegaJa.Entities;
using BackendDesapegaJa.Interfaces;

namespace BackendDesapegaJa.Services
{
  
    public class FreteService 
    {
        private readonly IUsuarioRepository _repoUser;
      

        public FreteService(IUsuarioRepository repoUser)
        {
            _repoUser = repoUser;
        }

        public async Task<int> CalcularFreteTotalAsync(DTOFreteRequest request)
        {
            int freteTotal = 0;

          
            var vendedoresIds = request.Itens.Select(i => i.UsuarioId).Distinct();

           
            var cepsOrigem = await _repoUser.BuscarCepsPorIdsAsync(vendedoresIds);

           
            var pacotes = request.Itens.GroupBy(i => i.UsuarioId)
                .Select(g => new
                {
                    VendedorId = g.Key,
                  
                    CepOrigem = cepsOrigem.GetValueOrDefault(g.Key) ?? string.Empty,
                    Itens = g.ToList(),
                    SubtotalDoVendedor = g.Sum(i => i.PrecoUnitario * i.Quantidade)
                })
                .ToList();

         
            foreach (var pacote in pacotes)
            {
                
                if (string.IsNullOrEmpty(pacote.CepOrigem))
                {
                   
                    continue;
                }

               
                int freteDoVendedor = CalcularFreteFixo(pacote.CepOrigem, request.CepDestino, pacote.SubtotalDoVendedor);
                
            }

            return freteTotal;
        }

      
        private int CalcularFreteFixo(string cepOrigem, string cepDestino, int subtotal)
        {
          
            if (cepOrigem.Substring(0, 2) == cepDestino.Substring(0, 2))
            {
                return 1000; 
            }
            else if (subtotal > 50000)
            {
                return 5000; 
            }
            else
            {
                return 3000; 
            }
        }
    }
}