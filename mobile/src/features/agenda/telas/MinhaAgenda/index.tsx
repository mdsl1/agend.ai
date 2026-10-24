import { useCallback, useEffect, useMemo, useState } from 'react';
import {
    ActivityIndicator,
    Pressable,
    RefreshControl,
    ScrollView,
    Text,
    View,
    type StyleProp,
    type TextStyle,
    type ViewStyle,
} from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import NavegacaoInferior from '../../../../components/NavegacaoInferior';
import { useAuth } from '../../../auth/AuthProvider';
import { obterEventosAgenda } from '../../apiAgenda';
import type { EventoAgenda, StatusAgendamento } from '../../typeAgenda';
import { useTema } from '../../../../tema/tema';
import { criarEstilos } from './styles';

const FUSO_HORARIO = 'America/Sao_Paulo';

function obterPartesData(data: Date): { ano: number; mes: number; dia: number } {
    return {
        ano: data.getUTCFullYear(),
        mes: data.getUTCMonth() + 1,
        dia: data.getUTCDate(),
    };
}

function criarDataCivil(ano: number, mes: number, dia: number): Date {
    return new Date(Date.UTC(ano, mes - 1, dia));
}

function obterHojeEmSaoPaulo(): Date {
    const partes = new Intl.DateTimeFormat('en', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        timeZone: FUSO_HORARIO,
    }).formatToParts(new Date());

    return criarDataCivil(
        Number(partes.find((parte) => parte.type === 'year')?.value),
        Number(partes.find((parte) => parte.type === 'month')?.value),
        Number(partes.find((parte) => parte.type === 'day')?.value),
    );
}

function adicionarDias(data: Date, quantidade: number): Date {
    const partes = obterPartesData(data);
    return criarDataCivil(partes.ano, partes.mes, partes.dia + quantidade);
}

function obterInicioSemana(data: Date): Date {
    const diaDaSemana = data.getUTCDay();
    return adicionarDias(data, -((diaDaSemana + 6) % 7));
}

function obterDeslocamentoFuso(data: Date): number {
    const partesData = obterPartesData(data);
    const instanteReferencia = Date.UTC(partesData.ano, partesData.mes - 1, partesData.dia, 12);
    const partesLocais = new Intl.DateTimeFormat('en', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        hourCycle: 'h23',
        timeZone: FUSO_HORARIO,
    }).formatToParts(new Date(instanteReferencia));
    const valor = (tipo: string) => Number(partesLocais.find((parte) => parte.type === tipo)?.value);
    const instanteRepresentado = Date.UTC(
        valor('year'),
        valor('month') - 1,
        valor('day'),
        valor('hour'),
        valor('minute'),
        valor('second'),
    );

    return instanteRepresentado - instanteReferencia;
}

function formatarInicioDiaComFuso(data: Date): string {
    const partes = obterPartesData(data);
    const deslocamentoMinutos = obterDeslocamentoFuso(data) / 60000;
    const sinal = deslocamentoMinutos >= 0 ? '+' : '-';
    const valorAbsoluto = Math.abs(deslocamentoMinutos);
    const horas = String(Math.floor(valorAbsoluto / 60)).padStart(2, '0');
    const minutos = String(valorAbsoluto % 60).padStart(2, '0');
    const ano = String(partes.ano).padStart(4, '0');
    const mes = String(partes.mes).padStart(2, '0');
    const dia = String(partes.dia).padStart(2, '0');

    return `${ano}-${mes}-${dia}T00:00:00${sinal}${horas}:${minutos}`;
}

function formatarIntervaloSemana(inicio: Date, fim: Date): string {
    const formatador = new Intl.DateTimeFormat('pt-BR', {
        day: 'numeric',
        month: 'short',
        timeZone: 'UTC',
    });
    return `${formatador.format(inicio)} — ${formatador.format(adicionarDias(fim, -1))}`;
}

