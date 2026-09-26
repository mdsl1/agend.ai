namespace AgendAi.API.Controllers;

using AgendAi.API.Contracts.Clientes;
using AgendAi.Application.Clientes.ResolverCliente;
using AgendAi.Application.Clientes.ListarClientes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly ResolverClienteHandler _resolverClienteHandler;
    private readonly ListarClientesHandler _listarClientesHandler;

    public ClientesController( 
        ResolverClienteHandler resolverClienteHandler,
        ListarClientesHandler listarClientesHandler
    )
    {
        _resolverClienteHandler = resolverClienteHandler;
        _listarClientesHandler = listarClientesHandler;
    }

    [HttpGet]
    [ProducesResponseType( typeof(ListarClientesResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ListarClientesResponse>> ListarAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _listarClientesHandler.HandleAsync( new ListarClientesQuery(), cancellationToken );
        
        var res = new ListarClientesResponse(
            result.Clientes
                .Select(cliente => new ClienteResponse(
                    Uuid: cliente.Uuid,
                    Nome: cliente.Nome,
                    Telefone: cliente.Telefone,
                    Email: cliente.Email,
                    DataNascimento: cliente.DataNascimento,
                    Genero: cliente.Genero
                )).ToArray()
        );

        return Ok(res);
    }


    [HttpPost("resolver")]
    [ProducesResponseType( typeof(ResolverClienteResponse), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof(ResolverClienteResponse), StatusCodes.Status201Created )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status409Conflict )]
    [ProducesResponseType( StatusCodes.Status401Unauthorized )]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status403Forbidden )]
    public async Task<ActionResult<ResolverClienteResponse>> ResolverAsync(
        [FromBody] ResolverClienteRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new ResolverClienteCommand(
            Nome: req.Nome,
            Telefone: req.Telefone,
            TelegramUserId: req.TelegramUserId
        );

        var result = await _resolverClienteHandler.HandleAsync(
            command,
            cancellationToken
        );

        var res = new ResolverClienteResponse(
            ClienteUuid: result.ClienteUuid,
            Criado: result.Criado,
            Nome: result.Nome,
            Telefone: result.Telefone
        );

        if (res.Criado)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                res
            );
        }

        return Ok(res);
    }


}