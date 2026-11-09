import { useCallback, useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, FlatList, Pressable, RefreshControl, Text, TextInput, View } from 'react-native';
import { useFocusEffect, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import NavegacaoInferior from '../../../../components/NavegacaoInferior';
import { useTema } from '../../../../tema/tema';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import { useAuth } from '../../../auth/AuthProvider';
import { obterCatalogoProcedimentos, obterMeusProcedimentos } from '../../apiProcedimentos';
import type { ProcedimentoCatalogo, ProcedimentoExibido, ProcedimentoProfissional } from '../../typeProcedimentos';
import { criarEstilos } from './styles';

function formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
}

function combinarProcedimentos(catalogo: ProcedimentoCatalogo[], meusProcedimentos: ProcedimentoProfissional[]): ProcedimentoExibido[] {
    const porUuid = new Map(meusProcedimentos.map((procedimento) => [procedimento.procedimentoUuid, procedimento]));

    return catalogo.map((procedimento) => {
        const meuProcedimento = porUuid.get(procedimento.uuid);
        return meuProcedimento
            ? {
                uuid: procedimento.uuid,
                profissionalProcedimentoUuid: meuProcedimento.profissionalProcedimentoUuid,
                nome: procedimento.nome,
                ativo: true,
                valor: meuProcedimento.valorEfetivo,
                duracaoMinutos: meuProcedimento.duracaoEfetivaMinutos,
                valorBase: procedimento.valorBase,
                duracaoEstimadaMinutos: procedimento.duracaoEstimadaMinutos,
            }
            : {
                uuid: procedimento.uuid,
                nome: procedimento.nome,
                ativo: false,
                valor: procedimento.valorBase,
                duracaoMinutos: procedimento.duracaoEstimadaMinutos,
                valorBase: procedimento.valorBase,
                duracaoEstimadaMinutos: procedimento.duracaoEstimadaMinutos,
            };
    });
}

