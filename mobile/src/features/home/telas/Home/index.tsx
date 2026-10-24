import { useCallback, useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Alert, Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import NavegacaoInferior from '../../../../components/NavegacaoInferior';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import { useAuth } from '../../../auth/AuthProvider';
import { obterResumoInicio } from '../../apiHome';
import type { ResumoInicio } from '../../homeTypes';
import { useTema } from '../../../../tema/tema';
import { criarEstilos } from './styles';

function formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
}

function formatarHora(valor: string): string {
    return new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit', timeZone: 'America/Sao_Paulo' }).format(new Date(valor));
}

function formatarData(valor: string): string {
    const data = new Date(`${valor}T12:00:00`);
    return new Intl.DateTimeFormat('pt-BR', { weekday: 'long', day: 'numeric', month: 'long', timeZone: 'America/Sao_Paulo' }).format(data);
}

function nomeExibicao(nome: string, prefixo: string | null): string {
    const primeiroNome = nome.trim().split(/\s+/)[0] ?? nome;
    if (!prefixo) return primeiroNome;
    if (prefixo === 'Dr.' || prefixo === 'Dra.') return `${prefixo} ${primeiroNome}`;
    return `${prefixo} ${primeiroNome}`;
}

function obterMensagemErro(erro: unknown): string {
    if (erro instanceof ApiError && erro.status === 401) return 'Sua sessão expirou. Entre novamente para continuar.';
    if (erro instanceof Error) return erro.message;
    return 'Não foi possível carregar o resumo agora.';
}

