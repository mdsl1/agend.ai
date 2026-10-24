import { StyleSheet } from 'react-native';
import type { CoresTema } from '../../tema/tema';

export function criarEstilos(cores: CoresTema) {
    return StyleSheet.create({
        barra: {
            flexDirection: 'row',
            justifyContent: 'space-around',
            paddingHorizontal: 8,
            paddingTop: 10,
            paddingBottom: 8,
            marginBottom: 4,
            backgroundColor: cores.superficie,
            borderTopWidth: 1,
            borderTopColor: cores.borda,
        },
        item: {
            minWidth: 60,
            minHeight: 48,
            alignItems: 'center',
            justifyContent: 'center',
            gap: 2,
            borderRadius: 12,
        },
        itemAtivo: { backgroundColor: cores.marcaSuave },
        itemDesativado: { opacity: 0.7 },
        icone: { color: cores.textoSecundario, fontSize: 18, lineHeight: 22, fontWeight: '600' },
        iconeAtivo: { color: cores.marca },
        rotulo: { color: cores.textoSecundario, fontSize: 10, fontWeight: '500' },
        rotuloAtivo: { color: cores.marca, fontWeight: '700' },
    });
}
