namespace AgendAi.Application.Auth.Permissions;

public static class PermissoesUsuario
{
    // Permissoes definidas no padrão "recurso:ação:escopo"
    
    // Permissoes de Profissional
    public const string AgendaPropriaVisualizar = "agenda:visualizar:propria";

    public const string AgendamentosPropriosGerenciar = "agendamentos:gerenciar:proprios";

    public const string IndisponibilidadesPropriasGerenciar = "indisponibilidades:gerenciar:proprias";

    public const string ProcedimentosPropriosGerenciar = "procedimentos:gerenciar:proprios";

    public const string ReceitasPropriasVisualizar = "receitas:visualizar:proprias";

    // Permissoes de Recepcionista
    public const string AgendaClinicaVisualizar = "agenda:visualizar:clinica";

    public const string AgendamentosClinicaGerenciar = "agendamentos:gerenciar:clinica";

    public const string ClientesClinicaGerenciar = "clientes:gerenciar:clinica";

    public const string ProcedimentosClinicaGerenciar = "procedimentos:gerenciar:clinica";

    public const string EspecialidadesClinicaGerenciar = "especialidades:gerenciar:clinica";

    public const string ProfissionaisClinicaGerenciar = "profissionais:gerenciar:clinica";

    // Permissoes de Admin
    public const string ClinicaGerenciar = "clinica:gerenciar";

    public const string UsuariosClinicaGerenciar = "usuarios:gerenciar:clinica";
}