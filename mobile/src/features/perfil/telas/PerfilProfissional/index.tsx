import { useCallback, useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Alert, Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import NavegacaoInferior from '../../../../components/NavegacaoInferior';
import { ApiError } from '../../../../services/api';
import { sessionService } from '../../../../services/session';
import { useTema } from '../../../../tema/tema';
import { useAuth } from '../../../auth/AuthProvider';
import { getMyProfile } from '../../../auth/authApi';
import type { MeuPerfil } from '../../../auth/authTypes';
import { criarEstilos } from './styles';

export function obterIniciais(nome: string): string {
    const partes = nome
        .trim()
        .split(/\s+/)
        .filter((parte) => parte.length > 0);

    if (partes.length === 0) return 'PR';
    if (partes.length === 1) return partes[0].slice(0, 2).toUpperCase();

    return `${partes[0][0]}${partes[1][0]}`.toUpperCase();
}

export function formatarNomeExibicao(nome: string, prefixo: string | null): string {
    const nomeLimpo = nome.trim();
    const prefixoLimpo = prefixo?.trim() ?? '';

    if (!prefixoLimpo) return nomeLimpo;
    return `${prefixoLimpo} ${nomeLimpo}`;
}

export function formatarEspecialidadeRegistro(
    especialidade: string | null,
    registroProfissional: string | null,
): string | null {
    const partes = [especialidade?.trim(), registroProfissional?.trim()].filter(
        (parte): parte is string => Boolean(parte && parte.length > 0),
    );

    if (partes.length === 0) return null;
    return partes.join(' · ');
}

function obterMensagemErro(erro: unknown): string {
    if (erro instanceof ApiError && erro.status === 401) {
        return 'Sua sessão expirou. Entre novamente para continuar.';
    }
    if (erro instanceof Error) return erro.message;
    return 'Não foi possível atualizar os dados do seu perfil agora.';
}

export default function TelaPerfilProfissional() {
    const { state, signOut } = useAuth();
    const router = useRouter();
    const { cores, modo } = useTema();
    const styles = useMemo(() => criarEstilos(cores, modo === 'escuro'), [cores, modo]);

    const [perfilAtualizado, setPerfilAtualizado] = useState<MeuPerfil | null>(null);
    const [carregando, setCarregando] = useState(false);
    const [atualizando, setAtualizando] = useState(false);
    const [saindo, setSaindo] = useState(false);
    const [erro, setErro] = useState<string | null>(null);

    const perfil = perfilAtualizado ?? (state.status === 'signedIn' ? state.profile : null);

    const carregarPerfil = useCallback(
        async (atualizacaoManual = false) => {
            if (atualizacaoManual) setAtualizando(true);
            else setCarregando(true);
            setErro(null);

            try {
                const resposta = await getMyProfile();
                setPerfilAtualizado(resposta);
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
        },
        [signOut],
    );

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

    const confirmarSaida = useCallback(() => {
        if (saindo) return;

        Alert.alert(
            'Sair da conta',
            'Deseja encerrar sua sessão neste dispositivo?',
            [
                { text: 'Cancelar', style: 'cancel' },
                {
                    text: 'Sair da conta',
                    style: 'destructive',
                    onPress: () => {
                        setSaindo(true);
                        void signOut().finally(() => setSaindo(false));
                    },
                },
            ],
        );
    }, [saindo, signOut]);

    if (state.status !== 'signedIn' && !perfil) {
        return (
            <SafeAreaView style={styles.centralizado}>
                <ActivityIndicator size="large" color={cores.marca} />
            </SafeAreaView>
        );
    }

    const especialidadeRegistro = perfil
        ? formatarEspecialidadeRegistro(perfil.especialidade, perfil.registroProfissional)
        : null;

    return (
        <SafeAreaView style={styles.tela} edges={['top', 'left', 'right', 'bottom']}>
            <ScrollView
                contentContainerStyle={styles.conteudo}
                refreshControl={
                    <RefreshControl
                        refreshing={atualizando}
                        onRefresh={() => void carregarPerfil(true)}
                        tintColor={cores.marca}
                    />
                }
            >
                <View style={styles.cabecalho}>
                    <Text style={styles.titulo}>Perfil</Text>
                    <Text style={styles.subtitulo}>Conta profissional e sessão no aplicativo</Text>
                </View>

                {carregando && !perfil ? (
                    <View style={styles.estado}>
                        <ActivityIndicator size="large" color={cores.marca} />
                        <Text style={styles.textoEstado}>Carregando perfil…</Text>
                    </View>
                ) : !perfil && erro ? (
                    <View style={styles.estado}>
                        <Text style={[styles.tituloEstado, styles.textoErro]}>Não foi possível carregar o perfil</Text>
                        <Text style={styles.textoEstado}>{erro}</Text>
                        <Pressable
                            accessibilityRole="button"
                            onPress={() => void carregarPerfil()}
                            style={styles.botaoTentarNovamente}
                        >
                            <Text style={styles.textoTentarNovamente}>Tentar novamente</Text>
                        </Pressable>
                    </View>
                ) : perfil ? (
                    <>
                        {erro ? (
                            <View style={styles.estado}>
                                <Text style={[styles.tituloEstado, styles.textoErro]}>
                                    Não foi possível atualizar os dados
                                </Text>
                                <Text style={styles.textoEstado}>{erro}</Text>
                                <Pressable
                                    accessibilityRole="button"
                                    disabled={carregando || atualizando}
                                    onPress={() => void carregarPerfil()}
                                    style={styles.botaoTentarNovamente}
                                >
                                    {carregando ? (
                                        <ActivityIndicator color={cores.textoSobreMarca} />
                                    ) : (
                                        <Text style={styles.textoTentarNovamente}>Tentar novamente</Text>
                                    )}
                                </Pressable>
                            </View>
                        ) : null}

                        <View style={styles.cartaoIdentificacao}>
                            <View style={styles.linhaIdentificacao}>
                                <View style={styles.avatar}>
                                    <Text style={styles.iniciais}>{obterIniciais(perfil.nome)}</Text>
                                </View>
                                <View style={styles.dadosPrincipais}>
                                    <Text style={styles.nomeExibicao}>
                                        {formatarNomeExibicao(perfil.nome, perfil.prefixo)}
                                    </Text>
                                    {especialidadeRegistro ? (
                                        <Text style={styles.especialidadeRegistro}>{especialidadeRegistro}</Text>
                                    ) : null}
                                    <Text style={styles.nomeClinica}>{perfil.clinica.nome}</Text>
                                </View>
                            </View>

                            <View style={styles.divisorEmail} />

                            <View style={styles.caixaEmail}>
                                <Text style={styles.rotuloEmail}>E-mail profissional</Text>
                                <Text style={styles.valorEmail}>{perfil.email}</Text>
                            </View>
                        </View>

                        <View style={styles.cartaoPrivacidade}>
                            <View style={styles.iconePrivacidade}>
                                <Text accessibilityElementsHidden style={styles.simboloPrivacidade}>
                                    🔒
                                </Text>
                            </View>
                            <View style={styles.textosPrivacidade}>
                                <Text style={styles.tituloPrivacidade}>Privacidade e segurança</Text>
                                <Text style={styles.descricaoPrivacidade}>
                                    Sua sessão é armazenada com proteção nativa no dispositivo e o acesso aos dados segue as diretrizes de privacidade da clínica e da LGPD.
                                </Text>
                            </View>
                        </View>
                    </>
                ) : null}

                <View style={styles.rodapeAcoes}>
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel="Sair da conta"
                        disabled={saindo}
                        onPress={confirmarSaida}
                        style={({ pressed }) => [
                            styles.botaoSair,
                            pressed && styles.botaoSairPressionado,
                            saindo && styles.botaoSairDesabilitado,
                        ]}
                    >
                        {saindo ? (
                            <ActivityIndicator color={modo === 'escuro' ? cores.perigo : '#DC2626'} />
                        ) : (
                            <Text style={styles.textoBotaoSair}>Sair da conta</Text>
                        )}
                    </Pressable>
                </View>
            </ScrollView>
            <NavegacaoInferior abaAtiva="perfil" />
        </SafeAreaView>
    );
}
