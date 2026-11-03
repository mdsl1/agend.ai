import { useCallback, useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Alert, Pressable, ScrollView, Text, View, type StyleProp, type TextStyle, type ViewStyle } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import { useAuth } from '../../../auth/AuthProvider';
import { useTema } from '../../../../tema/tema';
import { alterarStatusAgendamento, obterDetalhesAgendamento } from '../../apiAgendamento';
import type { DetalhesAgendamento } from '../../typeAgendamento';
import type { StatusAgendamento } from '../../../agenda/typeAgenda';
import { criarEstilos } from './styles';

const FUSO_HORARIO = 'America/Sao_Paulo';

const rotulosStatus: Record<string, string> = {
    pendente_integracao: 'Pendente de integração',
    agendado: 'Agendado',
    reagendado: 'Reagendado',
    falha_integracao: 'Falha na integração',
    concluido: 'Concluído',
    cancelado: 'Cancelado',
    faltou: 'Faltou',
};

function formatarData(inicio: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        timeZone: FUSO_HORARIO,
    }).format(new Date(inicio)).replace('.', '');
}

function formatarHora(instante: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        timeZone: FUSO_HORARIO,
    }).format(new Date(instante));
}

function obterIniciais(nome: string): string {
    return nome
        .trim()
        .split(/\s+/)
        .slice(0, 2)
        .map((parte) => parte[0])
        .join('')
        .toLocaleUpperCase('pt-BR');
}

type VarianteStatus = 'pendente' | 'agendado' | 'concluido' | 'erro';

function obterRotuloStatus(status: StatusAgendamento): string {
    return rotulosStatus[status];
}

function obterVarianteStatus(status: StatusAgendamento): VarianteStatus {
    if (status === 'pendente_integracao') return 'pendente';
    if (status === 'agendado' || status === 'reagendado') return 'agendado';
    if (status === 'concluido') return 'concluido';
    return 'erro';
}

function obterErro(erro: unknown): string {
    if (erro instanceof Error) return erro.message;
    return 'Não foi possível carregar os detalhes do agendamento.';
}

