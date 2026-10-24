import { useMemo } from 'react';
import { Stack } from 'expo-router';
import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { StatusBar } from 'expo-status-bar';
import { AuthProvider, useAuth } from '../src/features/auth/AuthProvider';
import { useTema, type CoresTema } from '../src/tema/tema';

function RootNavigator() {
    const { state, retryRestore, signOut } = useAuth();
    const { cores, modo } = useTema();
    const styles = useMemo(() => criarEstilos(cores), [cores]);

    if (state.status === 'loading') {
        return (
            <SafeAreaView style={styles.centered}>
                <StatusBar style={modo === 'escuro' ? 'light' : 'dark'} />
                <ActivityIndicator size="large" color={cores.marca} />
                <Text style={styles.statusText}>Verificando sua sessão…</Text>
            </SafeAreaView>
        );
    }

    if (state.status === 'error') {
        return (
            <SafeAreaView style={styles.centered}>
                <StatusBar style={modo === 'escuro' ? 'light' : 'dark'} />
                <Text style={styles.title}>Não foi possível restaurar a sessão</Text>
                <Text style={styles.message}>{state.message}</Text>
                <Pressable
                    accessibilityRole="button"
                    onPress={() => void retryRestore()}
                    style={styles.primaryButton}
                >
                    <Text style={styles.primaryButtonText}>Tentar novamente</Text>
                </Pressable>
                <Pressable
                    accessibilityRole="button"
                    onPress={() => void signOut()}
                    style={styles.secondaryButton}
                >
                    <Text style={styles.secondaryButtonText}>Sair da conta</Text>
                </Pressable>
            </SafeAreaView>
        );
    }

    return (
        <>
            <StatusBar style={modo === 'escuro' ? 'light' : 'dark'} />
            <Stack screenOptions={{ headerShown: false }}>
                <Stack.Screen name="(public)/clinic" />
                <Stack.Screen name="(public)/login" />
                <Stack.Screen name="index" />
                <Stack.Screen name="agenda" />
                <Stack.Screen name="receitas" />
            </Stack>
        </>
    );
}

export default function RootLayout() {
    return (
        <AuthProvider>
            <RootNavigator />
        </AuthProvider>
    );
}

function criarEstilos(cores: CoresTema) {
    return StyleSheet.create({
        centered: { flex: 1, alignItems: 'center', justifyContent: 'center', paddingHorizontal: 28, backgroundColor: cores.fundo },
        statusText: { marginTop: 16, color: cores.textoSuave, fontSize: 16 },
        title: { color: cores.texto, fontSize: 23, fontWeight: '700', textAlign: 'center' },
        message: { marginTop: 12, marginBottom: 24, color: cores.textoSuave, fontSize: 16, lineHeight: 23, textAlign: 'center' },
        primaryButton: { minHeight: 48, alignSelf: 'stretch', alignItems: 'center', justifyContent: 'center', borderRadius: 12, backgroundColor: '#007D8D' },
        primaryButtonText: { color: cores.textoSobreMarca, fontSize: 16, fontWeight: '700' },
        secondaryButton: { minHeight: 48, marginTop: 8, alignSelf: 'stretch', alignItems: 'center', justifyContent: 'center' },
        secondaryButtonText: { color: cores.marca, fontSize: 16, fontWeight: '600' },
    });
}