export default function HomeScreen() {
    const { state, signOut, changeClinic } = useAuth();
    const router = useRouter();
    const { cores } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);
    const [dados, setDados] = useState<ResumoInicio | null>(null);
    const [carregando, setCarregando] = useState(true);
    const [atualizando, setAtualizando] = useState(false);
    const [erro, setErro] = useState<string | null>(null);

    const carregarResumo = useCallback(async (atualizacaoManual = false) => {
        if (atualizacaoManual) setAtualizando(true);
        else setCarregando(true);
        setErro(null);
        try {
            const resposta = await obterResumoInicio();
            setDados(resposta);
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErro(obterMensagemErro(falha));
        } finally {
            setCarregando(false);
            setAtualizando(false);
        }
    }, [signOut]);

    useEffect(() => {
        if (state.status === 'signedIn') void carregarResumo();
    }, [carregarResumo, state.status]);

    useEffect(() => {
        if (state.status !== 'signedOut') return;
        let ativo = true;
        void sessionService.getClinicUuid().then((clinicUuid) => {
            if (ativo) router.replace(clinicUuid ? '/(public)/login' : '/(public)/clinic');
        });
        return () => { ativo = false; };
    }, [router, state.status]);

    if (state.status !== 'signedIn') {
        return <SafeAreaView style={styles.carregando}><ActivityIndicator size="large" color={cores.marca} /></SafeAreaView>;
    }

    const { profile } = state;
    const iniciais = profile.nome.split(/\s+/).slice(0, 2).map((parte) => parte[0]).join('').toUpperCase();
    const proximo = dados?.resumoAgenda.proximoAgendamento;
    const percentual = dados?.resumoReceitas.percentualVsMesAnterior;
    const abrirAcoesConta = () => Alert.alert('Sua conta', profile.clinica.nome, [
        { text: 'Sair da conta', style: 'destructive', onPress: () => void signOut() },
        { text: 'Trocar clínica', onPress: () => void changeClinic() },
        { text: 'Cancelar', style: 'cancel' },
    ]);

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <ScrollView
                contentContainerStyle={styles.conteudo}
                refreshControl={<RefreshControl refreshing={atualizando} onRefresh={() => void carregarResumo(true)} tintColor={cores.marca} />}
            >
                <View style={styles.topo}>
                    <View>
                        <Text style={styles.data}>{dados ? formatarData(dados.dataReferencia).toLocaleUpperCase('pt-BR') : 'SUA ROTINA PROFISSIONAL'}</Text>
                        <Text style={styles.saudacao}>Olá, {nomeExibicao(profile.nome, profile.prefixo)}</Text>
                    </View>
                    <Pressable accessibilityRole="button" accessibilityLabel="Opções da conta" onPress={abrirAcoesConta} style={styles.avatar}>
                        <Text style={styles.avatarTexto}>{iniciais}</Text>
                    </Pressable>
                </View>

                {carregando ? (
                    <View style={[styles.cartao, styles.carregando]}><ActivityIndicator color={cores.marca} /></View>
                ) : erro ? (
                    <View style={styles.cartao}>
                        <Text style={styles.erro}>{erro}</Text>
                        <Pressable accessibilityRole="button" onPress={() => void carregarResumo()} style={[styles.acao, { marginTop: 14 }]}><Text style={styles.acaoTexto}>Tentar novamente</Text></Pressable>
                    </View>
                ) : dados ? (
                    <>
                        <View style={styles.receita}>
                            <Text style={styles.receitaRotulo}>Receita total neste mês</Text>
                            <Text style={styles.receitaValor}>{formatarMoeda(dados.resumoReceitas.valorTotal)}</Text>
                            <Text style={styles.variacao}>
                                {percentual == null ? 'Sem comparação com o mês anterior' : `${percentual >= 0 ? '+' : ''}${new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 1 }).format(percentual)} vs. mês anterior`}
                            </Text>
                        </View>

                        <View>
                            <Text style={styles.secoesTitulo}>Hoje</Text>
                            <View style={[styles.contadoresGrade, { marginTop: 12 }]}>
                                <View style={styles.contadores}>
                                    <View style={styles.contador}>
                                        <View style={styles.contadorCabecalho}>
                                            <View style={[styles.indicador, styles.indicadorAgendamentos]} />
                                            <Text style={styles.contadorRotulo}>Agendamentos</Text>
                                        </View>
                                        <Text style={styles.contadorValor}>{dados.resumoAgenda.qtdeHoje}</Text>
                                    </View>
                                    <View style={styles.contador}>
                                        <View style={styles.contadorCabecalho}>
                                            <View style={[styles.indicador, styles.indicadorConfirmados]} />
                                            <Text style={styles.contadorRotulo}>Confirmados</Text>
                                        </View>
                                        <Text style={styles.contadorValor}>{dados.resumoAgenda.qtdeConfirmados}</Text>
                                    </View>
                                </View>
                                <View style={styles.contadores}>
                                    <View style={styles.contador}>
                                        <View style={styles.contadorCabecalho}>
                                            <View style={[styles.indicador, styles.indicadorRestantes]} />
                                            <Text style={styles.contadorRotulo}>Restantes</Text>
                                        </View>
                                        <Text style={styles.contadorValor}>{dados.resumoAgenda.qtdeRestante}</Text>
                                    </View>
                                    <View style={styles.contador}>
                                        <View style={styles.contadorCabecalho}>
                                            <View style={[styles.indicador, styles.indicadorCancelados]} />
                                            <Text style={styles.contadorRotulo}>Cancelados</Text>
                                        </View>
                                        <Text style={styles.contadorValor}>{dados.resumoAgenda.qtdeCancelados}</Text>
                                    </View>
                                </View>
                            </View>
                        </View>

                        <View>
                            <Text style={styles.secoesTitulo}>Próximo atendimento</Text>
                            <View style={[styles.cartao, { marginTop: 12 }]}>
                                {proximo ? (
                                    <>
                                        <View style={styles.cartaoTopo}><Text style={styles.etiqueta}>ATENDIMENTO</Text><Text style={styles.horario}>{formatarHora(proximo.inicio)} — {formatarHora(proximo.fim)}</Text></View>
                                        <Text style={styles.cliente}>{proximo.nomeCliente}</Text>
                                        <Text style={styles.procedimento}>{proximo.nomeProcedimento}</Text>
                                        <Text style={styles.estado}>{proximo.status.replaceAll('_', ' ')}</Text>
                                    </>
                                ) : <Text style={styles.vazio}>Você não tem outros atendimentos previstos para hoje.</Text>}
                            </View>
                        </View>
                    </>
                ) : null}

            </ScrollView>
            <NavegacaoInferior abaAtiva="inicio" />
        </SafeAreaView>
    );
}
