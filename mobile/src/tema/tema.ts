import { useColorScheme } from 'react-native';

export type CoresTema = {
    fundo: string;
    superficie: string;
    superficieSecundaria: string;
    borda: string;
    bordaForte: string;
    texto: string;
    textoSecundario: string;
    textoSuave: string;
    textoPlaceholder: string;
    marca: string;
    marcaEscura: string;
    marcaSuave: string;
    textoSobreMarca: string;
    sucesso: string;
    fundoSucesso: string;
    aviso: string;
    fundoAviso: string;
    informacao: string;
    fundoInformacao: string;
    perigo: string;
    fundoPerigo: string;
    perigoStatus: string;
    fundoPerigoStatus: string;
};

const temaClaro: CoresTema = {
    fundo: '#F8FAFC',
    superficie: '#FFFFFF',
    superficieSecundaria: '#F1F5F9',
    borda: '#E2E8F0',
    bordaForte: '#CBD5E1',
    texto: '#131B2E',
    textoSecundario: '#515F74',
    textoSuave: '#6D797D',
    textoPlaceholder: '#849398',
    marca: '#007D94',
    marcaEscura: '#006578',
    marcaSuave: '#E6F5F8',
    textoSobreMarca: '#F9FDFF',
    sucesso: '#166534',
    fundoSucesso: '#DCFCE7',
    aviso: '#92400E',
    fundoAviso: '#FEF3C7',
    informacao: '#1E7AAF',
    fundoInformacao: '#DAEFFD',
    perigo: '#B42318',
    fundoPerigo: '#FEE2E2',
    perigoStatus: '#B91C1C',
    fundoPerigoStatus: '#FEE2E2',
};

const temaEscuro: CoresTema = {
    fundo: '#0B1220',
    superficie: '#111B2E',
    superficieSecundaria: '#17243A',
    borda: '#2A3A52',
    bordaForte: '#42556E',
    texto: '#F1F5F9',
    textoSecundario: '#CBD5E1',
    textoSuave: '#AAB8CA',
    textoPlaceholder: '#8292A8',
    marca: '#007D94',
    marcaEscura: '#006578',
    marcaSuave: 'rgba(0, 151, 178, 0.16)',
    textoSobreMarca: '#F9FDFF',
    sucesso: '#4ADE80',
    fundoSucesso: '#123323',
    aviso: '#FBBF24',
    fundoAviso: '#38290E',
    informacao: '#60A5FA',
    fundoInformacao: '#142B4A',
    perigo: '#FB7185',
    fundoPerigo: '#3B1824',
    perigoStatus: '#FB7185',
    fundoPerigoStatus: '#3B1824',
};

export function useTema() {
    const esquemaSistema = useColorScheme();
    const modo = esquemaSistema === 'dark' ? 'escuro' : 'claro';

    return {
        modo,
        cores: modo === 'escuro' ? temaEscuro : temaClaro,
    } as const;
}
