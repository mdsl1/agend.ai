import { Alert, Pressable, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useMemo } from 'react';
import { useTema } from '../../tema/tema';
import { criarEstilos } from './styles';

type AbaAtiva = 'inicio' | 'agenda' | 'receitas';

const itens = [
    { chave: 'inicio', rotulo: 'Início', icone: '⌂', rota: '/' },
    { chave: 'agenda', rotulo: 'Agenda', icone: '▦', rota: '/agenda' },
    { chave: 'receitas', rotulo: 'Receitas', icone: 'R$', rota: '/receitas' },
    { chave: 'servicos', rotulo: 'Serviços', icone: '✚' },
    { chave: 'perfil', rotulo: 'Perfil', icone: '○' },
] as const;

export default function NavegacaoInferior({ abaAtiva }: { abaAtiva: AbaAtiva }) {
    const router = useRouter();
    const { cores } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);

    function selecionarAba(chave: string, rota?: '/' | '/agenda' | '/receitas') {
        if (rota) {
            router.replace(rota);
            return;
        }

        const item = itens.find((opcao) => opcao.chave === chave);
        if (item) Alert.alert('Em desenvolvimento', `A área ${item.rotulo} será implementada em uma próxima etapa.`);
    }

    return (
        <View accessibilityRole="tablist" style={styles.barra}>
            {itens.map((item) => {
                const ativa = item.chave === abaAtiva;
                const possuiRota = 'rota' in item;

                return (
                    <Pressable
                        key={item.chave}
                        accessibilityRole="tab"
                        accessibilityState={{ selected: ativa }}
                        accessibilityLabel={item.rotulo}
                        onPress={() => selecionarAba(item.chave, possuiRota ? item.rota : undefined)}
                        style={[styles.item, ativa && styles.itemAtivo, !possuiRota && styles.itemDesativado]}
                    >
                        <Text style={[styles.icone, ativa && styles.iconeAtivo]}>{item.icone}</Text>
                        <Text style={[styles.rotulo, ativa && styles.rotuloAtivo]}>{item.rotulo}</Text>
                    </Pressable>
                );
            })}
        </View>
    );
}
