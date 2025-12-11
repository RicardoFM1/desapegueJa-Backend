using System.ComponentModel.DataAnnotations;

namespace BackendDesapegaJa.Entities
{
    public class ItemFreteRequest
    {

        public int ProdutoId { get; set; }

        public int UsuarioId { get; set; }
        public int PrecoUnitario{ get; set; }
        public int Quantidade { get; set; }
      
    }

    public class DTOFreteRequest
    {
       
        [Required(ErrorMessage = "O CEP de destino é obrigatório para o cálculo do frete.")]
        public string? CepDestino { get; set; } 

        [Required(ErrorMessage = "O carrinho não pode estar vazio.")]
        public List<ItemFreteRequest> Itens { get; set; } = new List<ItemFreteRequest>();
    }
}

