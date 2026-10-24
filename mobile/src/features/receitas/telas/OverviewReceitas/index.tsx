import { useCallback, useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import NavegacaoInferior from '../../../../components/NavegacaoInferior';
import { useAuth } from '../../../auth/AuthProvider';
import { obterResumoReceitas } from '../../apiReceitas';
import type { ResumoReceitas } from '../../typeReceitas';
import { styles } from './styles';

function formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
}

function formatarMes(ano: number, mes: number): string {
    return new Intl.DateTimeFormat('pt-BR', { month: 'long', year: 'numeric', timeZone: 'America/Sao_Paulo' })
        .format(new Date(Date.UTC(ano, mes - 1, 1, 12)));
}

function obterErro(erro: unknown): string {
    if (erro instanceof Error) return erro.message;
    return 'Não foi possível carregar o resumo de receitas.';
}

function obterMesAtual(): { ano: number; mes: number } {
    const partes = new Intl.DateTimeFormat('en', {
        year: 'numeric',
        month: 'numeric',
        timeZone: 'America/Sao_Paulo',
    }).formatToParts(new Date());
    return {
        ano: Number(partes.find((parte) => parte.type === 'year')?.value),
        mes: Number(partes.find((parte) => parte.type === 'month')?.value),
    };
}

export default function TelaOverviewReceitas() {
    const { state, signOut } = useAuth();
    const router = useRouter();
    const mesAtual = obterMesAtual();
    const [ano, setAno] = useState(mesAtual.ano);
    const [mes, setMes] = useState(mesAtual.mes);
    const [dados, setDados] = useState<ResumoReceitas | null>(null);
    const [carregando, setCarregando] = useState(true);
    const [atualizando, setAtualizando] = useState(false);
    const [erro, setErro] = useState<string | null>(null);

    const carregarResumo = useCallback(async (atualizacaoManual = false) => {
        if (atualizacaoManual) setAtualizando(true);
        else setCarregando(true);
        setErro(null);
        try {
            const resposta = await obterResumoReceitas(mes, ano);
            setDados(resposta);
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErro(obterErro(falha));
        } finally {
            setCarregando(false);
            setAtualizando(false);
        }
    }, [ano, mes, signOut]);

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

    function alterarMes(direcao: -1 | 1) {
        const dataSeguinte = new Date(Date.UTC(ano, mes - 1 + direcao, 1));
        setAno(dataSeguinte.getUTCFullYear());
        setMes(dataSeguinte.getUTCMonth() + 1);
    }

    if (state.status !== 'signedIn') {
        return <SafeAreaView style={styles.carregando}><ActivityIndicator size="large" color="#007D94" /></SafeAreaView>;
    }

    const proximoMes = mes === 12 ? { ano: ano + 1, mes: 1 } : { ano, mes: mes + 1 };
    const podeAvancar = proximoMes.ano < mesAtual.ano || (proximoMes.ano === mesAtual.ano && proximoMes.mes <= mesAtual.mes);
    const totalReceitas = dados ? dados.receitaRecebida + dados.receitaEstimada : 0;
    const totalAtendimentos = dados?.porProcedimento.reduce((total, item) => total + item.qtde, 0) ?? 0;
    const ticketMedio = totalAtendimentos > 0 ? totalReceitas / totalAtendimentos : 0;
    const percentual = dados?.percentualVsMesAnterior;

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <ScrollView
                contentContainerStyle={styles.conteudo}
                refreshControl={<RefreshControl refreshing={atualizando} onRefresh={() => void carregarResumo(true)} tintColor="#007D94" />}
            >
                <View style={styles.cabecalho}>
                    <Text style={styles.titulo}>Receitas</Text>
                </View>

                <View style={styles.seletorMes}>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Mês anterior"
                        accessibilityState={{ disabled: ano === 2020 && mes === 1 }}
                        disabled={ano === 2020 && mes === 1}
                        onPress={() => alterarMes(-1)}
                        style={[styles.botaoMes, ano === 2020 && mes === 1 && styles.botaoMesDesativado]}
                    >
                        <Text style={styles.setaMes}>‹</Text>
                    </Pressable>
                    <Text accessibilityLiveRegion="polite" style={styles.mes}>{formatarMes(ano, mes)}</Text>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Próximo mês"
                        accessibilityState={{ disabled: !podeAvancar }}
                        disabled={!podeAvancar}
                        onPress={() => alterarMes(1)}
                        style={[styles.botaoMes, !podeAvancar && styles.botaoMesDesativado]}
                    >
                        <Text style={styles.setaMes}>›</Text>
                    </Pressable>
                </View>

                {carregando ? (
                    <View style={[styles.card, styles.carregando]}><ActivityIndicator color="#007D94" /></View>
                ) : erro ? (
                    <View style={styles.cartaoProcedimento}>
                        <Text style={styles.erro}>{erro}</Text>
                        <Pressable accessibilityRole="button" onPress={() => void carregarResumo()} style={[styles.botaoRepetir, { marginTop: 14 }]}>
                            <Text style={styles.botaoRepetirTexto}>Tentar novamente</Text>
                        </Pressable>
                    </View>
                ) : dados ? (
                    <>
                        <View style={styles.destaque}>
                            <Text style={styles.destaqueRotulo}>Receita total no mês</Text>
                            <Text style={styles.destaqueValor}>{formatarMoeda(totalReceitas)}</Text>
                            <Text style={styles.variacao}>
                                {percentual == null
                                    ? 'Sem comparação com o mês anterior'
                                    : `${percentual >= 0 ? '+' : ''}${new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 1 }).format(percentual)} vs. ${formatarMes(ano, mes-1).split(" ")[0]}`}
                            </Text>
                        </View>

                        <View style={styles.linhaCards}>
                            <View style={styles.card}>
                                <View style={styles.cardCabecalho}>
                                    <View style={[styles.marcador, styles.marcadorRecebido]} />
                                    <Text style={styles.rotuloCard}>Recebida</Text>
                                </View>
                                <Text style={styles.valorCard}>{formatarMoeda(dados.receitaRecebida)}</Text>
                            </View>
                            <View style={styles.card}>
                                <View style={styles.cardCabecalho}>
                                    <View style={[styles.marcador, styles.marcadorEstimado]} />
                                    <Text style={styles.rotuloCard}>Estimada</Text>
                                </View>
                                <Text style={styles.valorCard}>{formatarMoeda(dados.receitaEstimada)}</Text>
                            </View>
                        </View>

                        <View style={styles.resumoTicket}>
                            <Text style={styles.rotuloTicket}>Ticket médio por atendimento</Text>
                            <Text style={styles.valorTicket}>{formatarMoeda(ticketMedio)}</Text>
                        </View>

                        <View style={{ gap: 12 }}>
                            <Text style={styles.tituloSecao}>Por procedimento</Text>
                            {dados.porProcedimento.length === 0 ? (
                                <Text style={styles.vazio}>Nenhuma receita registrada neste mês.</Text>
                            ) : (
                                <View style={styles.lista}>
                                    {dados.porProcedimento.map((procedimento) => (
                                        <View key={procedimento.procedimentoUuid} style={styles.cartaoProcedimento}>
                                            <View style={styles.procedimentoTopo}>
                                                <Text style={styles.procedimentoNome}>{procedimento.nome}</Text>
                                                <Text style={styles.procedimentoValor}>{formatarMoeda(procedimento.valorTotal)}</Text>
                                            </View>
                                            <Text style={styles.procedimentoQuantidade}>
                                                {procedimento.qtde} {procedimento.qtde === 1 ? 'atendimento' : 'atendimentos'}
                                            </Text>
                                        </View>
                                    ))}
                                </View>
                            )}
                        </View>
                    </>
                ) : null}
            </ScrollView>
            <NavegacaoInferior abaAtiva="receitas/resumo" />
        </SafeAreaView>
    );
}
