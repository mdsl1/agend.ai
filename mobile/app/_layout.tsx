import { Stack } from 'expo-router';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { StatusBar } from 'expo-status-bar';
import { AuthProvider, useAuth } from '../src/features/auth/AuthProvider';

function RootNavigator() {
    const { state, retryRestore, signOut } = useAuth();

    if (state.status === 'loading') {
        return (
            <SafeAreaView style={styles.centered}>
                <StatusBar style="dark" />
                <ActivityIndicator size="large" color="#007D8D" />
                <Text style={styles.statusText}>Verificando sua sessão…</Text>
            </SafeAreaView>
        );
    }

    if (state.status === 'error') {
        return (
            <SafeAreaView style={styles.centered}>
                <StatusBar style="dark" />
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
            <StatusBar style="dark" />
            <Stack screenOptions={{ headerShown: false }}>
                <Stack.Screen name="(public)/clinic" />
                <Stack.Screen name="(public)/login" />
                <Stack.Screen name="index" />
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

const styles = StyleSheet.create({
    centered: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 28,
        backgroundColor: '#F6FAFB',
    },
    statusText: {
        marginTop: 16,
        color: '#53656B',
        fontSize: 16,
    },
    title: {
        color: '#16252A',
        fontSize: 23,
        fontWeight: '700',
        textAlign: 'center',
    },
    message: {
        marginTop: 12,
        marginBottom: 24,
        color: '#53656B',
        fontSize: 16,
        lineHeight: 23,
        textAlign: 'center',
    },
    primaryButton: {
        minHeight: 48,
        alignSelf: 'stretch',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 12,
        backgroundColor: '#007D8D',
    },
    primaryButtonText: {
        color: '#FFFFFF',
        fontSize: 16,
        fontWeight: '700',
    },
    secondaryButton: {
        minHeight: 48,
        marginTop: 8,
        alignSelf: 'stretch',
        alignItems: 'center',
        justifyContent: 'center',
    },
    secondaryButtonText: {
        color: '#007D8D',
        fontSize: 16,
        fontWeight: '600',
    },
});
