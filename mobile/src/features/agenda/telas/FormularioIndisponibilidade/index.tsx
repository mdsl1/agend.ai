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
import { criarMinhaIndisponibilidade, excluirMinhaIndisponibilidade } from '../../apiAgenda';
import type { FrequenciaIndisponibilidade } from '../../typeAgenda';
import { criarEstilos } from './styles';

const FUSO_HORARIO = 'America/Sao_Paulo';
const TAMANHO_MAXIMO_MOTIVO = 150;

const OPCOES_FREQUENCIA: ReadonlyArray<{ valor: FrequenciaIndisponibilidade; rotulo: string }> = [
    { valor: 'Daily', rotulo: 'Diariamente' },
    { valor: 'Weekly', rotulo: 'Semanalmente' },
    { valor: 'Monthly', rotulo: 'Mensalmente' },
    { valor: 'Yearly', rotulo: 'Anualmente' },
];

type PartesData = {
    ano: number;
    mes: number;
    dia: number;
};

type PartesHora = {
    hora: number;
    minuto: number;
};

function obterHojeEmSaoPaulo(): PartesData {
    const partes = new Intl.DateTimeFormat('en', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        timeZone: FUSO_HORARIO,
    }).formatToParts(new Date());

    return {
        ano: Number(partes.find((parte) => parte.type === 'year')?.value),
        mes: Number(partes.find((parte) => parte.type === 'month')?.value),
        dia: Number(partes.find((parte) => parte.type === 'day')?.value),
    };
}

function formatarPartesDataParaBr(partes: PartesData): string {
    const dia = String(partes.dia).padStart(2, '0');
    const mes = String(partes.mes).padStart(2, '0');
    const ano = String(partes.ano).padStart(4, '0');
    return `${dia}/${mes}/${ano}`;
}

function interpretarDataParametroIso(valorIso: string | undefined): PartesData | null {
    if (!valorIso) return null;
    const correspondencia = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valorIso.trim());
    if (!correspondencia) return null;

    const ano = Number(correspondencia[1]);
    const mes = Number(correspondencia[2]);
    const dia = Number(correspondencia[3]);
    const dataUtc = new Date(Date.UTC(ano, mes - 1, dia));

    if (
        dataUtc.getUTCFullYear() !== ano ||
        dataUtc.getUTCMonth() + 1 !== mes ||
        dataUtc.getUTCDate() !== dia
    ) {
        return null;
    }

    return { ano, mes, dia };
}

function interpretarDataBr(texto: string): PartesData | null {
    const correspondencia = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(texto.trim());
    if (!correspondencia) return null;

    const dia = Number(correspondencia[1]);
    const mes = Number(correspondencia[2]);
    const ano = Number(correspondencia[3]);
    if (ano < 1900 || ano > 2100) return null;

    const dataUtc = new Date(Date.UTC(ano, mes - 1, dia));
    if (
        dataUtc.getUTCFullYear() !== ano ||
        dataUtc.getUTCMonth() + 1 !== mes ||
        dataUtc.getUTCDate() !== dia
    ) {
        return null;
    }

    return { ano, mes, dia };
}

function interpretarHorario(texto: string): PartesHora | null {
    const correspondencia = /^(\d{2}):(\d{2})$/.exec(texto.trim());
    if (!correspondencia) return null;

    const hora = Number(correspondencia[1]);
    const minuto = Number(correspondencia[2]);
    if (hora < 0 || hora > 23 || minuto < 0 || minuto > 59) return null;

    return { hora, minuto };
}

function aplicarMascaraData(texto: string): string {
    const digitos = texto.replace(/\D/g, '').slice(0, 8);
    if (digitos.length <= 2) return digitos;
    if (digitos.length <= 4) return `${digitos.slice(0, 2)}/${digitos.slice(2)}`;
    return `${digitos.slice(0, 2)}/${digitos.slice(2, 4)}/${digitos.slice(4, 8)}`;
}

