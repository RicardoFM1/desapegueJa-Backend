using System;
using System.Text;

namespace BackendDesapegaJa.Helpers
{
    public static class PixGenerator
    {
       
        private static string FormatarCampo(string id, string valor)
        {
            var val = valor ?? string.Empty;
            return $"{id}{val.Length:D2}{val}";
        }

      
        private static string CalcularCRC16(string data)
        {
            int crc = 0xFFFF;
            int polynomial = 0x1021;
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            foreach (byte b in bytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool bit = ((b >> (7 - i) & 1) == 1);
                    bool c15 = ((crc >> 15 & 1) == 1);
                    crc <<= 1;
                    if (c15 ^ bit) crc ^= polynomial;
                }
            }

            return (crc & 0xFFFF).ToString("X4");
        }

        public static string GerarPayloadPix(string chave, string nome, string cidade, string txid, decimal valor)
        {
        
            nome = nome.Length > 25 ? nome.Substring(0, 25) : nome;
            cidade = cidade.Length > 15 ? cidade.Substring(0, 15) : cidade;

           
            string valorString = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

           
            string gui = FormatarCampo("00", "br.gov.bcb.pix");
            string key = FormatarCampo("01", chave);
            string campo26 = FormatarCampo("26", gui + key);

       
            StringBuilder sb = new StringBuilder();
            sb.Append(FormatarCampo("00", "01")); 
            sb.Append(FormatarCampo("01", "12")); 
            sb.Append(campo26);
            sb.Append(FormatarCampo("52", "0000")); 
            sb.Append(FormatarCampo("53", "986"));  
            sb.Append(FormatarCampo("54", valorString)); 
            sb.Append(FormatarCampo("58", "BR"));
            sb.Append(FormatarCampo("59", nome.ToUpper())); 
            sb.Append(FormatarCampo("60", cidade.ToUpper())); 
            sb.Append(FormatarCampo("62", FormatarCampo("05", txid))); 
            sb.Append("6304"); 

            
            string payloadSemCrc = sb.ToString();
            string crc = CalcularCRC16(payloadSemCrc);

            return payloadSemCrc + crc;
        }
    }
}