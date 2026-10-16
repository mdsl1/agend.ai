const fs = require('node:fs');
const path = require('node:path');

const repositoryEnvPath = path.resolve(__dirname, '..', '.env');

function loadPublicApiUrl() {
    if (!fs.existsSync(repositoryEnvPath)) {
        return;
    }

    const envFile = fs.readFileSync(repositoryEnvPath, 'utf8');
    const apiUrlLine = envFile
        .split(/\r?\n/)
        .find((line) => /^\s*EXPO_PUBLIC_API_URL\s*=/.test(line));

    if (!apiUrlLine) {
        return;
    }

    const value = apiUrlLine.slice(apiUrlLine.indexOf('=') + 1).trim();
    const unquotedValue = value.replace(/^("|')(.*)\1$/, '$2');

    // Expo embeds EXPO_PUBLIC_* values in the client bundle; load only this safe URL.
    process.env.EXPO_PUBLIC_API_URL = unquotedValue;
}

module.exports = ({ config }) => {
    loadPublicApiUrl();
    return config;
};
