using CommandLine;

//using System.Security;

namespace EHN_Macros
{
    public class Login
    {
        [Option('u', Required = true, HelpText = "Usuario")]
        public string User { get; set; }

        [Option('p', Required = true, HelpText = "Contraseña")]
        public string Password { get; set; }
    }
}