function formatarDiaSemana(data: Date): string {
    return new Intl.DateTimeFormat('pt-BR', {
        weekday: 'short',
        timeZone: 'UTC',
    }).format(data).replace('.', '').toUpperCase();
}

function formatarHora(valor: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        timeZone: FUSO_HORARIO,
    }).format(new Date(valor));
}

function formatarDuracao(inicio: string, fim: string): string {
    const minutos = Math.max(0, Math.round((new Date(fim).getTime() - new Date(inicio).getTime()) / 60000));
    const horas = Math.floor(minutos / 60);
    const minutosRestantes = minutos % 60;
    if (horas === 0) return `${minutos} min`;
    if (minutosRestantes === 0) return `${horas} h`;
    return `${horas} h ${minutosRestantes} min`;
}

function obterMensagemErro(erro: unknown): string {
    if (erro instanceof Error) return erro.message;
    return 'Não foi possível carregar sua agenda.';
}

type VarianteStatus = 'pendente' | 'agendado' | 'concluido' | 'erro' | 'semStatus' | 'indisponibilidade';

const rotulosStatus: Record<StatusAgendamento, string> = {
    pendente_integracao: 'Pendente de integração',
    agendado: 'Agendado',
    reagendado: 'Reagendado',
    falha_integracao: 'Falha na integração',
    concluido: 'Concluído',
    cancelado: 'Cancelado',
    faltou: 'Faltou',
};

function obterVarianteStatus(
    status: StatusAgendamento | null | undefined,
    indisponibilidade: boolean,
): VarianteStatus {
    if (indisponibilidade) return 'indisponibilidade';
    if (status === 'pendente_integracao') return 'pendente';
    if (status === 'agendado' || status === 'reagendado') return 'agendado';
    if (status === 'concluido') return 'concluido';
    if (status === 'cancelado' || status === 'faltou' || status === 'falha_integracao') return 'erro';
    return 'semStatus';
}