function aplicarMascaraHora(texto: string): string {
    const digitos = texto.replace(/\D/g, '').slice(0, 4);
    if (digitos.length <= 2) return digitos;
    return `${digitos.slice(0, 2)}:${digitos.slice(2, 4)}`;
}

function obterDeslocamentoFuso(partesData: PartesData): number {
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

function montarDataHoraIsoComFuso(data: PartesData, horario: PartesHora, segundos = 0): string {
    const deslocamentoMinutos = obterDeslocamentoFuso(data) / 60000;
    const sinal = deslocamentoMinutos >= 0 ? '+' : '-';
    const valorAbsoluto = Math.abs(deslocamentoMinutos);
    const horasOffset = String(Math.floor(valorAbsoluto / 60)).padStart(2, '0');
    const minutosOffset = String(valorAbsoluto % 60).padStart(2, '0');

    const ano = String(data.ano).padStart(4, '0');
    const mes = String(data.mes).padStart(2, '0');
    const dia = String(data.dia).padStart(2, '0');
    const hora = String(horario.hora).padStart(2, '0');
    const minuto = String(horario.minuto).padStart(2, '0');
    const segundo = String(segundos).padStart(2, '0');

    return `${ano}-${mes}-${dia}T${hora}:${minuto}:${segundo}${sinal}${horasOffset}:${minutosOffset}`;
}

function extrairDataBrDeInstanteIso(instanteIso: string | undefined): string {
    if (!instanteIso) return '';
    const data = new Date(instanteIso);
    if (Number.isNaN(data.getTime())) return '';

    return new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        timeZone: FUSO_HORARIO,
    }).format(data);
}

function extrairHoraDeInstanteIso(instanteIso: string | undefined): string {
    if (!instanteIso) return '';
    const data = new Date(instanteIso);
    if (Number.isNaN(data.getTime())) return '';

    return new Intl.DateTimeFormat('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        hourCycle: 'h23',
        timeZone: FUSO_HORARIO,
    }).format(data);
}

function formatarDataExtenso(data: PartesData | null, instanteFallback?: string): string {
    if (data) {
        const dataUtc = new Date(Date.UTC(data.ano, data.mes - 1, data.dia, 12));
        return new Intl.DateTimeFormat('pt-BR', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric',
            timeZone: 'UTC',
        }).format(dataUtc);
    }

    if (instanteFallback) {
        const parsed = new Date(instanteFallback);
        if (!Number.isNaN(parsed.getTime())) {
            return new Intl.DateTimeFormat('pt-BR', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
                year: 'numeric',
                timeZone: FUSO_HORARIO,
            }).format(parsed);
        }
    }

    return '';
}

function calcularLimiteMaximoRepeticao(dataInicio: PartesData, horarioInicio: PartesHora): Date {
    const anoLimite = dataInicio.ano + 1;
    const dataTeste = new Date(Date.UTC(anoLimite, dataInicio.mes - 1, dataInicio.dia));
    const diaAjustado =
        dataTeste.getUTCMonth() + 1 !== dataInicio.mes ? 28 : dataInicio.dia;

    const isoLimite = montarDataHoraIsoComFuso(
        { ano: anoLimite, mes: dataInicio.mes, dia: diaAjustado },
        horarioInicio,
    );
    return new Date(isoLimite);
}