export default function TelaMeusProcedimentos() {
    const { state, signOut } = useAuth();
    const router = useRouter();
    const { cores } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);
    const [procedimentos, setProcedimentos] = useState<ProcedimentoExibido[]>([]);
    const [termoBusca, setTermoBusca] = useState('');
    const [carregando, setCarregando] = useState(true);
    const [atualizando, setAtualizando] = useState(false);
    const [erro, setErro] = useState<string | null>(null);

    const carregarProcedimentos = useCallback(async (atualizacaoManual = false) => {
        if (atualizacaoManual) setAtualizando(true);
        else setCarregando(true);
        setErro(null);

        try {
            const [respostaCatalogo, respostaProfissional] = await Promise.all([
                obterCatalogoProcedimentos(),
                obterMeusProcedimentos(),
            ]);
            setProcedimentos(combinarProcedimentos(respostaCatalogo.procedimentos, respostaProfissional.procedimentos));
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setProcedimentos([]);
            setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar os procedimentos.');
        } finally {
            setCarregando(false);
            setAtualizando(false);
        }
    }, [signOut]);

    useFocusEffect(useCallback(() => {
        if (state.status === 'signedIn') void carregarProcedimentos();
    }, [carregarProcedimentos, state.status]));

    useEffect(() => {
        if (state.status !== 'signedOut') return;
        let ativo = true;
        void sessionService.getClinicUuid().then((clinicUuid) => {
            if (ativo) router.replace(clinicUuid ? '/(public)/login' : '/(public)/clinic');
        });
        return () => { ativo = false; };
    }, [router, state.status]);

    const procedimentosFiltrados = useMemo(() => {
        const busca = termoBusca.trim().toLocaleLowerCase('pt-BR');
        if (!busca) return procedimentos;
        return procedimentos.filter((procedimento) => procedimento.nome.toLocaleLowerCase('pt-BR').includes(busca));
    }, [procedimentos, termoBusca]);

    if (state.status !== 'signedIn') {
        return <SafeAreaView style={styles.tela}><View style={[styles.estado, styles.estadoCarregando]}><ActivityIndicator size="large" color={cores.marca} /></View></SafeAreaView>;
    }

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <FlatList
                data={erro ? [] : procedimentosFiltrados}
                keyExtractor={(procedimento) => procedimento.uuid}
                contentContainerStyle={styles.conteudo}
                ItemSeparatorComponent={() => <View style={{ height: 12 }} />}
                refreshControl={<RefreshControl refreshing={atualizando} onRefresh={() => void carregarProcedimentos(true)} tintColor={cores.marca} />}
                ListHeaderComponent={(
                    <View style={{ gap: 18 }}>
                        <View style={styles.cabecalho}><Text style={styles.titulo}>Meus procedimentos</Text></View>
                        <View style={styles.busca}>
                            <Text accessibilityElementsHidden style={styles.iconeBusca}>⌕</Text>
                            <TextInput
                                accessibilityLabel="Buscar procedimento"
                                placeholder="Buscar procedimento"
                                placeholderTextColor={cores.textoPlaceholder}
                                returnKeyType="search"
                                value={termoBusca}
                                onChangeText={setTermoBusca}
                                style={styles.campoBusca}
                            />
                        </View>
                    </View>
                )}
                ListEmptyComponent={carregando ? (
                    <View style={[styles.estado, styles.estadoCarregando]}>
                        <ActivityIndicator size="large" color={cores.marca} />
                        <Text style={styles.textoEstado}>Carregando procedimentos…</Text>
                    </View>
                ) : erro ? (
                    <View style={styles.estado}>
                        <Text style={[styles.tituloEstado, styles.textoErro]}>Não foi possível carregar</Text>
                        <Text style={styles.textoEstado}>{erro}</Text>
                        <Pressable accessibilityRole="button" onPress={() => void carregarProcedimentos()} style={styles.botaoTentarNovamente}>
                            <Text style={styles.textoBotao}>Tentar novamente</Text>
                        </Pressable>
                    </View>
                ) : procedimentos.length === 0 ? (
                    <View style={styles.estado}>
                        <Text style={styles.tituloEstado}>Nenhum procedimento no catálogo</Text>
                        <Text style={styles.textoEstado}>Quando a clínica disponibilizar procedimentos, eles aparecerão aqui.</Text>
                    </View>
                ) : (
                    <View style={styles.estado}>
                        <Text style={styles.tituloEstado}>Nenhum resultado encontrado</Text>
                        <Text style={styles.textoEstado}>Tente buscar por outro nome de procedimento.</Text>
                    </View>
                )}
                renderItem={({ item }) => (
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel={`${item.ativo ? 'Editar' : 'Vincular'} procedimento ${item.nome}`}
                        onPress={() => {
                            router.push({
                                pathname: '/procedimento/[procedimentoUuid]',
                                params: { procedimentoUuid: item.uuid },
                            });
                        }}
                        style={styles.cartao}
                    >
                        <View style={styles.linhaTopo}>
                            <View style={styles.iconeProcedimento}><Text style={styles.simboloProcedimento}>◇</Text></View>
                            <View style={styles.dadosNome}>
                                <Text style={styles.nome}>{item.nome}</Text>
                                <Text style={styles.origem}>Catálogo da clínica</Text>
                            </View>
                            <View style={[styles.etiqueta, item.ativo ? styles.etiquetaAtiva : styles.etiquetaInativa]}>
                                <Text style={[styles.textoEtiqueta, item.ativo ? styles.textoEtiquetaAtiva : styles.textoEtiquetaInativa]}>{item.ativo ? 'Ativo' : 'Inativo'}</Text>
                            </View>
                            <Text accessibilityElementsHidden style={styles.setaCartao}>›</Text>
                        </View>
                        <View style={styles.separador} />
                        <View style={styles.linhaValores}>
                            <View style={styles.grupoValor}>
                                <Text style={styles.rotuloValor}>{item.ativo ? 'Seu valor' : 'Valor do catálogo'}</Text>
                                <Text style={styles.valor}>{formatarMoeda(item.valor)}</Text>
                            </View>
                            <View style={styles.grupoValor}>
                                <Text style={styles.rotuloValor}>{item.ativo ? 'Sua duração' : 'Duração estimada'}</Text>
                                <Text style={styles.valor}>{item.duracaoMinutos} min</Text>
                            </View>
                        </View>
                        {!item.ativo && <Text style={styles.rodapeCatalogo}>Toque para vincular ou reativar e personalizar valor e duração.</Text>}
                    </Pressable>
                )}
            />
            <NavegacaoInferior abaAtiva="servicos" />
        </SafeAreaView>
    );
}
