namespace BackendDesapegaJa.Entities
{
    public class LoginResponse
    {
        public int Id { get; set; }          
        public string Email { get; set; }   

        public string Admin { get; set; }
        public string Token { get; set; }
    }
}
