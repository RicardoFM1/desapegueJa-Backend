using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace BackendDesapegaJa.Entities
{
    public class Enderecos
    {
        public int id { get; set; }

        [Required(ErrorMessage = "A referência do usuario deve ser preenchida.")]
        public int usuario_id { get; set; }

        public int? numero { get; set; }

        public string? bairro { get; set; }

        public string? cidade { get; set; }

        public string? estado { get; set; }

        public string? rua { get; set; }

        public string? tipo_de_logradouro { get; set; }

        public string? complemento { get; set; }
        
        public string ?status { get; set; }
    }
    public class EnderecosUpdateDTO
    {
        public int? usuario_id { get; set; }
        public int? numero { get; set; }

        public string? bairro { get; set; }

        public string? cidade { get; set; }

        public string? estado { get; set; }

        public string? rua { get; set; }

        public string? tipo_de_logradouro { get; set; }

        public string? complemento { get; set; }

        public string? status { get; set; }
    }
}
