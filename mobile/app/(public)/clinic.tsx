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
import { useAuth } from '../../src/features/auth/AuthProvider';

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export default function ClinicScreen() {
    const { saveClinic } = useAuth();
    const [clinicUuid, setClinicUuid] = useState('');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    async function handleSubmit() {
        const normalizedUuid = clinicUuid.trim();
        if (!UUID_PATTERN.test(normalizedUuid)) {
            setErrorMessage('Informe um UUID de clínica válido.');
            return;
        }

        setIsSubmitting(true);
        setErrorMessage(null);
        try {
            await saveClinic(normalizedUuid);
        } catch {
            setErrorMessage('Não foi possível salvar a clínica. Tente novamente.');
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <SafeAreaView style={styles.safeArea}>
            <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.keyboardArea}>
                <View style={styles.container}>
                    <View style={styles.brandMark}><Text style={styles.brandMarkText}>A</Text></View>
                    <Text style={styles.brandName}>Agend.AI Profissional</Text>
                    <Text style={styles.title}>Identifique sua clínica</Text>
                    <Text style={styles.subtitle}>
                        Informe o UUID da clínica para continuar. No futuro, esta etapa poderá usar um código de identificação.
                    </Text>
                    <Text style={styles.label}>UUID da clínica</Text>
                    <TextInput
                        accessibilityLabel="UUID da clínica"
                        autoCapitalize="none"
                        autoCorrect={false}
                        onChangeText={setClinicUuid}
                        placeholder="00000000-0000-0000-0000-000000000000"
                        placeholderTextColor="#849398"
                        returnKeyType="done"
                        value={clinicUuid}
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
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: { flex: 1, backgroundColor: '#F6FAFB' },
    keyboardArea: { flex: 1, justifyContent: 'center' },
    container: { width: '100%', maxWidth: 480, alignSelf: 'center', paddingHorizontal: 24, paddingVertical: 28 },
    brandMark: { width: 48, height: 48, alignItems: 'center', justifyContent: 'center', borderRadius: 16, backgroundColor: '#007D8D' },
    brandMarkText: { color: '#FFFFFF', fontSize: 25, fontWeight: '800' },
    brandName: { marginTop: 12, color: '#007D8D', fontSize: 17, fontWeight: '700' },
    title: { marginTop: 30, color: '#16252A', fontSize: 29, fontWeight: '700' },
    subtitle: { marginTop: 8, marginBottom: 28, color: '#53656B', fontSize: 16, lineHeight: 23 },
    label: { marginTop: 14, marginBottom: 8, color: '#25363B', fontSize: 15, fontWeight: '600' },
    input: { minHeight: 52, paddingHorizontal: 15, borderWidth: 1, borderColor: '#C9D7DA', borderRadius: 12, backgroundColor: '#FFFFFF', color: '#16252A', fontSize: 15 },
    error: { marginTop: 14, color: '#B42318', fontSize: 14, lineHeight: 20 },
    submitButton: { minHeight: 52, marginTop: 24, alignItems: 'center', justifyContent: 'center', borderRadius: 12, backgroundColor: '#007D8D' },
    pressed: { backgroundColor: '#006977' },
    submitText: { color: '#FFFFFF', fontSize: 16, fontWeight: '700' },
});