export default function TelaDetalhesAgendamento() {
    const { agendamentoUuid } = useLocalSearchParams<{ agendamentoUuid: string }>();
    const router = useRouter();
    const { state, signOut } = useAuth();
    const { cores, modo } = useTema();
    const styles = useMemo(() => criarEstilos(cores, modo === 'escuro'), [cores, modo]);
    const [dados, setDados] = useState<DetalhesAgendamento | null>(null);
    const [carregando, setCarregando] = useState(true);
    const [alterandoStatus, setAlterandoStatus] = useState(false);
    const [erro, setErro] = useState<string | null>(null);
    const varianteStatus = dados ? obterVarianteStatus(dados.status) : null;
    const estilosEtiquetaStatus: Record<VarianteStatus, StyleProp<ViewStyle>> = {
        pendente: styles.etiquetaPendente,
        agendado: styles.etiquetaAgendado,
        concluido: styles.etiquetaConcluido,
        erro: styles.etiquetaErro,
    };
    const estilosTextoStatus: Record<VarianteStatus, StyleProp<TextStyle>> = {
        pendente: styles.textoStatusPendente,
        agendado: styles.textoStatusAgendado,
        concluido: styles.textoStatusConcluido,
        erro: styles.textoStatusErro,
    };
    const podeAlterarStatus = dados?.status === 'agendado' && !alterandoStatus;

    const carregarDetalhes = useCallback(async () => {
        if (!agendamentoUuid) {
            setErro('O identificador do agendamento não foi informado.');
            setCarregando(false);
            return;
        }

        setCarregando(true);
        setErro(null);
        try {
            const resposta = await obterDetalhesAgendamento(agendamentoUuid);
            setDados(resposta);
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErro(obterErro(falha));
        } finally {
            setCarregando(false);
        }
    }, [agendamentoUuid, signOut]);

    useEffect(() => {
        if (state.status === 'signedIn') void carregarDetalhes();
    }, [carregarDetalhes, state.status]);

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
        else router.replace('/agenda');
    }

    function informarAcaoFutura(acao: string) {
        Alert.alert('Em desenvolvimento', `A ação “${acao}” será habilitada quando a respectiva rota estiver disponível.`);
    }

    async function enviarAlteracaoStatus(status: 'concluido' | 'cancelado' | 'faltou') {
        if (!dados || !podeAlterarStatus) return;

        setAlterandoStatus(true);
        try {
            const resposta = await alterarStatusAgendamento(dados.uuid, status);
            setDados((dadosAtuais) => dadosAtuais
                ? { ...dadosAtuais, status: resposta.novoStatus }
                : dadosAtuais);
            Alert.alert('Status atualizado', `O agendamento foi marcado como ${obterRotuloStatus(resposta.novoStatus).toLocaleLowerCase('pt-BR')}.`);
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            Alert.alert(
                falha instanceof ApiError && falha.status === 409 ? 'Alteração não permitida' : 'Não foi possível atualizar',
                obterErro(falha),
            );
        } finally {
            setAlterandoStatus(false);
        }
    }

    function confirmarAlteracaoStatus(status: 'concluido' | 'cancelado' | 'faltou') {
        Alert.alert(
            'Confirmar alteração',
            `Deseja marcar este agendamento como ${obterRotuloStatus(status).toLocaleLowerCase('pt-BR')}?`,
            [
                { text: 'Voltar', style: 'cancel' },
                { text: 'Confirmar', onPress: () => void enviarAlteracaoStatus(status) },
            ],
        );
    }

    function obterStatusCancelamentoOuFalta(inicio: string): 'cancelado' | 'faltou' | null {
        const hojeUtc = new Date();
        hojeUtc.setUTCHours(0, 0, 0, 0);
        const dataAgendamentoUtc = new Date(inicio);
        dataAgendamentoUtc.setUTCHours(0, 0, 0, 0);
        const limiteCancelamentoUtc = new Date(dataAgendamentoUtc);
        limiteCancelamentoUtc.setUTCDate(limiteCancelamentoUtc.getUTCDate() - 1);

        if (hojeUtc < limiteCancelamentoUtc) return 'cancelado';
        if (hojeUtc >= dataAgendamentoUtc) return 'faltou';
        return null;
    }

    function confirmarCancelamentoOuFalta() {
        if (!dados || !podeAlterarStatus) return;
        const status = obterStatusCancelamentoOuFalta(dados.inicio);
        if (!status) {
            Alert.alert(
                'Alteração não permitida',
                'Pelas regras atuais da API, não é possível cancelar nem registrar falta na véspera do atendimento.',
            );
            return;
        }
        confirmarAlteracaoStatus(status);
    }

    if (state.status !== 'signedIn') {
        return <SafeAreaView style={styles.centralizado}><ActivityIndicator size="large" color={cores.marca} /></SafeAreaView>;
    }

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <ScrollView contentContainerStyle={styles.conteudo}>
                <View style={styles.cabecalho}>
                    <Pressable accessibilityRole="button" accessibilityLabel="Voltar para a agenda" onPress={voltar} style={styles.botaoVoltar}>
                        <Text style={styles.setaVoltar}>‹</Text>
                    </Pressable>
                    <Text style={styles.tituloCabecalho}>Detalhes</Text>
                </View>

                {carregando ? (
                    <View style={styles.estado}><ActivityIndicator size="large" color={cores.marca} /></View>
                ) : erro ? (
                    <View style={styles.estado}>
                        <Text style={styles.textoErro}>{erro}</Text>
                        <Pressable accessibilityRole="button" onPress={() => void carregarDetalhes()} style={styles.botaoTentarNovamente}>
                            <Text style={styles.textoTentarNovamente}>Tentar novamente</Text>
                        </Pressable>
                    </View>
                ) : dados ? (
                    <>
                        <View style={styles.cartaoCliente}>
                            <View style={styles.avatar}><Text style={styles.iniciais}>{obterIniciais(dados.cliente.nome)}</Text></View>
                            <View style={styles.dadosCliente}>
                                <Text style={styles.nomeCliente} numberOfLines={2}>{dados.cliente.nome}</Text>
                                <Text style={styles.nomeProcedimentoTopo} numberOfLines={2}>{dados.procedimento.nome}</Text>
                                <View style={[styles.etiquetaStatus, varianteStatus && estilosEtiquetaStatus[varianteStatus]]}>
                                    <Text style={[styles.textoStatus, varianteStatus && estilosTextoStatus[varianteStatus]]}>
                                        {obterRotuloStatus(dados.status)}
                                    </Text>
                                </View>
                            </View>
                            <View style={styles.divisorCliente} />
                            <Text style={styles.rotuloAgendamento}>Agendamento #{dados.uuid}</Text>
                        </View>

                        <Text style={styles.tituloSecao}>Informações</Text>
                        <View style={styles.cartaoInformacoes}>
                            <View style={styles.linhaInformacao}>
                                <View style={styles.iconeInformacao}><Text style={styles.textoIcone}>□</Text></View>
                                <View style={styles.conteudoInformacao}>
                                    <Text style={styles.rotuloInformacao}>Data e horário</Text>
                                    <Text style={styles.valorInformacao}>{formatarData(dados.inicio)} · {formatarHora(dados.inicio)} — {formatarHora(dados.fim)}</Text>
                                </View>
                            </View>
                            <View style={styles.separadorInformacao} />
                            <View style={styles.linhaInformacao}>
                                <View style={styles.iconeInformacao}><Text style={styles.textoIcone}>＋</Text></View>
                                <View style={styles.conteudoInformacao}>
                                    <Text style={styles.rotuloInformacao}>Procedimento</Text>
                                    <Text style={styles.valorInformacao}>{dados.procedimento.nome} · {dados.procedimento.duracaoMinutos} min</Text>
                                </View>
                            </View>
                            <View style={styles.separadorInformacao} />
                            <View style={styles.linhaInformacao}>
                                <View style={styles.iconeInformacao}><Text style={styles.textoMoeda}>R$</Text></View>
                                <View style={styles.conteudoInformacao}>
                                    <Text style={styles.rotuloInformacao}>Valor</Text>
                                    <Text style={styles.valorInformacao}>{new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(dados.valorTotal)}</Text>
                                </View>
                            </View>
                        </View>

                        <View style={styles.acoes}>
                            <Pressable accessibilityRole="button" accessibilityState={{ disabled: !podeAlterarStatus }} disabled={!podeAlterarStatus} onPress={() => confirmarAlteracaoStatus('concluido')} style={[styles.botaoConcluir, !podeAlterarStatus && styles.botaoDesabilitado]}>
                                <Text style={styles.textoBotaoConcluir}>{alterandoStatus ? 'Atualizando…' : 'Marcar como Concluído'}</Text>
                            </Pressable>
                            <Pressable accessibilityRole="button" accessibilityState={{ disabled: !podeAlterarStatus }} disabled={!podeAlterarStatus} onPress={confirmarCancelamentoOuFalta} style={[styles.botaoCancelar, !podeAlterarStatus && styles.botaoDesabilitado]}>
                                <Text style={styles.textoBotaoCancelar}>Marcar como Cancelado/ Falta</Text>
                            </Pressable>
                            <Pressable accessibilityRole="button" accessibilityState={{ disabled: !podeAlterarStatus }} disabled={!podeAlterarStatus} onPress={() => informarAcaoFutura('solicitar reagendamento')} style={[styles.botaoReagendar, !podeAlterarStatus && styles.botaoDesabilitado]}>
                                <Text style={styles.textoBotaoReagendar}>Solicitar reagendamento (Em breve)</Text>
                            </Pressable>
                            <Text style={styles.ajudaAcoes}>A alteração só ocorre após aceite do paciente.</Text>
                        </View>
                    </>
                ) : null}
            </ScrollView>
        </SafeAreaView>
    );
}
