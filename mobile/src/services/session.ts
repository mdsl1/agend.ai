import * as SecureStore from 'expo-secure-store';

const SESSION_KEY = 'session_token';
const CLINIC_UUID_KEY = 'clinic_uuid';
const CLINIC_NAME_KEY = 'clinic_name';

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

    getClinicName() {
        return SecureStore.getItemAsync(CLINIC_NAME_KEY);
    },

    async saveClinic(clinicUuid: string, clinicName: string) {
        await SecureStore.setItemAsync(CLINIC_UUID_KEY, clinicUuid);
        await SecureStore.setItemAsync(CLINIC_NAME_KEY, clinicName);
    },

    async clearClinic() {
        await SecureStore.deleteItemAsync(SESSION_KEY);
        await SecureStore.deleteItemAsync(CLINIC_UUID_KEY);
        await SecureStore.deleteItemAsync(CLINIC_NAME_KEY);
    },
};
