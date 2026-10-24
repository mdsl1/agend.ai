import { Alert, Pressable, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { styles } from './styles';

type AbaAtiva = 'inicio' | 'receitas/resumo';

const itens = [
    { chave: 'inicio', rotulo: 'Início', icone: '⌂', rota: '/' },
    { chave: 'agenda', rotulo: 'Agenda', icone: '▦' },
    { chave: 'receitas/resumo', rotulo: 'Receitas', icone: 'R$' , rota: '/receitas/resumo' },
    { chave: 'servicos', rotulo: 'Serviços', icone: '✚' },
    { chave: 'perfil', rotulo: 'Perfil', icone: '○' },
] as const;

export default function NavegacaoInferior({ abaAtiva }: { abaAtiva: AbaAtiva }) {
    const router = useRouter();

    function selecionarAba(chave: string, rota?: '/' | '/receitas/resumo') {
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
