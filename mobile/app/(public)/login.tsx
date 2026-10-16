import { useEffect, useState } from 'react';
import { useRouter } from 'expo-router';
import {
    ActivityIndicator,
    Image,
    KeyboardAvoidingView,
    Platform,
    Pressable,
    ScrollView,
    StyleSheet,
    Text,
    TextInput,
    View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../src/services/api';
import { useAuth } from '../../src/features/auth/AuthProvider';
import { sessionService } from '../../src/services/session';

function explainLoginError(error: unknown): string {
    if (error instanceof ApiError && error.status === 401) {
        return 'E-mail ou senha inválidos. Confira os dados e tente novamente.';
    }

    if (error instanceof ApiError && error.status === 403) {
        return 'Esta conta não tem permissão para acessar o aplicativo profissional.';
    }

    if (error instanceof Error) {
        return error.message;
    }

    return 'Não foi possível entrar. Tente novamente.';
}

export default function LoginScreen() {
    const { state, signIn, changeClinic } = useAuth();
    const router = useRouter();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isClinicReady, setIsClinicReady] = useState(false);
    const [clinicName, setClinicName] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        async function loadActiveClinic() {
            try {
                const clinicUuid = await sessionService.getClinicUuid();
                if (!clinicUuid) {
                    router.replace('/(public)/clinic');
                    return;
                }

                const storedName = await sessionService.getClinicName();
                if (active) {
                    setClinicName(storedName);
                    setIsClinicReady(true);
                }
            } catch {
                if (active) router.replace('/(public)/clinic');
            }
        }

        void loadActiveClinic();
        return () => { active = false; };
    }, [router]);

    useEffect(() => {
        if (state.status === 'signedIn') router.replace('/');
    }, [router, state.status]);

    async function handleSubmit() {
        if (!email.trim() || !password) {
            setErrorMessage('Informe seu e-mail e sua senha.');
            return;
        }

        setIsSubmitting(true);
        setErrorMessage(null);

        try {
            await signIn(email, password);
        } catch (error) {
            setErrorMessage(explainLoginError(error));
        } finally {
            setIsSubmitting(false);
        }
    }

    if (!isClinicReady || state.status === 'signedIn') {
        return (
            <SafeAreaView style={[styles.safeArea, styles.loading]}>
                <ActivityIndicator size="large" color="#007D8D" />
            </SafeAreaView>
        );
    }

    return (
        <SafeAreaView style={styles.safeArea}>
            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                keyboardVerticalOffset={0}
                style={styles.keyboardArea}
            >
                <ScrollView
                    contentContainerStyle={styles.scrollContent}
                    keyboardDismissMode="on-drag"
                    keyboardShouldPersistTaps="handled"
                    automaticallyAdjustKeyboardInsets
                >
                <View style={styles.container}>
                    <View style={styles.brandMark}>
                        <Image
                            accessibilityLabel="Logo Agend.AI"
                            source={require('../../assets/agendai-logo.png')}
                            style={styles.brandLogo}
                        />
                    </View>
                    <Text style={styles.brandName}>Agend.AI</Text>
                    <Text style={styles.title}>Acesse sua conta</Text>
                    <Text style={styles.subtitle}>
                        Entre para consultar sua agenda e gerenciar sua rotina profissional.
                    </Text>
                    {clinicName ? (
                        <View style={styles.activeClinic}>
                            <Text style={styles.activeClinicLabel}>Clínica ativa</Text>
                            <Text style={styles.activeClinicName}>{clinicName}</Text>
                        </View>
                    ) : null}

                    <Text style={styles.label}>E-mail</Text>
                    <TextInput
                        accessibilityLabel="E-mail"
                        autoCapitalize="none"
                        autoComplete="email"
                        autoCorrect={false}
                        keyboardType="email-address"
                        onChangeText={setEmail}
                        placeholder="seu@email.com"
                        placeholderTextColor="#849398"
                        returnKeyType="next"
                        textContentType="emailAddress"
                        value={email}
                        style={styles.input}
                    />

                    <Text style={styles.label}>Senha</Text>
                    <TextInput
                        accessibilityLabel="Senha"
                        autoCapitalize="none"
                        autoComplete="current-password"
                        onChangeText={setPassword}
                        onSubmitEditing={() => void handleSubmit()}
                        placeholder="Digite sua senha"
                        placeholderTextColor="#849398"
                        returnKeyType="done"
                        secureTextEntry
                        textContentType="password"
                        value={password}
                        style={styles.input}
                    />

                    {errorMessage ? (
                        <Text accessibilityRole="alert" style={styles.error}>
                            {errorMessage}
                        </Text>
                    ) : null}

                    <Pressable
                        accessibilityRole="button"
                        accessibilityState={{ disabled: isSubmitting }}
                        disabled={isSubmitting}
                        onPress={() => void handleSubmit()}
                        style={({ pressed }) => [
                            styles.submitButton,
                            pressed && !isSubmitting ? styles.submitButtonPressed : null,
                            isSubmitting ? styles.submitButtonDisabled : null,
                        ]}
                    >
                        {isSubmitting ? (
                            <ActivityIndicator color="#FFFFFF" />
                        ) : (
                            <Text style={styles.submitButtonText}>Entrar</Text>
                        )}
                    </Pressable>

                    <Pressable accessibilityRole="button" onPress={() => void changeClinic().then(() => router.replace('/(public)/clinic'))} style={styles.changeClinicButton}>
                        <Text style={styles.changeClinicText}>Trocar clínica</Text>
                    </Pressable>
                </View>
                </ScrollView>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: {
        flex: 1,
        backgroundColor: '#F6FAFB',
    },
    loading: { alignItems: 'center', justifyContent: 'center' },
    keyboardArea: { flex: 1 },
    scrollContent: { flexGrow: 1, justifyContent: 'center' },
    container: {
        width: '100%',
        maxWidth: 480,
        alignSelf: 'center',
        paddingHorizontal: 24,
        paddingVertical: 28,
    },
    brandMark: {
        width: 48,
        height: 48,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 16,
        backgroundColor: '#007D8D',
    },
    brandMarkText: {
        color: '#FFFFFF',
        fontSize: 25,
        fontWeight: '800',
    },
    brandLogo: {
        width: 48,
        height: 48,
        borderRadius: 16,
    },
    brandName: {
        marginTop: 12,
        color: '#007D8D',
        fontSize: 17,
        fontWeight: '700',
    },
    title: {
        marginTop: 30,
        color: '#16252A',
        fontSize: 29,
        fontWeight: '700',
    },
    subtitle: {
        marginTop: 8,
        marginBottom: 28,
        color: '#53656B',
        fontSize: 16,
        lineHeight: 23,
    },
    activeClinic: {
        marginBottom: 12,
        paddingHorizontal: 14,
        paddingVertical: 12,
        borderRadius: 12,
        backgroundColor: '#E7F3F4',
    },
    activeClinicLabel: { color: '#53656B', fontSize: 12 },
    activeClinicName: { marginTop: 3, color: '#007D8D', fontSize: 15, fontWeight: '700' },
    label: {
        marginTop: 14,
        marginBottom: 8,
        color: '#25363B',
        fontSize: 15,
        fontWeight: '600',
    },
    input: {
        minHeight: 52,
        paddingHorizontal: 15,
        borderWidth: 1,
        borderColor: '#C9D7DA',
        borderRadius: 12,
        backgroundColor: '#FFFFFF',
        color: '#16252A',
        fontSize: 16,
    },
    error: {
        marginTop: 14,
        color: '#B42318',
        fontSize: 14,
        lineHeight: 20,
    },
    submitButton: {
        minHeight: 52,
        marginTop: 30,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 12,
        backgroundColor: '#007D8D',
    },
    submitButtonPressed: {
        backgroundColor: '#006977',
    },
    submitButtonDisabled: {
        opacity: 0.7,
    },
    submitButtonText: {
        color: '#FFFFFF',
        fontSize: 16,
        fontWeight: '700',
    },
    footer: {
        marginTop: 24,
        color: '#68787C',
        fontSize: 13,
        lineHeight: 19,
        textAlign: 'center',
    },
    changeClinicButton: {
        minHeight: 48,
        marginTop: 8,
        alignItems: 'center',
        justifyContent: 'center',
    },
    changeClinicText: {
        color: '#007D8D',
        fontSize: 14,
        fontWeight: '600',
    },
});
