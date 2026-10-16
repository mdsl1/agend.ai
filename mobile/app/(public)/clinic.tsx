import { useEffect, useState } from 'react';
import { useRouter } from 'expo-router';
import {
    ActivityIndicator,
    Image,
    Alert,
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
import { useAuth } from '../../src/features/auth/AuthProvider';
import { ApiError } from '../../src/services/api';
import { resolveClinic } from '../../src/features/auth/authApi';

export default function ClinicScreen() {
    const { state, saveClinic } = useAuth();
    const router = useRouter();
    const [clinicCode, setClinicCode] = useState('');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        if (state.status === 'signedIn') {
            router.replace('/');
        }
    }, [router, state.status]);

    async function handleSubmit() {
        const normalizedCode = clinicCode.trim();
        if (!normalizedCode) {
            setErrorMessage('Informe o código de identificação da clínica.');
            return;
        }

        setIsSubmitting(true);
        setErrorMessage(null);
        try {
            const clinic = await resolveClinic(normalizedCode);
            await saveClinic(clinic.uuid, clinic.nome);
            router.replace('/(public)/login');
        } catch (error) {
            if (error instanceof ApiError && error.status === 404) {
                Alert.alert(
                    'Código não encontrado',
                    'Não encontramos uma clínica com esse código. Informe um código válido e tente novamente.',
                );
            } else if (error instanceof ApiError && error.status === 400) {
                setErrorMessage('O código informado é inválido. Confira e tente novamente.');
            } else {
                setErrorMessage('Não foi possível verificar a clínica. Confira sua conexão e tente novamente.');
            }
        } finally {
            setIsSubmitting(false);
        }
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
                    <Text style={styles.brandName}>Agend.AI Profissional</Text>
                    <Text style={styles.title}>Identifique sua clínica</Text>
                    <Text style={styles.subtitle}>
                        Informe o código de identificação fornecido pela clínica para continuar.
                    </Text>
                    <Text style={styles.label}>Código da clínica</Text>
                    <TextInput
                        accessibilityLabel="Código da clínica"
                        autoCapitalize="none"
                        autoCorrect={false}
                        onChangeText={setClinicCode}
                        placeholder="Ex.: VITALITY"
                        placeholderTextColor="#849398"
                        returnKeyType="done"
                        onSubmitEditing={() => void handleSubmit()}
                        value={clinicCode}
                        style={styles.input}
                    />
                    {errorMessage ? <Text accessibilityRole="alert" style={styles.error}>{errorMessage}</Text> : null}
                    <Pressable
                        accessibilityRole="button"
                        accessibilityState={{ disabled: isSubmitting }}
                        disabled={isSubmitting}
                        onPress={() => void handleSubmit()}
                        style={({ pressed }) => [styles.submitButton, pressed && !isSubmitting ? styles.pressed : null]}
                    >
                        {isSubmitting ? <ActivityIndicator color="#FFFFFF" /> : <Text style={styles.submitText}>Continuar</Text>}
                    </Pressable>
                </View>
                </ScrollView>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: { flex: 1, backgroundColor: '#F6FAFB' },
    keyboardArea: { flex: 1 },
    scrollContent: { flexGrow: 1, justifyContent: 'center' },
    container: { width: '100%', maxWidth: 480, alignSelf: 'center', paddingHorizontal: 24, paddingVertical: 28 },
    brandMark: { width: 48, height: 48, alignItems: 'center', justifyContent: 'center', borderRadius: 16, backgroundColor: '#007D8D' },
    brandMarkText: { color: '#FFFFFF', fontSize: 25, fontWeight: '800' },
    brandLogo: {
        width: 48,
        height: 48,
        borderRadius: 16,
    },
    brandName: { marginTop: 12, color: '#007D8D', fontSize: 17, fontWeight: '700' },
    title: { marginTop: 30, color: '#16252A', fontSize: 29, fontWeight: '700' },
    subtitle: { marginTop: 8, marginBottom: 28, color: '#53656B', fontSize: 16, lineHeight: 23 },
    label: { marginTop: 14, marginBottom: 8, color: '#25363B', fontSize: 15, fontWeight: '600' },
    input: { minHeight: 52, paddingHorizontal: 15, borderWidth: 1, borderColor: '#C9D7DA', borderRadius: 12, backgroundColor: '#FFFFFF', color: '#16252A', fontSize: 15 },
    error: { marginTop: 14, color: '#B42318', fontSize: 14, lineHeight: 20 },
    submitButton: { minHeight: 52, marginTop: 30, alignItems: 'center', justifyContent: 'center', borderRadius: 12, backgroundColor: '#007D8D' },
    pressed: { backgroundColor: '#006977' },
    submitText: { color: '#FFFFFF', fontSize: 16, fontWeight: '700' },
});
