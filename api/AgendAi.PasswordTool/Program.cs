using System.Text;
using Microsoft.AspNetCore.Identity;

Console.Write("Digite a senha: ");

var senha = LerSenha();

Console.WriteLine();

if (string.IsNullOrWhiteSpace(senha))
{
    Console.Error.WriteLine("A senha não pode ser vazia.");
    return;
}

var passwordHasher = new PasswordHasher<object>();

var senhaHash = passwordHasher.HashPassword(
    new object(),
    senha
);

Console.WriteLine();
Console.WriteLine("Hash gerado:");
Console.WriteLine(senhaHash);

static string LerSenha()
{
    var caracteres = new StringBuilder();

    while (true)
    {
        var tecla = Console.ReadKey(intercept: true);

        if (tecla.Key == ConsoleKey.Enter)
        {
            break;
        }

        if (tecla.Key == ConsoleKey.Backspace)
        {
            if (caracteres.Length > 0)
            {
                caracteres.Length--;
            }

            continue;
        }

        if (!char.IsControl(tecla.KeyChar))
        {
            caracteres.Append(tecla.KeyChar);
        }
    }

    return caracteres.ToString();
}