export default function TelaFormularioIndisponibilidade() {
    const parametros = useLocalSearchParams<{
        data?: string;
        eventoId?: string;
        titulo?: string;
        inicio?: string;
        fim?: string;
    }>();
    const router = useRouter();
    const { state, signOut } = useAuth();
    const { cores, modo } = useTema();
    const styles = useMemo(() => criarEstilos(cores, modo === 'escuro'), [cores, modo]);

    const eventoId = typeof parametros.eventoId === 'string' && parametros.eventoId.trim().length > 0
        ? parametros.eventoId.trim()
        : null;
    const modoVisualizacao = Boolean(eventoId);

    const dataInicialCriacao = useMemo(() => {
        const dataParam = typeof parametros.data === 'string' ? parametros.data : undefined;
        return interpretarDataParametroIso(dataParam) ?? obterHojeEmSaoPaulo();
    }, [parametros.data]);

    const [motivo, setMotivo] = useState(() =>
        modoVisualizacao ? (typeof parametros.titulo === 'string' ? parametros.titulo : 'Indisponível') : '',
    );
    const [dataTexto, setDataTexto] = useState(() =>
        modoVisualizacao
            ? extrairDataBrDeInstanteIso(typeof parametros.inicio === 'string' ? parametros.inicio : undefined)
            : formatarPartesDataParaBr(dataInicialCriacao),
    );
    const [horaInicioTexto, setHoraInicioTexto] = useState(() =>
        modoVisualizacao
            ? extrairHoraDeInstanteIso(typeof parametros.inicio === 'string' ? parametros.inicio : undefined)
            : '12:00',
    );
    const [horaFimTexto, setHoraFimTexto] = useState(() =>
        modoVisualizacao
            ? extrairHoraDeInstanteIso(typeof parametros.fim === 'string' ? parametros.fim : undefined)
            : '13:00',
    );
    const [seRepete, setSeRepete] = useState(false);
    const [frequencia, setFrequencia] = useState<FrequenciaIndisponibilidade>('Weekly');
    const [repetirAteTexto, setRepetirAteTexto] = useState('');
    const [campoFocado, setCampoFocado] = useState<'motivo' | 'data' | 'inicio' | 'fim' | 'repetirAte' | null>(null);

    const [enviando, setEnviando] = useState(false);
    const [erroFormulario, setErroFormulario] = useState<string | null>(null);

    useEffect(() => {
        if (state.status !== 'signedOut') return;
        let ativo = true;
        void sessionService.getClinicUuid().then((clinicUuid) => {
            if (ativo) router.replace(clinicUuid ? '/(public)/login' : '/(public)/clinic');
        });
        return () => {
            ativo = false;
        };
    }, [router, state.status]);

    const voltar = useCallback(() => {
        if (router.canGoBack()) router.back();
        else router.replace('/agenda');
    }, [router]);

    const dataInterpretada = useMemo(() => interpretarDataBr(dataTexto), [dataTexto]);
    const descricaoDataExtenso = useMemo(
        () =>
            formatarDataExtenso(
                dataInterpretada,
                typeof parametros.inicio === 'string' ? parametros.inicio : undefined,
            ),
        [dataInterpretada, parametros.inicio],
    );

    async function salvarIndisponibilidade() {
        if (enviando || modoVisualizacao) return;

        const motivoLimpo = motivo.trim();
        if (motivoLimpo.length > TAMANHO_MAXIMO_MOTIVO) {
            setErroFormulario(`O motivo deve ter no máximo ${TAMANHO_MAXIMO_MOTIVO} caracteres.`);
            return;
        }

        const partesData = interpretarDataBr(dataTexto);
        if (!partesData) {
            setErroFormulario('Informe uma data válida no formato DD/MM/AAAA.');
            return;
        }

        const partesHoraInicio = interpretarHorario(horaInicioTexto);
        if (!partesHoraInicio) {
            setErroFormulario('Informe um horário de início válido no formato HH:mm (ex.: 12:00).');
            return;
        }

        const partesHoraFim = interpretarHorario(horaFimTexto);
        if (!partesHoraFim) {
            setErroFormulario('Informe um horário de término válido no formato HH:mm (ex.: 13:30).');
            return;
        }

        const inicioIso = montarDataHoraIsoComFuso(partesData, partesHoraInicio);
        const fimIso = montarDataHoraIsoComFuso(partesData, partesHoraFim);
        const instanteInicio = new Date(inicioIso);
        const instanteFim = new Date(fimIso);

        if (instanteFim.getTime() <= instanteInicio.getTime()) {
            setErroFormulario('O horário de término deve ser posterior ao horário de início.');
            return;
        }

        let repetirAteIso: string | null = null;
        if (seRepete && repetirAteTexto.trim().length > 0) {
            const partesRepetirAte = interpretarDataBr(repetirAteTexto);
            if (!partesRepetirAte) {
                setErroFormulario('Informe uma data final de repetição válida no formato DD/MM/AAAA ou deixe em branco.');
                return;
            }

            const candidatoRepetirAteIso = montarDataHoraIsoComFuso(partesRepetirAte, partesHoraInicio);
            const instanteRepetirAte = new Date(candidatoRepetirAteIso);

            if (instanteRepetirAte.getTime() <= instanteFim.getTime()) {
                setErroFormulario('A data limite de repetição deve ser posterior à data e ao horário final do bloqueio.');
                return;
            }

            const limiteMaximo = calcularLimiteMaximoRepeticao(partesData, partesHoraInicio);
            if (instanteRepetirAte.getTime() > limiteMaximo.getTime()) {
                setErroFormulario('A repetição da indisponibilidade não pode ultrapassar 1 ano a partir da data de início.');
                return;
            }

            repetirAteIso = candidatoRepetirAteIso;
        }

        setEnviando(true);
        setErroFormulario(null);

        try {
            const resposta = await criarMinhaIndisponibilidade({
                inicio: inicioIso,
                fim: fimIso,
                motivo: motivoLimpo.length > 0 ? motivoLimpo : null,
                seRepete,
                frequencia: seRepete ? frequencia : null,
                repetirAte: seRepete ? repetirAteIso : null,
            });

            Alert.alert(
                'Indisponibilidade registrada',
                `${resposta.titulo} foi adicionada à sua agenda.`,
                [{ text: 'OK', onPress: voltar }],
            );
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErroFormulario(
                falha instanceof Error
                    ? falha.message
                    : 'Não foi possível registrar a indisponibilidade agora.',
            );
        } finally {
            setEnviando(false);
        }
    }

    const executarExclusao = useCallback(async () => {
        if (!eventoId || enviando) return;

        setEnviando(true);
        setErroFormulario(null);

        try {
            await excluirMinhaIndisponibilidade(eventoId);
            Alert.alert(
                'Bloqueio removido',
                'O período foi liberado na sua agenda.',
                [{ text: 'OK', onPress: voltar }],
            );
        } catch (falha) {
            if (falha instanceof ApiError && falha.status === 401) {
                await signOut();
                return;
            }
            setErroFormulario(
                falha instanceof Error
                    ? falha.message
                    : 'Não foi possível excluir este bloqueio agora.',
            );
        } finally {
            setEnviando(false);
        }
    }, [enviando, eventoId, signOut, voltar]);

    function confirmarExclusao() {
        if (!eventoId || enviando) return;

        Alert.alert(
            'Excluir bloqueio',
            'Deseja remover esta indisponibilidade e liberar o horário na sua agenda?',
            [
                { text: 'Cancelar', style: 'cancel' },
                {
                    text: 'Excluir bloqueio',
                    style: 'destructive',
                    onPress: () => void executarExclusao(),
                },
            ],
        );
    }

    if (state.status !== 'signedIn') {
        return (
            <SafeAreaView style={styles.centralizado}>
                <ActivityIndicator size="large" color={cores.marca} />
            </SafeAreaView>
        );
    }

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
                            accessibilityLabel="Voltar para a agenda"
                            onPress={voltar}
                            style={styles.botaoVoltar}
                        >
                            <Text style={styles.setaVoltar}>‹</Text>
                        </Pressable>
                        <Text style={styles.tituloCabecalho}>Indisponibilidade</Text>
                    </View>

                    <View style={styles.cartaoResumo}>
                        <View style={styles.iconeResumo}>
                            <Text style={styles.simboloResumo}>⊘</Text>
                        </View>
                        <View style={styles.dadosResumo}>
                            <Text style={styles.tituloResumo}>
                                {modoVisualizacao
                                    ? motivo || 'Indisponível'
                                    : 'Novo bloqueio de agenda'}
                            </Text>
                            <Text style={styles.subtituloResumo}>
                                {modoVisualizacao
                                    ? 'Bloqueio sincronizado com sua agenda'
                                    : 'Reserve um período em que não realizará atendimentos'}
                            </Text>
                        </View>
                        <View style={styles.etiquetaBloqueio}>
                            <Text style={styles.textoEtiquetaBloqueio}>Indisponível</Text>
                        </View>
                    </View>

                    <View style={styles.blocoCampo}>
                        <Text style={styles.rotuloCampo}>Motivo</Text>
                        <View
                            style={[
                                styles.caixaEntrada,
                                modoVisualizacao && styles.caixaEntradaSomenteLeitura,
                                campoFocado === 'motivo' && styles.caixaEntradaFoco,
                            ]}
                        >
                            {modoVisualizacao ? (
                                <Text style={styles.textoSomenteLeitura}>{motivo || 'Indisponível'}</Text>
                            ) : (
                                <TextInput
                                    accessibilityLabel="Motivo da indisponibilidade"
                                    editable={!enviando}
                                    keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                    maxLength={TAMANHO_MAXIMO_MOTIVO}
                                    onBlur={() => setCampoFocado(null)}
                                    onChangeText={(texto) => {
                                        setMotivo(texto);
                                        if (erroFormulario) setErroFormulario(null);
                                    }}
                                    onFocus={() => setCampoFocado('motivo')}
                                    placeholder="Ex.: Almoço, reunião ou compromisso pessoal"
                                    placeholderTextColor={cores.textoPlaceholder}
                                    returnKeyType="next"
                                    value={motivo}
                                    style={styles.entradaTexto}
                                />
                            )}
                        </View>
                        {!modoVisualizacao && (
                            <Text style={styles.ajudaCampo}>
                                Opcional (até {TAMANHO_MAXIMO_MOTIVO} caracteres). Se deixar em branco, será salvo como &quot;Indisponível&quot;.
                            </Text>
                        )}
                    </View>

                    <View style={styles.blocoCampo}>
                        <Text style={styles.rotuloCampo}>Data</Text>
                        <View
                            style={[
                                styles.caixaEntrada,
                                modoVisualizacao && styles.caixaEntradaSomenteLeitura,
                                campoFocado === 'data' && styles.caixaEntradaFoco,
                            ]}
                        >
                            {modoVisualizacao ? (
                                <Text style={styles.textoSomenteLeitura}>{dataTexto || '—'}</Text>
                            ) : (
                                <TextInput
                                    accessibilityLabel="Data da indisponibilidade no formato dia, mês e ano"
                                    editable={!enviando}
                                    keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                    keyboardType="number-pad"
                                    maxLength={10}
                                    onBlur={() => setCampoFocado(null)}
                                    onChangeText={(texto) => {
                                        setDataTexto(aplicarMascaraData(texto));
                                        if (erroFormulario) setErroFormulario(null);
                                    }}
                                    onFocus={() => setCampoFocado('data')}
                                    placeholder="DD/MM/AAAA"
                                    placeholderTextColor={cores.textoPlaceholder}
                                    returnKeyType="next"
                                    value={dataTexto}
                                    style={styles.entradaTexto}
                                />
                            )}
                            <Text accessibilityElementsHidden style={styles.iconeCampo}>📅</Text>
                        </View>
                        {descricaoDataExtenso ? (
                            <Text style={styles.ajudaCampo}>{descricaoDataExtenso}</Text>
                        ) : null}
                    </View>

                    <View style={styles.blocoCampo}>
                        <Text style={styles.rotuloCampo}>Horário</Text>
                        <View style={styles.linhaHorarios}>
                            <View style={styles.colunaHorario}>
                                <Text style={styles.subrotuloHorario}>Início</Text>
                                <View
                                    style={[
                                        styles.caixaEntrada,
                                        modoVisualizacao && styles.caixaEntradaSomenteLeitura,
                                        campoFocado === 'inicio' && styles.caixaEntradaFoco,
                                    ]}
                                >
                                    {modoVisualizacao ? (
                                        <Text style={styles.textoSomenteLeitura}>{horaInicioTexto || '—'}</Text>
                                    ) : (
                                        <TextInput
                                            accessibilityLabel="Horário de início"
                                            editable={!enviando}
                                            keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                            keyboardType="number-pad"
                                            maxLength={5}
                                            onBlur={() => setCampoFocado(null)}
                                            onChangeText={(texto) => {
                                                setHoraInicioTexto(aplicarMascaraHora(texto));
                                                if (erroFormulario) setErroFormulario(null);
                                            }}
                                            onFocus={() => setCampoFocado('inicio')}
                                            placeholder="12:00"
                                            placeholderTextColor={cores.textoPlaceholder}
                                            returnKeyType="next"
                                            value={horaInicioTexto}
                                            style={styles.entradaTexto}
                                        />
                                    )}
                                </View>
                            </View>

                            <View style={styles.colunaHorario}>
                                <Text style={styles.subrotuloHorario}>Término</Text>
                                <View
                                    style={[
                                        styles.caixaEntrada,
                                        modoVisualizacao && styles.caixaEntradaSomenteLeitura,
                                        campoFocado === 'fim' && styles.caixaEntradaFoco,
                                    ]}
                                >
                                    {modoVisualizacao ? (
                                        <Text style={styles.textoSomenteLeitura}>{horaFimTexto || '—'}</Text>
                                    ) : (
                                        <TextInput
                                            accessibilityLabel="Horário de término"
                                            editable={!enviando}
                                            keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                            keyboardType="number-pad"
                                            maxLength={5}
                                            onBlur={() => setCampoFocado(null)}
                                            onChangeText={(texto) => {
                                                setHoraFimTexto(aplicarMascaraHora(texto));
                                                if (erroFormulario) setErroFormulario(null);
                                            }}
                                            onFocus={() => setCampoFocado('fim')}
                                            placeholder="13:30"
                                            placeholderTextColor={cores.textoPlaceholder}
                                            returnKeyType="done"
                                            value={horaFimTexto}
                                            style={styles.entradaTexto}
                                        />
                                    )}
                                </View>
                            </View>
                        </View>
                    </View>

                    {!modoVisualizacao && (
                        <View style={styles.blocoCampo}>
                            <Text style={styles.rotuloCampo}>Repetição</Text>
                            <View style={styles.cartaoRepeticao}>
                                <View style={styles.linhaSwitchRepeticao}>
                                    <View style={styles.textosRepeticao}>
                                        <Text style={styles.tituloRepeticao}>Repetir bloqueio</Text>
                                        <Text style={styles.subtituloRepeticao}>
                                            {seRepete
                                                ? 'Até a data escolhida ou por até 1 ano'
                                                : 'Apenas na data selecionada'}
                                        </Text>
                                    </View>
                                    <Switch
                                        accessibilityLabel="Repetir bloqueio"
                                        disabled={enviando}
                                        onValueChange={(novoValor) => {
                                            setSeRepete(novoValor);
                                            if (erroFormulario) setErroFormulario(null);
                                        }}
                                        thumbColor="#FFFFFF"
                                        trackColor={{ false: cores.bordaForte, true: cores.marca }}
                                        value={seRepete}
                                    />
                                </View>

                                {seRepete && (
                                    <>
                                        <View style={styles.divisorRepeticao} />
                                        <View style={styles.blocoCampo}>
                                            <Text style={styles.rotuloCampo}>Frequência</Text>
                                            <View style={styles.gradeFrequencias}>
                                                {OPCOES_FREQUENCIA.map((opcao) => {
                                                    const selecionado = frequencia === opcao.valor;
                                                    return (
                                                        <Pressable
                                                            key={opcao.valor}
                                                            accessibilityRole="button"
                                                            accessibilityState={{ selected: selecionado, disabled: enviando }}
                                                            disabled={enviando}
                                                            onPress={() => {
                                                                setFrequencia(opcao.valor);
                                                                if (erroFormulario) setErroFormulario(null);
                                                            }}
                                                            style={[
                                                                styles.chipFrequencia,
                                                                selecionado && styles.chipFrequenciaSelecionado,
                                                            ]}
                                                        >
                                                            <Text
                                                                style={[
                                                                    styles.textoChipFrequencia,
                                                                    selecionado && styles.textoChipFrequenciaSelecionado,
                                                                ]}
                                                            >
                                                                {opcao.rotulo}
                                                            </Text>
                                                        </Pressable>
                                                    );
                                                })}
                                            </View>
                                        </View>

                                        <View style={styles.blocoCampo}>
                                            <Text style={styles.rotuloCampo}>Repetir até (opcional)</Text>
                                            <View
                                                style={[
                                                    styles.caixaEntrada,
                                                    campoFocado === 'repetirAte' && styles.caixaEntradaFoco,
                                                ]}
                                            >
                                                <TextInput
                                                    accessibilityLabel="Repetir até a data no formato dia, mês e ano"
                                                    editable={!enviando}
                                                    keyboardAppearance={modo === 'escuro' ? 'dark' : 'light'}
                                                    keyboardType="number-pad"
                                                    maxLength={10}
                                                    onBlur={() => setCampoFocado(null)}
                                                    onChangeText={(texto) => {
                                                        setRepetirAteTexto(aplicarMascaraData(texto));
                                                        if (erroFormulario) setErroFormulario(null);
                                                    }}
                                                    onFocus={() => setCampoFocado('repetirAte')}
                                                    placeholder="DD/MM/AAAA"
                                                    placeholderTextColor={cores.textoPlaceholder}
                                                    returnKeyType="done"
                                                    value={repetirAteTexto}
                                                    style={styles.entradaTexto}
                                                />
                                                <Text accessibilityElementsHidden style={styles.iconeCampo}>📅</Text>
                                            </View>
                                            <Text style={styles.ajudaCampo}>
                                                Se deixar em branco, o bloqueio se repetirá por até 1 ano (ou até você remover o bloqueio).
                                            </Text>
                                        </View>
                                    </>
                                )}
                            </View>
                        </View>
                    )}

                    <View style={styles.avisoInformativo}>
                        <Text style={styles.textoAvisoInformativo}>
                            {modoVisualizacao
                                ? 'Ao excluir este bloqueio, o evento é removido do Google Calendar e o horário volta a ficar livre.'
                                : 'A API verifica conflitos na sua agenda antes de confirmar a criação do bloqueio no Google Calendar.'}
                        </Text>
                    </View>

                    {erroFormulario ? (
                        <View accessibilityRole="alert" style={styles.erroFormulario}>
                            <Text style={styles.textoErroFormulario}>{erroFormulario}</Text>
                        </View>
                    ) : null}

                    <View style={styles.rodapeAcoes}>
                        {modoVisualizacao ? (
                            <Pressable
                                accessibilityRole="button"
                                accessibilityState={{ disabled: enviando }}
                                disabled={enviando}
                                onPress={confirmarExclusao}
                                style={({ pressed }) => [
                                    styles.botaoExcluir,
                                    pressed && !enviando && styles.botaoExcluirPressionado,
                                    enviando && styles.botaoDesabilitado,
                                ]}
                            >
                                {enviando ? (
                                    <ActivityIndicator color={cores.perigoStatus} />
                                ) : (
                                    <Text style={styles.textoBotaoExcluir}>Excluir bloqueio</Text>
                                )}
                            </Pressable>
                        ) : (
                            <Pressable
                                accessibilityRole="button"
                                accessibilityState={{ disabled: enviando }}
                                disabled={enviando}
                                onPress={() => void salvarIndisponibilidade()}
                                style={({ pressed }) => [
                                    styles.botaoSalvar,
                                    pressed && !enviando && styles.botaoSalvarPressionado,
                                    enviando && styles.botaoDesabilitado,
                                ]}
                            >
                                {enviando ? (
                                    <ActivityIndicator color={cores.textoSobreMarca} />
                                ) : (
                                    <Text style={styles.textoBotaoSalvar}>Salvar indisponibilidade</Text>
                                )}
                            </Pressable>
                        )}
                    </View>
                </ScrollView>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}
