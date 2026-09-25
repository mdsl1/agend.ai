import type { MeuPerfilApi } from './authApi'

type ProfilePresentationData = Pick<MeuPerfilApi, 'cargo' | 'prefixo'> & {
  clinica: Pick<MeuPerfilApi['clinica'], 'tipoClinica'>
}

function normalizePrefix(prefix: string | null) {
  return prefix?.trim().toLocaleLowerCase('pt-BR').replace(/\./g, '')
}

export function getDisplayedRole({
  cargo,
  prefixo,
  clinica,
}: ProfilePresentationData) {
  if (cargo !== 'Profissional') {
    return cargo
  }

  if (clinica.tipoClinica === 'estetica') {
    return 'Profissional'
  }

  const normalizedPrefix = normalizePrefix(prefixo)

  if (normalizedPrefix === 'dr') {
    return 'Doutor'
  }

  if (normalizedPrefix === 'dra') {
    return 'Doutora'
  }

  return 'Profissional'
}
