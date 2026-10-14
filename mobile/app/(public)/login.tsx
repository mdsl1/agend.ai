import { useState } from 'react';
import {
    ActivityIndicator,
    KeyboardAvoidingView,
    Platform,
    Pressable,
    StyleSheet,
    Text,
    TextInput,
    View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ApiError } from '../../src/services/api';
import { useAuth } from '../../src/features/auth/AuthProvider';

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
    const { signIn, changeClinic } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

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

    return (
        <SafeAreaView style={styles.safeArea}>
            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : undefined}
                style={styles.keyboardArea}
            >
                <View style={styles.container}>
                    <View style={styles.brandMark}>
                        <Text style={styles.brandMarkText}>A</Text>
                    </View>
                    <Text style={styles.brandName}>Agend.AI</Text>
                    <Text style={styles.title}>Acesse sua conta</Text>
                    <Text style={styles.subtitle}>
                        Entre para consultar sua agenda e gerenciar sua rotina profissional.
                    </Text>

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

                    <Pressable accessibilityRole="button" onPress={() => void changeClinic()} style={styles.changeClinicButton}>
                        <Text style={styles.changeClinicText}>Trocar clínica</Text>
                    </Pressable>
                </View>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: {
        flex: 1,
        backgroundColor: '#F6FAFB',
    },
    keyboardArea: {
        flex: 1,
        justifyContent: 'center',
    },
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
        marginTop: 24,
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
