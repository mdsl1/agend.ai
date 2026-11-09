import { useCallback, useEffect, useMemo, useState } from 'react';
import {
    ActivityIndicator,
    Alert,
    KeyboardAvoidingView,
    Platform,
    Pressable,
    ScrollView,
    Switch,
    Text,
    TextInput,
    View,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import { useAuth } from '../../../auth/AuthProvider';
import { useTema } from '../../../../tema/tema';
import {
    alterarMeuProcedimento,
    obterCatalogoProcedimentos,
    obterMeusProcedimentos,
    vincularMeuProcedimento,
} from '../../apiProcedimentos';
import type { ProcedimentoExibido } from '../../typeProcedimentos';
import { criarEstilos } from './styles';

function formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
}

function formatarValorParaCampo(valor: number): string {
    return valor.toFixed(2).replace('.', ',');
}

function interpretarValorMonetario(texto: string): number | null {
    const limpo = texto.trim().replace(/\s+/g, '');
    if (!limpo) return null;

    const normalizado = limpo.includes(',')
        ? limpo.replace(/\./g, '').replace(',', '.')
        : limpo;

    const numero = Number(normalizado);
    if (!Number.isFinite(numero)) return Number.NaN;
    return Math.round(numero * 100) / 100;
}

function interpretarDuracaoMinutos(texto: string): number | null {
    const limpo = texto.trim();
    if (!limpo) return null;
    if (!/^\d+$/.test(limpo)) return Number.NaN;
    return Number.parseInt(limpo, 10);
}

