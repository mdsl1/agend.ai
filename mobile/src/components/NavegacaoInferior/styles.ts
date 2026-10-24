import { StyleSheet } from 'react-native';

export const styles = StyleSheet.create({
    barra: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        paddingHorizontal: 8,
        paddingTop: 10,
        paddingBottom: 4,
        backgroundColor: '#FFFFFF',
        borderTopWidth: 1,
        borderTopColor: '#E2E8F0',
    },
    item: {
        minWidth: 60,
        minHeight: 48,
        alignItems: 'center',
        justifyContent: 'center',
        gap: 2,
        borderRadius: 12,
    },
    itemAtivo: { backgroundColor: '#E6F5F8' },
    itemDesativado: { opacity: 0.7 },
    icone: { color: '#515F74', fontSize: 18, lineHeight: 22, fontWeight: '600' },
    iconeAtivo: { color: '#007D94' },
    rotulo: { color: '#515F74', fontSize: 10, fontWeight: '500' },
    rotuloAtivo: { color: '#007D94', fontWeight: '700' },
});