export default function TelaMinhaAgenda() {
    const { state, signOut } = useAuth();
    const router = useRouter();
    const { cores } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);
    const hoje = useMemo(obterHojeEmSaoPaulo, []);
    const [dataSelecionada, setDataSelecionada] = useState(hoje);
    const inicioSemana = useMemo(() => obterInicioSemana(dataSelecionada), [dataSelecionada]);
    const [eventos, setEventos] = useState<EventoAgenda[]>([]);
    const [carregando, setCarregando] = useState(true);
    const [atualizando, setAtualizando] = useState(false);
    const [erro, setErro] = useState<string | null>(null);
    const profissionalUuid = state.status === 'signedIn' ? state.profile.profissionalUuid : null;

    const carregarAgenda = useCallback(async (atualizacaoManual = false) => {
        if (!profissionalUuid) {
            setCarregando(false);
            setEventos([]);
            return;
        }

        if (atualizacaoManual) setAtualizando(true);
        else setCarregando(true);
        setErro(null);

        const fimSemana = adicionarDias(inicioSemana, 7);
        try {
            const resposta = await obterEventosAgenda(
                profissionalUuid,
                formatarInicioDiaComFuso(inicioSemana),
                formatarInicioDiaComFuso(fimSemana),
            );
            setEventos(resposta.eventos);
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
    }, [inicioSemana, profissionalUuid, signOut]);

    useEffect(() => {
        if (state.status === 'signedIn') void carregarAgenda();
    }, [carregarAgenda, state.status]);

    useEffect(() => {
        if (state.status !== 'signedOut') return;
        let ativo = true;
        void sessionService.getClinicUuid().then((clinicUuid) => {
            if (ativo) router.replace(clinicUuid ? '/(public)/login' : '/(public)/clinic');
        });
        return () => { ativo = false; };
    }, [router, state.status]);

    function mudarSemana(direcao: -1 | 1) {
        setDataSelecionada((dataAtual) => adicionarDias(dataAtual, direcao * 7));
    }

    function irParaHoje() {
        setDataSelecionada(hoje);
    }

    const eventosDoDia = useMemo(() => {
        const inicioDia = formatarInicioDiaComFuso(dataSelecionada);
        const fimDia = formatarInicioDiaComFuso(adicionarDias(dataSelecionada, 1));
        const inicioMs = new Date(inicioDia).getTime();
        const fimMs = new Date(fimDia).getTime();

        return eventos
            .filter((evento) => new Date(evento.inicio).getTime() < fimMs && new Date(evento.fim).getTime() > inicioMs)
            .sort((primeiro, segundo) => new Date(primeiro.inicio).getTime() - new Date(segundo.inicio).getTime());
    }, [dataSelecionada, eventos]);

    const estilosEtiquetaPorVariante: Record<VarianteStatus, StyleProp<ViewStyle>> = {
        pendente: styles.etiquetaPendente,
        agendado: styles.etiquetaAgendado,
        concluido: undefined,
        erro: styles.etiquetaStatusErro,
        semStatus: styles.etiquetaSemStatus,
        indisponibilidade: styles.etiquetaIndisponibilidade,
    };
    const estilosTextoPorVariante: Record<VarianteStatus, StyleProp<TextStyle>> = {
        pendente: styles.textoEtiquetaPendente,
        agendado: styles.textoEtiquetaAgendado,
        concluido: undefined,
        erro: styles.textoEtiquetaStatusErro,
        semStatus: styles.textoEtiquetaSemStatus,
        indisponibilidade: styles.textoEtiquetaIndisponibilidade,
    };

    if (state.status !== 'signedIn') {
        return <SafeAreaView style={styles.centralizado}><ActivityIndicator size="large" color={cores.marca} /></SafeAreaView>;
    }

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <ScrollView
                contentContainerStyle={styles.conteudo}
                refreshControl={
                    <RefreshControl
                        refreshing={atualizando}
                        onRefresh={() => void carregarAgenda(true)}
                        tintColor={cores.marca}
                    />
                }
            >
                <View style={styles.cabecalho}>
                    <Text style={styles.titulo}>Minha agenda</Text>
                    <Pressable accessibilityRole="button" onPress={irParaHoje} style={styles.botaoHoje}>
                        <Text style={styles.textoBotaoHoje}>Hoje</Text>
                    </Pressable>
                </View>

                <View style={styles.navegadorSemana}>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Semana anterior"
                        onPress={() => mudarSemana(-1)}
                        style={styles.botaoSemana}
                    >
                        <Text style={styles.setaSemana}>‹</Text>
                    </Pressable>
                    <Text style={styles.intervaloSemana}>{formatarIntervaloSemana(inicioSemana, adicionarDias(inicioSemana, 7))}</Text>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Próxima semana"
                        onPress={() => mudarSemana(1)}
                        style={styles.botaoSemana}
                    >
                        <Text style={styles.setaSemana}>›</Text>
                    </Pressable>
                </View>

                <View accessibilityRole="tablist" style={styles.seletorDias}>
                    {Array.from({ length: 7 }, (_, indice) => adicionarDias(inicioSemana, indice)).map((data) => {
                        const selecionado = data.getTime() === dataSelecionada.getTime();
                        const ehHoje = data.getTime() === hoje.getTime();

                        return (
                            <Pressable
                                key={data.toISOString()}
                                accessibilityRole="tab"
                                accessibilityState={{ selected: selecionado }}
                                accessibilityLabel={`${formatarDiaSemana(data)}, ${data.getUTCDate()}`}
                                onPress={() => setDataSelecionada(data)}
                                style={[styles.itemDia, selecionado && styles.itemDiaSelecionado]}
                            >
                                <Text style={[styles.nomeDia, selecionado && styles.textoDiaSelecionado]}>{formatarDiaSemana(data)}</Text>
                                <Text style={[styles.numeroDia, ehHoje && styles.numeroHoje, selecionado && styles.textoDiaSelecionado]}>
                                    {data.getUTCDate()}
                                </Text>
                            </Pressable>
                        );
                    })}
                </View>

                <View style={styles.cabecalhoLista}>
                    <View>
                        <Text style={styles.tituloLista}>Atendimentos do dia</Text>
                        <Text style={styles.dataSelecionada}>
                            {new Intl.DateTimeFormat('pt-BR', {
                                weekday: 'long', day: 'numeric', month: 'long', timeZone: 'UTC',
                            }).format(dataSelecionada)}
                        </Text>
                    </View>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Atualizar agenda"
                        onPress={() => void carregarAgenda(true)}
                        style={styles.botaoAtualizar}
                    >
                        <Text style={styles.iconeAtualizar}>↻</Text>
                    </Pressable>
                </View>

                {state.profile.profissionalUuid === null ? (
                    <View style={styles.cartaoMensagem}>
                        <Text style={styles.textoErro}>Seu usuário ainda não está vinculado a um profissional.</Text>
                    </View>
                ) : carregando ? (
                    <View style={styles.cartaoCarregando}><ActivityIndicator color={cores.marca} /></View>
                ) : erro ? (
                    <View style={styles.cartaoMensagem}>
                        <Text style={styles.textoErro}>{erro}</Text>
                        <Pressable accessibilityRole="button" onPress={() => void carregarAgenda()} style={styles.botaoTentarNovamente}>
                            <Text style={styles.textoTentarNovamente}>Tentar novamente</Text>
                        </Pressable>
                    </View>
                ) : eventosDoDia.length === 0 ? (
                    <View style={styles.cartaoVazio}>
                        <Text style={styles.iconeVazio}>▦</Text>
                        <Text style={styles.tituloVazio}>Agenda livre neste dia</Text>
                        <Text style={styles.textoVazio}>Não há atendimentos ou indisponibilidades para esta data.</Text>
                    </View>
                ) : (
                    <View style={styles.listaEventos}>
                        {eventosDoDia.map((evento) => {
                            const indisponibilidade = evento.tipo === 'indisponibilidade';
                            const varianteStatus = obterVarianteStatus(evento.statusAgendamento, indisponibilidade);
                            const rotuloStatus = indisponibilidade
                                ? 'Indisponível'
                                : evento.statusAgendamento
                                    ? rotulosStatus[evento.statusAgendamento]
                                    : 'Status não informado';
                            return (
                                <View
                                    key={evento.id}
                                    style={[styles.cartaoEvento, indisponibilidade && styles.cartaoIndisponibilidade]}
                                >
                                    <View style={styles.horarioEvento}>
                                        <Text style={styles.horaEvento}>{formatarHora(evento.inicio)}</Text>
                                        <Text style={styles.duracaoEvento}>{formatarDuracao(evento.inicio, evento.fim)}</Text>
                                    </View>
                                    <View style={[styles.indicadorEvento, indisponibilidade && styles.indicadorIndisponibilidade]} />
                                    <View style={styles.detalhesEvento}>
                                        <Text style={styles.tituloEvento} numberOfLines={1}>
                                            {indisponibilidade ? evento.titulo : evento.nomeCliente ?? evento.titulo}
                                        </Text>
                                        <Text style={styles.subtituloEvento} numberOfLines={1}>
                                            {indisponibilidade
                                                ? 'Indisponibilidade'
                                                : evento.nomeProcedimento ?? evento.titulo}
                                        </Text>
                                    </View>
                                    <View style={[styles.etiquetaEvento, estilosEtiquetaPorVariante[varianteStatus]]}>
                                        <Text style={[styles.textoEtiqueta, estilosTextoPorVariante[varianteStatus]]}>
                                            {rotuloStatus}
                                        </Text>
                                    </View>
                                </View>
                            );
                        })}
                    </View>
                )}
            </ScrollView>
            <NavegacaoInferior abaAtiva="agenda" />
        </SafeAreaView>
    );
}