export default function TelaFormularioProcedimento() {
    const { procedimentoUuid } = useLocalSearchParams<{ procedimentoUuid: string }>();
    const router = useRouter();
    const { state, signOut } = useAuth();
    const { cores, modo } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);

    const [procedimento, setProcedimento] = useState<ProcedimentoExibido | null>(null);
    const [valorTexto, setValorTexto] = useState('');
    const [duracaoTexto, setDuracaoTexto] = useState('');
    const [disponivelAgendamento, setDisponivelAgendamento] = useState(true);
    const [campoFocado, setCampoFocado] = useState<'preco' | 'duracao' | null>(null);

    const [carregando, setCarregando] = useState(true);
    const [salvando, setSalvando] = useState(false);
    const [erroCarregamento, setErroCarregamento] = useState<string | null>(null);
    const [erroFormulario, setErroFormulario] = useState<string | null>(null);

    const carregarProcedimento = useCallback(async () => {
        if (!procedimentoUuid) {
            setErroCarregamento('O identificador do procedimento não foi informado.');
            setCarregando(false);
            return;
        }

        setCarregando(true);
        setErroCarregamento(null);
        setErroFormulario(null);

        try {
            const [respostaCatalogo, respostaMeus] = await Promise.all([
                obterCatalogoProcedimentos(),
                obterMeusProcedimentos(),
            ]);

            const itemCatalogo = respostaCatalogo.procedimentos.find(
                (item) => item.uuid === procedimentoUuid,
            );

            if (!itemCatalogo) {
                setProcedimento(null);
                setErroCarregamento('Procedimento não encontrado no catálogo ativo da clínica.');
                return;
            }

            const meuVinculo = respostaMeus.procedimentos.find(
                (item) => item.procedimentoUuid === procedimentoUuid,
            );

            const combinado: ProcedimentoExibido = meuVinculo
                ? {
                    uuid: itemCatalogo.uuid,
                    profissionalProcedimentoUuid: meuVinculo.profissionalProcedimentoUuid,
                    nome: itemCatalogo.nome,
                    ativo: true,
                    valor: meuVinculo.valorEfetivo,
                    duracaoMinutos: meuVinculo.duracaoEfetivaMinutos,
                    valorBase: itemCatalogo.valorBase,
                    duracaoEstimadaMinutos: itemCatalogo.duracaoEstimadaMinutos,
                }
                : {
                    uuid: itemCatalogo.uuid,
                    nome: itemCatalogo.nome,
                    ativo: false,
                    valor: itemCatalogo.valorBase,
                    duracaoMinutos: itemCatalogo.duracaoEstimadaMinutos,
                    valorBase: itemCatalogo.valorBase,
                    duracaoEstimadaMinutos: itemCatalogo.duracaoEstimadaMinutos,
                };

            setProcedimento(combinado);
            setValorTexto(formatarValorParaCampo(combinado.valor));
            setDuracaoTexto(String(combinado.duracaoMinutos));
            setDisponivelAgendamento(true);
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setProcedimento(null);
            setErroCarregamento(
                falha instanceof Error ? falha.message : 'Não foi possível carregar os dados do procedimento.',
            );
        } finally {
            setCarregando(false);
        }
    }, [procedimentoUuid, signOut]);

    useEffect(() => {
        if (state.status === 'signedIn') {
            void carregarProcedimento();
        }
    }, [carregarProcedimento, state.status]);

    useEffect(() => {
        if (state.status !== 'signedOut') return;
        let ativo = true;
        void sessionService.getClinicUuid().then((clinicUuid) => {
            if (ativo) router.replace(clinicUuid ? '/(public)/login' : '/(public)/clinic');
        });
        return () => { ativo = false; };
    }, [router, state.status]);

    function voltar() {
        if (router.canGoBack()) router.back();
        else router.replace('/procedimentos');
    }

    async function salvarProcedimento() {
        if (!procedimento || salvando) return;

        const valorInterpretado = interpretarValorMonetario(valorTexto);
        if (Number.isNaN(valorInterpretado) || (valorInterpretado !== null && valorInterpretado < 0)) {
            setErroFormulario(
                procedimento.ativo
                    ? 'Informe um valor válido maior ou igual a zero, ou deixe em branco para manter o valor atual.'
                    : 'Informe um valor válido maior ou igual a zero, ou deixe em branco para usar o valor do catálogo.',
            );
            return;
        }

        const duracaoInterpretada = interpretarDuracaoMinutos(duracaoTexto);
        if (Number.isNaN(duracaoInterpretada) || (duracaoInterpretada !== null && duracaoInterpretada <= 0)) {
            setErroFormulario(
                procedimento.ativo
                    ? 'Informe uma duração em minutos inteiros maior que zero, ou deixe em branco para manter a duração atual.'
                    : 'Informe uma duração em minutos inteiros maior que zero, ou deixe em branco para usar a duração estimada.',
            );
            return;
        }

        if (!procedimento.ativo && !disponivelAgendamento) {
            setErroFormulario(
                'Mantenha a disponibilidade ativa para vincular ou reativar este procedimento.',
            );
            return;
        }

        setSalvando(true);
        setErroFormulario(null);

        try {
            if (procedimento.ativo) {
                if (!procedimento.profissionalProcedimentoUuid) {
                    setErroFormulario('Não foi possível identificar o vínculo ativo deste procedimento.');
                    return;
                }

                const resposta = await alterarMeuProcedimento(
                    procedimento.profissionalProcedimentoUuid,
                    {
                        valor: valorInterpretado,
                        duracaoMinutos: duracaoInterpretada,
                        disponivelAgendamento,
                    },
                );

                Alert.alert(
                    resposta.disponivelAgendamento ? 'Procedimento atualizado' : 'Procedimento inativado',
                    resposta.disponivelAgendamento
                        ? `${resposta.nome} foi atualizado (${formatarMoeda(resposta.valor)} · ${resposta.duracaoMinutos} min).`
                        : `${resposta.nome} deixou de ser oferecido para novos agendamentos.`,
                    [{ text: 'OK', onPress: voltar }],
                );
                return;
            }

            const resposta = await vincularMeuProcedimento({
                procedimentoUuid: procedimento.uuid,
                valor: valorInterpretado,
                duracaoMinutos: duracaoInterpretada,
            });

            Alert.alert(
                resposta.criado ? 'Procedimento vinculado' : 'Vínculo reativado',
                `${resposta.nome} agora está ativo nos seus procedimentos (${formatarMoeda(resposta.valor)} · ${resposta.duracaoMinutos} min).`,
                [{ text: 'OK', onPress: voltar }],
            );
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErroFormulario(
                falha instanceof Error ? falha.message : 'Não foi possível salvar os dados do procedimento.',
            );
        } finally {
            setSalvando(false);
        }
    }

    if (state.status !== 'signedIn') {
        return (
            <SafeAreaView style={styles.centralizado}>
                <ActivityIndicator size="large" color={cores.marca} />
            </SafeAreaView>
        );
    }

    const modoEdicao = Boolean(procedimento?.ativo);
    const desabilitarBotaoSalvar = salvando || (!modoEdicao && !disponivelAgendamento);

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                style={styles.areaTeclado}
            >
                <ScrollView
                    contentContainerStyle={styles.conteudo}
                    keyboardDismissMode="on-drag"
                    keyboardShouldPersistTaps="handled"
                    automaticallyAdjustKeyboardInsets
                >
                    <View style={styles.cabecalho}>
                        <Pressable
                            accessibilityRole="button"
                            accessibilityLabel="Voltar para meus procedimentos"
                            onPress={voltar}
                            style={styles.botaoVoltar}
                        >
                            <Text style={styles.setaVoltar}>‹</Text>
                        </Pressable>
                        <Text style={styles.tituloCabecalho}>
                            {modoEdicao ? 'Editar procedimento' : 'Vincular procedimento'}
                        </Text>
                    </View>

                    {carregando ? (
                        <View style={styles.estado}>
                            <ActivityIndicator size="large" color={cores.marca} />
                            <Text style={styles.textoEstado}>Carregando procedimento…</Text>
                        </View>
                    ) : erroCarregamento ? (
                        <View style={styles.estado}>
                            <Text style={[styles.tituloEstado, styles.textoErro]}>Não foi possível carregar</Text>
                            <Text style={styles.textoEstado}>{erroCarregamento}</Text>
                            <Pressable
                                accessibilityRole="button"
                                onPress={() => void carregarProcedimento()}
                                style={styles.botaoTentarNovamente}
                            >
                                <Text style={styles.textoTentarNovamente}>Tentar novamente</Text>
                            </Pressable>
                        </View>
                    ) : procedimento ? (
                        <>
                            <View style={styles.cartaoProcedimento}>
                                <View style={styles.iconeProcedimento}>
                                    <Text style={styles.simboloProcedimento}>◇</Text>
                                </View>
                                <View style={styles.dadosProcedimento}>
                                    <Text style={styles.nomeProcedimento}>{procedimento.nome}</Text>
                                    <Text style={styles.origemProcedimento}>Definido pela Clínica</Text>
                                    <Text style={styles.referenciaCatalogo}>
                                        Catálogo: {formatarMoeda(procedimento.valorBase)} · {procedimento.duracaoEstimadaMinutos} min
                                    </Text>
                                </View>
                                <View style={[styles.etiquetaStatus, procedimento.ativo ? styles.etiquetaAtiva : styles.etiquetaInativa]}>
                                    <Text style={[styles.textoEtiqueta, procedimento.ativo ? styles.textoEtiquetaAtiva : styles.textoEtiquetaInativa]}>
                                        {procedimento.ativo ? 'Ativo' : 'Inativo'}
                                    </Text>
                                </View>
                            </View>

                            <View style={styles.blocoCampo}>
                                <Text style={styles.rotuloCampo}>Seu preço</Text>
                                <View style={[styles.caixaEntradaPreco, campoFocado === 'preco' && styles.caixaEntradaFoco]}>
                                    <Text style={styles.prefixoMoeda}>R$</Text>
                                    <TextInput
                                        accessibilityLabel="Seu preço"
                                        editable={!salvando}
                                        keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                        keyboardType="decimal-pad"
                                        onBlur={() => setCampoFocado(null)}
                                        onChangeText={(texto) => {
                                            setValorTexto(texto);
                                            if (erroFormulario) setErroFormulario(null);
                                        }}
                                        onFocus={() => setCampoFocado('preco')}
                                        placeholder={formatarValorParaCampo(
                                            modoEdicao ? procedimento.valor : procedimento.valorBase,
                                        )}
                                        placeholderTextColor={cores.textoPlaceholder}
                                        returnKeyType="next"
                                        value={valorTexto}
                                        style={styles.entradaPreco}
                                    />
                                </View>
                                <Text style={styles.ajudaCampo}>
                                    {modoEdicao
                                        ? `Se deixar em branco, será mantido seu valor atual (${formatarMoeda(procedimento.valor)}).`
                                        : `Se deixar em branco, será usado o valor-base do catálogo (${formatarMoeda(procedimento.valorBase)}).`}
                                </Text>
                            </View>

                            <View style={styles.blocoCampo}>
                                <Text style={styles.rotuloCampo}>Duração do atendimento</Text>
                                <View style={[styles.caixaEntradaDuracao, campoFocado === 'duracao' && styles.caixaEntradaFoco]}>
                                    <TextInput
                                        accessibilityLabel="Duração do atendimento em minutos"
                                        editable={!salvando}
                                        keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                        keyboardType="number-pad"
                                        onBlur={() => setCampoFocado(null)}
                                        onChangeText={(texto) => {
                                            setDuracaoTexto(texto);
                                            if (erroFormulario) setErroFormulario(null);
                                        }}
                                        onFocus={() => setCampoFocado('duracao')}
                                        placeholder={String(
                                            modoEdicao
                                                ? procedimento.duracaoMinutos
                                                : procedimento.duracaoEstimadaMinutos,
                                        )}
                                        placeholderTextColor={cores.textoPlaceholder}
                                        returnKeyType="done"
                                        value={duracaoTexto}
                                        style={styles.entradaDuracao}
                                    />
                                    <Text style={styles.sufixoDuracao}>minutos</Text>
                                </View>
                                <Text style={styles.ajudaCampo}>
                                    {modoEdicao
                                        ? `Se deixar em branco, será mantida sua duração atual (${procedimento.duracaoMinutos} min).`
                                        : `Se deixar em branco, será usada a duração estimada do catálogo (${procedimento.duracaoEstimadaMinutos} min).`}
                                </Text>
                            </View>

                            <View style={styles.blocoCampo}>
                                <Text style={styles.rotuloCampo}>Disponibilidade</Text>
                                <View style={styles.cartaoDisponibilidade}>
                                    <View style={styles.textosDisponibilidade}>
                                        <Text style={styles.tituloDisponibilidade}>Oferecer este procedimento</Text>
                                        <Text style={styles.subtituloDisponibilidade}>
                                            {disponivelAgendamento
                                                ? 'Disponível para novos agendamentos'
                                                : 'Indisponível para novos agendamentos'}
                                        </Text>
                                    </View>
                                    <Switch
                                        accessibilityLabel="Oferecer este procedimento"
                                        disabled={salvando}
                                        onValueChange={(novoValor) => {
                                            setDisponivelAgendamento(novoValor);
                                            if (erroFormulario) setErroFormulario(null);
                                        }}
                                        thumbColor="#FFFFFF"
                                        trackColor={{ false: cores.bordaForte, true: cores.marca }}
                                        value={disponivelAgendamento}
                                    />
                                </View>
                            </View>

                            <View style={styles.avisoInformativo}>
                                <Text style={styles.textoAvisoInformativo}>
                                    {modoEdicao && !disponivelAgendamento
                                        ? 'Ao salvar com a disponibilidade desativada, este procedimento deixará de ser oferecido nos seus novos agendamentos, preservando o histórico.'
                                        : 'Seu preço e duração personalizados não alteram o catálogo da clínica.'}
                                </Text>
                            </View>

                            {erroFormulario ? (
                                <View accessibilityRole="alert" style={styles.erroFormulario}>
                                    <Text style={styles.textoErroFormulario}>{erroFormulario}</Text>
                                </View>
                            ) : null}

                            <View style={styles.rodapeAcoes}>
                                <Pressable
                                    accessibilityRole="button"
                                    accessibilityState={{ disabled: desabilitarBotaoSalvar }}
                                    disabled={desabilitarBotaoSalvar}
                                    onPress={() => void salvarProcedimento()}
                                    style={({ pressed }) => [
                                        styles.botaoSalvar,
                                        pressed && !desabilitarBotaoSalvar && styles.botaoSalvarPressionado,
                                        desabilitarBotaoSalvar && styles.botaoSalvarDesabilitado,
                                    ]}
                                >
                                    {salvando ? (
                                        <ActivityIndicator color={cores.textoSobreMarca} />
                                    ) : (
                                        <Text style={styles.textoBotaoSalvar}>
                                            {modoEdicao ? 'Salvar alterações' : 'Salvar vínculo'}
                                        </Text>
                                    )}
                                </Pressable>
                            </View>
                        </>
                    ) : null}
                </ScrollView>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}
