namespace AgendAi.Application.Clientes.ResolverCliente;

using AgendAi.Application.Clientes.Ports;
using AgendAi.Application.Common.Exceptions;
using AgendAi.Domain.Clientes;

public sealed class ResolverClienteHandler
{
    private const int TamanhoMaximoNome = 150;
    private const int TamanhoMinimoTelefone = 10;
    private const int TamanhoMaximoTelefone = 15;
    private const int TamanhoMaximoTelegramUserId = 30;

    private readonly IResolucaoClienteReader _resolucaoClienteReader;
    private readonly IClienteWriter _clienteWriter;

    public ResolverClienteHandler(
        IResolucaoClienteReader resolucaoClienteReader,
        IClienteWriter clienteWriter
    )
    {
        _resolucaoClienteReader = resolucaoClienteReader;
        _clienteWriter = clienteWriter;
    }

    public async Task<ResolverClienteResult> HandleAsync(
        ResolverClienteCommand command,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidarCommand(command);

        var nome = command.Nome.Trim();
        var telefone = NormalizarTelefone(command.Telefone);
        var telegramUserId = string.IsNullOrWhiteSpace(command.TelegramUserId) ? null : command.TelegramUserId.Trim();

        ValidarDados(
            telefone,
            telegramUserId
        );

        var dados = await _resolucaoClienteReader.ObterAsync(
            command.ClinicaUuid,
            telefone,
            telegramUserId,
            cancellationToken
        ) ?? throw new RecursoNaoEncontradoException(
            codigo: "clinica_nao_encontrada",
            mensagem: "A clínica informada não foi encontrada."
        );

        if (dados.ClientePorTelefone is not null)
        {
            var cliente = dados.ClientePorTelefone;

            if (dados.ClientePorTelegram is not null && dados.ClientePorTelegram.Uuid != cliente.Uuid)
            {
                throw CriarConflitoIdentidade();
            }

            if (telegramUserId is not null)
            {
                if (
                    !string.IsNullOrWhiteSpace(cliente.IdTelegram) 
                    && !string.Equals(cliente.IdTelegram, telegramUserId, StringComparison.Ordinal)
                )
                {
                    throw CriarConflitoIdentidade();
                }

                var needVincularTelegram = string.IsNullOrWhiteSpace(cliente.IdTelegram);

                cliente.VincularTelegram(telegramUserId);

                if (needVincularTelegram)
                {
                    await _clienteWriter.AtualizarAsync(
                        cliente,
                        cancellationToken
                    );
                }
            }

            return MapearResult(
                cliente,
                criado: false
            );
        }

        if (dados.ClientePorTelegram is not null)
        {
            throw CriarConflitoIdentidade();
        }

        var newCliente = Cliente.Criar(
            dados.Clinica,
            nome,
            telefone,
            telegramUserId
        );

        await _clienteWriter.InserirAsync(
            newCliente,
            cancellationToken
        );

        return MapearResult(
            newCliente,
            criado: true
        );
    }

    private static void ValidarCommand(ResolverClienteCommand command)
    {
        if (command.ClinicaUuid == Guid.Empty)
        {
            throw new ValidacaoException(
                codigo: "clinica_uuid_invalido",
                mensagem: "O UUID da clínica é obrigatório."
            );
        }

        if (string.IsNullOrWhiteSpace(command.Nome))
        {
            throw new ValidacaoException(
                codigo: "nome_cliente_obrigatorio",
                mensagem: "O nome do cliente é obrigatório."
            );
        }

        if (command.Nome.Trim().Length > TamanhoMaximoNome)
        {
            throw new ValidacaoException(
                codigo: "nome_cliente_invalido",
                mensagem: $"O nome do cliente deve possuir no máximo {TamanhoMaximoNome} caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(command.Telefone))
        {
            throw new ValidacaoException(
                codigo: "telefone_cliente_obrigatorio",
                mensagem: "O telefone do cliente é obrigatório."
            );
        }
    }

    private static void ValidarDados(
        string telefone,
        string? telegramUserId
    )
    {
        if (telefone.Length < TamanhoMinimoTelefone || telefone.Length > TamanhoMaximoTelefone)
        {
            throw new ValidacaoException(
                codigo: "telefone_cliente_invalido",
                mensagem: $"O telefone do cliente deve possuir entre {TamanhoMinimoTelefone} e {TamanhoMaximoTelefone} caracteres."
            );
        }

        if (telegramUserId is null)
        {
            return;
        }

        if (
            telegramUserId.Length > TamanhoMaximoTelegramUserId 
            || telegramUserId.Any(cat => cat < '0' || cat > '9')
        )
        {
            throw new ValidacaoException(
                codigo: "telegram_user_id_invalido",
                mensagem: $"O identificador do Telegram deve conter somente números e ter no máximo {TamanhoMaximoTelegramUserId} caracteres."
            );
        }
    }

    private static string NormalizarTelefone(string telefone)
    {
        return new string(
            telefone.Where(cat => cat >= '0' && cat <= '9').ToArray()
        );
    }

    private static ConflitoException CriarConflitoIdentidade()
    {
        return new ConflitoException(
            codigo: "identidade_cliente_conflitante",
            mensagem: "O telefone e o usuário do Telegram estão vinculados a clientes diferentes.."
        );
    }

    private static ResolverClienteResult MapearResult(
        Cliente cliente,
        bool criado
    )
    {
        return new ResolverClienteResult(
            ClienteUuid: cliente.Uuid,
            Criado: criado,
            Nome: cliente.Nome,
            Telefone: cliente.Telefone
        );
    }

}