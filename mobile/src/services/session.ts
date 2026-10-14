import * as SecureStore from 'expo-secure-store';

const SESSION_KEY = 'session_token';
const CLINIC_UUID_KEY = 'clinic_uuid';

export const sessionService = {
    getToken() {
        return SecureStore.getItemAsync(SESSION_KEY);
    },

    saveToken(token: string) {
        return SecureStore.setItemAsync(SESSION_KEY, token);
    },

    clearToken() {
        return SecureStore.deleteItemAsync(SESSION_KEY);
    },

    getClinicUuid() {
        return SecureStore.getItemAsync(CLINIC_UUID_KEY);
    },

    saveClinicUuid(clinicUuid: string) {
        return SecureStore.setItemAsync(CLINIC_UUID_KEY, clinicUuid);
    },

    async clearClinic() {
        await SecureStore.deleteItemAsync(SESSION_KEY);
        await SecureStore.deleteItemAsync(CLINIC_UUID_KEY);
    },
};
