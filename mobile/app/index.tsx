import { Pressable, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuth } from '../src/features/auth/AuthProvider';

export default function HomeScreen() {
    const { state, signOut, changeClinic } = useAuth();

    if (state.status !== 'signedIn') {
        return null;
    }

    const { profile } = state;

    return (
        <SafeAreaView style={styles.safeArea}>
            <View style={styles.container}>
                <Text style={styles.brand}>Agend.AI Profissional</Text>
                <Text style={styles.eyebrow}>SESSÃO CONECTADA</Text>
                <Text style={styles.title}>
                    Olá, {profile.prefixo ? `${profile.prefixo} ` : ''}{profile.nome}
                </Text>
                <Text style={styles.description}>
                    O aplicativo confirmou sua identidade consultando a rota segura GET /api/me.
                </Text>

                <View style={styles.card}>
                    <Text style={styles.cardLabel}>Clínica</Text>
                    <Text style={styles.cardValue}>{profile.clinica.nome}</Text>
                    {profile.especialidade ? (
                        <>
                            <Text style={[styles.cardLabel, styles.secondLabel]}>Especialidade</Text>
                            <Text style={styles.cardValue}>{profile.especialidade}</Text>
                        </>
                    ) : null}
                    <Text style={[styles.cardLabel, styles.secondLabel]}>Conta</Text>
                    <Text style={styles.cardValue}>{profile.email}</Text>
                </View>

                <Text style={styles.permissionTitle}>Permissões recebidas</Text>
                {profile.permissoes.map((permission) => (
                    <Text key={permission} style={styles.permission}>
                        • {permission}
                    </Text>
                ))}

                <Pressable
                    accessibilityRole="button"
                    onPress={() => void signOut()}
                    style={styles.signOutButton}
                >
                    <Text style={styles.signOutText}>Sair da conta</Text>
                </Pressable>
                <Pressable accessibilityRole="button" onPress={() => void changeClinic()} style={styles.changeClinicButton}>
                    <Text style={styles.changeClinicText}>Trocar clínica</Text>
                </Pressable>
            </View>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: {
        flex: 1,
        backgroundColor: '#F6FAFB',
    },
    container: {
        flex: 1,
        paddingHorizontal: 24,
        paddingTop: 24,
    },
    brand: {
        color: '#007D8D',
        fontSize: 16,
        fontWeight: '700',
    },
    eyebrow: {
        marginTop: 48,
        color: '#16825D',
        fontSize: 12,
        fontWeight: '800',
        letterSpacing: 1.1,
    },
    title: {
        marginTop: 10,
        color: '#16252A',
        fontSize: 29,
        fontWeight: '700',
    },
    description: {
        marginTop: 10,
        color: '#53656B',
        fontSize: 15,
        lineHeight: 22,
    },
    card: {
        marginTop: 28,
        padding: 20,
        borderWidth: 1,
        borderColor: '#D9E5E7',
        borderRadius: 16,
        backgroundColor: '#FFFFFF',
    },
    cardLabel: {
        color: '#68787C',
        fontSize: 13,
    },
    secondLabel: {
        marginTop: 17,
    },
    cardValue: {
        marginTop: 4,
        color: '#16252A',
        fontSize: 16,
        fontWeight: '600',
    },
    permissionTitle: {
        marginTop: 25,
        marginBottom: 9,
        color: '#25363B',
        fontSize: 15,
        fontWeight: '700',
    },
    permission: {
        marginTop: 4,
        color: '#53656B',
        fontSize: 13,
    },
    signOutButton: {
        minHeight: 50,
        marginTop: 'auto',
        marginBottom: 16,
        alignItems: 'center',
        justifyContent: 'center',
        borderWidth: 1,
        borderColor: '#C9D7DA',
        borderRadius: 12,
        backgroundColor: '#FFFFFF',
    },
    signOutText: {
        color: '#007D8D',
        fontSize: 16,
        fontWeight: '700',
    },
    changeClinicButton: { minHeight: 48, alignItems: 'center', justifyContent: 'center' },
    changeClinicText: { color: '#007D8D', fontSize: 14, fontWeight: '600' },
});
